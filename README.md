# DCT Steganography pentru imagini BMP

## 1. Scopul proiectului

Acest proiect implementează o metodă de steganografie pentru imagini `.bmp`, folosind transformarea DCT și ascunderea informației prin LSB aplicat pe coeficienți DCT cuantizați.

Metoda nu ascunde mesajul direct în biții pixelilor RGB, ca în varianta clasică LSB în domeniul spațial. În schimb, imaginea este convertită într-un spațiu de culoare de tip `YCbCr`, este împărțită în blocuri de `8x8`, iar pe componentele de crominanță `Cb` și `Cr` se aplică DCT. După această transformare, biții mesajului sunt ascunși în ultimul bit al unor coeficienți DCT cuantizați.

Fluxul general este:

```text
RGB bitmap
    -> împărțire în blocuri 8x8
    -> conversie RGB -> YCbCr
    -> DCT pe Cb și Cr
    -> cuantizare coeficienți DCT
    -> LSB pe coeficient DCT cuantizat
    -> IDCT
    -> reconstrucție YCbCr -> RGB
    -> salvare bitmap modificat
```

La extragere, pașii sunt reluați până la DCT, apoi se citește LSB-ul acelorași coeficienți DCT cuantizați.

---

## 2. Domeniul spațial vs domeniul frecvență

Într-o metodă LSB clasică, mesajul este ascuns direct în valorile pixelilor. Un pixel RGB are trei componente:

```text
R, G, B
```

Fiecare componentă este un număr între `0` și `255`, adică un byte. În LSB clasic, se modifică ultimul bit al uneia dintre aceste componente.

Exemplu:

```text
valoare pixel:  10010110
bit ascuns:             1
valoare nouă:   10010111
```

Aceasta este o metodă în domeniul spațial, deoarece modificarea se face direct în valorile pixelilor.

În acest proiect, mesajul este ascuns în domeniul frecvență. Asta înseamnă că nu lucrăm direct cu pixelii pentru ascundere, ci transformăm blocurile imaginii în coeficienți DCT. Acești coeficienți descriu blocul ca o sumă de unde cosinus cu frecvențe diferite.

Un coeficient DCT indică cât de mult contribuie o anumită frecvență la blocul analizat.

---

## 3. De ce se folosesc blocuri de 8x8

Imaginea este împărțită în blocuri de `8x8` pixeli deoarece DCT este aplicată local, pe blocuri mici. Aceasta este aceeași idee folosită în standardul JPEG.

Un bloc `8x8` conține `64` valori. După DCT, obținem tot o matrice `8x8`, dar valorile nu mai reprezintă pixeli, ci coeficienți de frecvență.

```text
Bloc spațial 8x8        ->        Bloc DCT 8x8
valori de culoare                 coeficienți de frecvență
```

Dacă dimensiunile imaginii nu sunt divizibile cu 8, marginile rămase sunt ignorate. De exemplu, pentru o imagine cu lățimea `1025`, se folosește doar partea până la `1024`, deoarece `1024` este divizibil cu `8`.

În cod:

```csharp
int usableWidth = bitmap.Width - (bitmap.Width % blockSize);
int usableHeight = bitmap.Height - (bitmap.Height % blockSize);
```

---

## 4. Conversia RGB -> YCbCr

Imaginea BMP este citită ca RGB. Pentru fiecare pixel se extrag componentele:

```csharp
double red = pixelColor.R;
double green = pixelColor.G;
double blue = pixelColor.B;
```

Apoi pixelul este convertit în `YCbCr`:

```csharp
double yValue = 0.299 * red + 0.587 * green + 0.114 * blue;
double cbValue = -0.169 * red - 0.331 * green + 0.500 * blue + 128.0;
double crValue = 0.500 * red - 0.419 * green - 0.081 * blue + 128.0;
```

Componentele au următoarea semnificație:

| Componentă | Semnificație |
|---|---|
| `Y` | Luma, adică informația de luminozitate |
| `Cb` | Diferența față de componenta albastră |
| `Cr` | Diferența față de componenta roșie |

În acest proiect, mesajul nu este ascuns în `Y`. Componenta `Y` este păstrată nemodificată, deoarece ochiul uman este mai sensibil la modificările de luminozitate. Mesajul este ascuns în `Cb` și `Cr`, adică în crominanță.

---

## 5. Centrarea valorilor Cb și Cr

Înainte de DCT, valorile `Cb` și `Cr` sunt centrate în jurul lui `0`:

```csharp
cbBlock[y, x] = cbValue - 128.0;
crBlock[y, x] = crValue - 128.0;
```

Motivul este că valorile `Cb` și `Cr` sunt în mod normal în intervalul aproximativ `[0, 255]`, cu valoarea neutră în jur de `128`. Prin scăderea lui `128`, obținem valori centrate în jurul lui `0`, ceea ce este mai potrivit pentru transformarea DCT.

---

## 6. Matricea DCT

DCT este aplicată folosind o matrice de transformare `C`. Pentru un bloc `B`, transformarea este:

```text
DCT = C * B * C^T
```

unde:

- `C` este matricea DCT;
- `B` este blocul `8x8` de date;
- `C^T` este transpusa matricei DCT.

În cod, matricea este construită cu `MathNet.Numerics`:

```csharp
var matrixBuilder = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build;

var dctMatrix = matrixBuilder.Dense(blockSize, blockSize, (u, x) =>
{
    double alpha = (u == 0) ? Math.Sqrt(1.0 / blockSize) : Math.Sqrt(2.0 / blockSize);
    return alpha * Math.Cos(((2 * x + 1) * u * Math.PI) / (2.0 * blockSize));
});
```

Aceasta este formula DCT-II normalizată. Coeficientul `alpha` normalizează prima linie diferit față de celelalte linii:

```text
alpha(0) = sqrt(1 / N)
alpha(u) = sqrt(2 / N), pentru u > 0
```

Pentru `N = 8`, matricea rezultată este o matrice ortonormală. De aceea, inversa sa este transpusa:

```csharp
var inverseDctMatrix = dctMatrix.Transpose();
```

---

## 7. Aplicarea DCT pe Cb și Cr

După ce blocurile `Cb` și `Cr` sunt construite, se aplică DCT:

```csharp
var cbDct = dctMatrix * cbBlock * inverseDctMatrix;
var crDct = dctMatrix * crBlock * inverseDctMatrix;
```

Rezultatul este o matrice de coeficienți pentru fiecare componentă de crominanță.

Într-un bloc DCT `8x8`:

- coeficientul `[0,0]` este componenta DC, adică frecvența foarte joasă;
- coeficienții apropiați de `[0,0]` conțin informație vizuală importantă;
- coeficienții foarte îndepărtați conțin frecvențe înalte, care pot fi instabile;
- coeficienții de frecvență medie sunt un compromis bun pentru ascunderea datelor.

În cod se folosesc două poziții:

```csharp
int coefficientY = (color == 0) ? 3 : 4;
int coefficientX = (color == 0) ? 4 : 3;
```

Asta înseamnă:

```text
Cb -> coeficientul [3,4]
Cr -> coeficientul [4,3]
```

Astfel, fiecare bloc `8x8` poate ascunde `2` biți:

```text
1 bit în Cb
1 bit în Cr
```

---

## 8. Payload-ul ascuns

Mesajul nu este ascuns singur. Înaintea lui se adaugă 4 bytes care reprezintă lungimea mesajului.

Formatul payload-ului este:

```text
[4 bytes lungime mesaj][bytes mesaj]
```

În cod:

```csharp
byte[] payload = new byte[4 + messageBytes.Length];
byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
Array.Copy(lengthBytes, 0, payload, 0, 4);
Array.Copy(messageBytes, 0, payload, 4, messageBytes.Length);
```

Cei 4 bytes de lungime sunt necesari la extragere. Fără ei, algoritmul nu ar ști câți bytes trebuie să citească din imagine.

---

## 9. Calcularea capacității imaginii

Fiecare bloc `8x8` ascunde `2` biți. Numărul total de biți disponibili este:

```csharp
int availableBits = (usableWidth / blockSize) * (usableHeight / blockSize) * 2;
```

Numărul de biți necesari este:

```csharp
int requiredBits = payload.Length * 8;
```

Dacă mesajul este prea mare, metoda aruncă o excepție:

```csharp
if (requiredBits > availableBits)
{
    throw new Exception("Message is too large for this bitmap using DCT chroma embedding.");
}
```

Exemplu pentru o imagine `512x512`:

```text
blocuri pe lățime = 512 / 8 = 64
blocuri pe înălțime = 512 / 8 = 64
număr blocuri = 64 * 64 = 4096
biți disponibili = 4096 * 2 = 8192 biți
bytes disponibili = 8192 / 8 = 1024 bytes
bytes mesaj util = 1024 - 4 = 1020 bytes
```

---

## 10. Rolul cheii și operația XOR

Cheia este citită din `keyStream`:

```csharp
byte[] keyBytes = new byte[keyStream.Length];
keyStream.Seek(0, SeekOrigin.Begin);
keyStream.Read(keyBytes, 0, keyBytes.Length);
```

Pentru fiecare bit din mesaj, se citește un bit corespunzător din cheie. Dacă mesajul este mai lung decât cheia, cheia se repetă:

```csharp
int keyByteIndex = (embeddedBitIndex / 8) % keyBytes.Length;
int keyBitIndex = embeddedBitIndex % 8;
int keyBit = (keyBytes[keyByteIndex] >> keyBitIndex) & 1;
```

Bitul ascuns efectiv este:

```csharp
int bitToEmbed = messageBit ^ keyBit;
```

La extragere se aplică din nou XOR:

```csharp
int decodedBit = extractedBit ^ keyBit;
```

Proprietatea folosită este:

```text
(messageBit XOR keyBit) XOR keyBit = messageBit
```

Astfel, fără aceeași cheie, mesajul nu se poate reconstrui corect.

---

## 11. Ce înseamnă coeficient DCT cuantizat

Un coeficient DCT este o valoare `double`, de exemplu:

```text
87.34
```

LSB se poate aplica doar pe valori întregi, nu direct pe `double`. De aceea, coeficientul este cuantizat:

```csharp
int quantizedCoefficient = (int)Math.Round(coefficient / coefficientStep);
```

Dacă:

```text
coefficient = 87.34
coefficientStep = 20.0
```

atunci:

```text
87.34 / 20.0 = 4.367
round(4.367) = 4
```

Coeficientul DCT cuantizat este `4`.

După ascunderea bitului, valoarea este readusă înapoi în domeniul DCT:

```csharp
cbDct[coefficientY, coefficientX] = quantizedCoefficient * coefficientStep;
```

Dacă valoarea cuantizată devine `5`, coeficientul DCT pus înapoi devine:

```text
5 * 20 = 100
```

---

## 12. Aplicarea LSB pe coeficientul DCT cuantizat

LSB se aplică prin această linie:

```csharp
quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;
```

Această operație face două lucruri:

1. șterge ultimul bit al coeficientului;
2. pune bitul mesajului în acel loc.

### 12.1. Ștergerea ultimului bit

```csharp
quantizedCoefficient & ~1
```

`1` în binar este:

```text
00000001
```

`~1` este:

```text
11111110
```

Când facem `AND` cu `11111110`, ultimul bit devine obligatoriu `0`.

Exemplu:

```text
5       = 00000101
~1      = 11111110
5 & ~1  = 00000100
```

### 12.2. Setarea ultimului bit

După ce ultimul bit a fost șters, se aplică `OR` cu bitul care trebuie ascuns:

```csharp
| bitToEmbed
```

Dacă `bitToEmbed = 1`:

```text
00000100 | 00000001 = 00000101
```

Dacă `bitToEmbed = 0`:

```text
00000100 | 00000000 = 00000100
```

Deci expresia completă:

```csharp
quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;
```

înseamnă:

```text
pune bitToEmbed în LSB-ul coeficientului DCT cuantizat
```

---

## 13. Tratarea coeficienților negativi

Coeficienții DCT pot fi negativi. Pentru operațiile pe biți, codul lucrează pe valoarea absolută, apoi restaurează semnul:

```csharp
if (quantizedCoefficient < 0)
{
    quantizedCoefficient = -quantizedCoefficient;
    quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;
    quantizedCoefficient = -quantizedCoefficient;
}
else
{
    quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;
}
```

Această abordare evită problemele cauzate de reprezentarea internă a numerelor negative în complement față de doi.

La extragere se citește LSB-ul din valoarea absolută:

```csharp
int extractedBit = Math.Abs(quantizedCoefficient) & 1;
```

---

## 14. Aplicarea IDCT

După modificarea coeficienților DCT, blocurile `Cb` și `Cr` trebuie transformate înapoi în domeniul spațial.

Formula este:

```text
block = C^T * DCT * C
```

În cod:

```csharp
var newCbBlock = inverseDctMatrix * cbDct * dctMatrix;
var newCrBlock = inverseDctMatrix * crDct * dctMatrix;
```

Rezultatul este un bloc `8x8` de valori `Cb` și un bloc `8x8` de valori `Cr`, dar modificate astfel încât să conțină informația ascunsă.

---

## 15. Reconstrucția YCbCr -> RGB

După IDCT, se reconstruiește pixelul RGB.

`Y` este păstrat nemodificat:

```csharp
double yValue = yBlock[y, x];
```

`Cb` și `Cr` sunt luate din blocurile reconstruite:

```csharp
double cbValue = newCbBlock[y, x] + 128.0;
double crValue = newCrBlock[y, x] + 128.0;
```

Conversia înapoi în RGB este:

```csharp
double redValue = yValue + 1.402 * (crValue - 128.0);
double greenValue = yValue - 0.344136 * (cbValue - 128.0) - 0.714136 * (crValue - 128.0);
double blueValue = yValue + 1.772 * (cbValue - 128.0);
```

Valorile sunt apoi rotunjite și limitate la intervalul `[0, 255]`:

```csharp
int red = (int)Math.Round(redValue);
int green = (int)Math.Round(greenValue);
int blue = (int)Math.Round(blueValue);

red = Math.Max(0, Math.Min(255, red));
green = Math.Max(0, Math.Min(255, green));
blue = Math.Max(0, Math.Min(255, blue));
```

La final, pixelul este scris în bitmap:

```csharp
bitmap.SetPixel(blockX + x, blockY + y, Color.FromArgb(red, green, blue));
```

---

## 16. Extragerea mesajului

La extragere se repetă pașii:

```text
RGB -> CbCr -> DCT -> cuantizare -> citire LSB -> XOR cu cheia -> reconstruire bytes
```

Nu este nevoie de IDCT la extragere, deoarece imaginea nu este modificată. Trebuie doar să citim coeficienții DCT.

Pentru fiecare bloc:

1. se calculează `Cb` și `Cr`;
2. se aplică DCT;
3. se citesc coeficienții `[3,4]` și `[4,3]`;
4. se cuantizează;
5. se citește LSB-ul;
6. se aplică XOR cu cheia;
7. se reconstruiesc bytes.

Citirea LSB-ului se face cu:

```csharp
int extractedBit = Math.Abs(quantizedCoefficient) & 1;
```

După ce se adună 8 biți, se formează un byte:

```csharp
currentByte |= decodedBit << currentBitInByte;
```

Primii 4 bytes extrași sunt interpretați ca lungime:

```csharp
messageLength = BitConverter.ToInt32(lengthBytes, 0);
```

După aceea, se citesc exact `messageLength` bytes și se scriu în `messageStream`.

---

## 17. De ce se folosește coefficientStep = 20.0

`coefficientStep` controlează cât de mare este treapta de cuantizare.

Dacă pasul este prea mic, mici erori produse de conversiile RGB/YCbCr, rotunjiri sau reconstrucție pot schimba valoarea cuantizată și se poate pierde bitul.

Dacă pasul este mai mare, bitul ascuns devine mai stabil, dar modificarea poate deveni mai vizibilă.

Exemplu de trepte pentru `coefficientStep = 20`:

```text
..., -60, -40, -20, 0, 20, 40, 60, 80, 100, ...
```

Această valoare reprezintă un compromis între stabilitatea extragerii și calitatea vizuală.

---

## 18. Limitări tehnice

### 18.1. Capacitate redusă

Metoda ascunde doar `2` biți per bloc `8x8`. Astfel, capacitatea este mai mică decât la LSB clasic pe pixeli.

### 18.2. Sensibilitate la salvare și conversii

Metoda funcționează pe BMP, unde imaginea nu este comprimată lossy. Dacă imaginea este salvată ulterior în JPEG sau redimensionată, coeficienții DCT se pot schimba și mesajul poate deveni imposibil de extras.

### 18.3. Posibile erori din cauza reconstrucției RGB

Chiar dacă mesajul este ascuns în DCT, imaginea trebuie reconstruită în RGB. Conversia inversă și rotunjirea pot modifica ușor coeficienții care vor fi recalculați la extragere. De aceea se folosește cuantizarea cu `coefficientStep`.

### 18.4. Cheia nu oferă criptare puternică

XOR-ul cu cheia nu este criptare puternică. El maschează biții mesajului, dar nu trebuie considerat echivalent cu un algoritm criptografic modern.

---

## 19. Rezumat tehnic al metodei Hide

Metoda `HideMessageInBitmap` face următoarele:

1. citește mesajul din `messageStream`;
2. construiește payload-ul `[4 bytes length][message bytes]`;
3. citește cheia;
4. calculează zona utilă a imaginii;
5. construiește matricea DCT folosind `MathNet.Numerics`;
6. parcurge imaginea în blocuri de `8x8`;
7. convertește fiecare bloc din RGB în YCbCr;
8. aplică DCT pe `Cb` și `Cr`;
9. extrage bitul curent din payload;
10. aplică XOR cu cheia;
11. cuantizează coeficientul DCT ales;
12. aplică LSB pe coeficientul cuantizat;
13. scrie coeficientul modificat înapoi;
14. aplică IDCT;
15. reconstruiește pixelii RGB;
16. modifică bitmap-ul final.

---

## 20. Rezumat tehnic al metodei Extract

Metoda `ExtractMessageInBitmap` face următoarele:

1. citește cheia;
2. calculează zona utilă a imaginii;
3. construiește aceeași matrice DCT;
4. parcurge imaginea în blocuri de `8x8`;
5. convertește fiecare bloc din RGB în CbCr;
6. aplică DCT pe `Cb` și `Cr`;
7. citește aceiași coeficienți folosiți la ascundere;
8. cuantizează coeficienții;
9. citește LSB-ul;
10. aplică XOR cu cheia;
11. reconstruiește bytes din biți;
12. citește primii 4 bytes ca lungime;
13. citește mesajul complet;
14. scrie mesajul în `messageStream`.

---

## 21. Explicație scurtă pentru prezentare orală

Implementarea folosește steganografie în domeniul frecvență. Imaginea BMP este împărțită în blocuri de `8x8`, fiecare bloc este convertit din RGB în YCbCr, apoi se aplică DCT pe componentele `Cb` și `Cr`. Mesajul nu este ascuns în luma `Y`, ci doar în crominanță, pentru a reduce impactul vizual. Pentru fiecare bloc se aleg doi coeficienți DCT de frecvență medie, unul din `Cb` și unul din `Cr`. Coeficientul este cuantizat, iar bitul mesajului este introdus prin LSB în coeficientul cuantizat. După modificare, se aplică IDCT și imaginea este reconstruită în RGB. La extragere se repetă conversia și DCT, se citește LSB-ul acelorași coeficienți și se reconstruiește mesajul folosind aceeași cheie XOR.

---

## 22. Dependențe

Proiectul folosește:

```text
System
System.Drawing
System.IO
System.Text
MathNet.Numerics
```

Pachetul necesar este:

```powershell
Install-Package MathNet.Numerics
```

Nu sunt necesare alte librării externe.
