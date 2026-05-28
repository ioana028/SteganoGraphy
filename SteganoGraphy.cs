using System;
using System.Drawing;
using System.IO;
using System.Text;


// DRAGOMIR IOANA
namespace SteganoGraphy
{
    public class SteganoGraphy
    {
        public static void HideMessageInBitmap(Stream messageStream, Bitmap bitmap, Stream keyStream)
        {
            int blockSize = 8;

            // Quantization step used before applying LSB on the DCT coefficient.
            // A larger value can make the hidden bit more stable, but may affect image quality more.
            double coefficientStep = 20.0;

            // THE MESSAGE --------------------------------------------------------
            byte[] messageBytes = new byte[messageStream.Length];
            messageStream.Seek(0, SeekOrigin.Begin);
            messageStream.Read(messageBytes, 0, messageBytes.Length);


           
            //PAYLOAD 4 Bytes for Length + message bytes --------------------------
            byte[] payload = new byte[4 + messageBytes.Length];
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            Array.Copy(lengthBytes, 0, payload, 0, 4);
            Array.Copy(messageBytes, 0, payload, 4, messageBytes.Length);


       
            //KEY for XOR -----------------------------------------------------------
            byte[] keyBytes = new byte[keyStream.Length];
            keyStream.Seek(0, SeekOrigin.Begin);
            keyStream.Read(keyBytes, 0, keyBytes.Length);


            // WIDTH AND HEIGHT THAT CAN BE USED -------------------------------------
            int usableWidth = bitmap.Width - (bitmap.Width % blockSize);
            int usableHeight = bitmap.Height - (bitmap.Height % blockSize);  //removing the margins

            
            int availableBits = (usableWidth / blockSize) * (usableHeight / blockSize) * 2; //number of blocks possible *2 because we are puting a bit in Cb and one in Cr
            int requiredBits = payload.Length * 8;   //calculating nr of bits that we need to hide (payload is in bytes)


            
            //---------------------------------------------------------------------------
            //DCT matrix         DCT = C * block * C^T       block = C^T * DCT * C
            var matrixBuilder = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build;
            var dctMatrix = matrixBuilder.Dense(blockSize, blockSize, (u, x) =>
            {
                double alpha = (u == 0) ? Math.Sqrt(1.0 / blockSize) : Math.Sqrt(2.0 / blockSize);
                return alpha * Math.Cos(((2 * x + 1) * u * Math.PI) / (2.0 * blockSize));
            });
            var inverseDctMatrix = dctMatrix.Transpose();


            

            


            int embeddedBitIndex = 0; //index of the current bit from the message that has to be hiden

            for (int blockY = 0; blockY < usableHeight && embeddedBitIndex < requiredBits; blockY += blockSize)  //vertical
            {
                for (int blockX = 0; blockX < usableWidth && embeddedBitIndex < requiredBits; blockX += blockSize) //orizontal
                {
                    var yBlock = matrixBuilder.Dense(blockSize, blockSize);
                    var cbBlock = matrixBuilder.Dense(blockSize, blockSize);
                    var crBlock = matrixBuilder.Dense(blockSize, blockSize);


                    // Y Cb Cr  CONVERSION WITH THE MATRIX
                    for (int y = 0; y < blockSize; y++) //each row inside block
                    {
                        for (int x = 0; x < blockSize; x++)  //column
                        {
                            Color pixelColor = bitmap.GetPixel(blockX + x, blockY + y);  //pixel curent
                            double red = pixelColor.R;
                            double green = pixelColor.G;
                            double blue = pixelColor.B;


                            double yValue = 0.299 * red + 0.587 * green + 0.114 * blue; //Luma Y
                            double cbValue = -0.169 * red - 0.331 * green + 0.500 * blue + 128.0; //Cb
                            double crValue = 0.500 * red - 0.419 * green - 0.081 * blue + 128.0; //Cr
                            //valori intre 0–255

                            //normalizam -128 128 pt ca DCT sa lucreze cu valorile reale ale crominanței față de valoarea neutră
                            yBlock[y, x] = yValue;
                            cbBlock[y, x] = cbValue - 128.0;
                            crBlock[y, x] = crValue - 128.0;
                        }
                    }

                    //=====================  DCT  ========================
                    // DCT = C * block * C^T      CREARE DCT PT. Cb SI Cr
                    var cbDct = dctMatrix * cbBlock * inverseDctMatrix;
                    var crDct = dctMatrix * crBlock * inverseDctMatrix;


                    
                    //HIDING 2 bits per block, one in Cb and one in Cr
                    // color = 0 means Cb, color = 1 means Cr.
                    for (int color = 0; color < 2 && embeddedBitIndex < requiredBits; color++)
                    { 
                        int payloadByteIndex = embeddedBitIndex / 8; //the byte index with the bit to be embeded
                        int payloadBitIndex = embeddedBitIndex % 8; //the bit index to be embeded

                        int messageBit = (payload[payloadByteIndex] >> payloadBitIndex) & 1;  //geting he bit from the message

                        //the key bit ( its repeting if the message is larger that the key
                        int keyByteIndex = (embeddedBitIndex / 8) % keyBytes.Length;
                        int keyBitIndex = embeddedBitIndex % 8;
                        int keyBit = (keyBytes[keyByteIndex] >> keyBitIndex) & 1;

                        int bitToEmbed = messageBit ^ keyBit;  //XOR 

                        //midle frequency ( low frequency modification can show and high can be lost in compresion, decompresion, redemensioning )
                        int coefficientY = (color == 0) ? 3 : 4;
                        int coefficientX = (color == 0) ? 4 : 3;

                        double coefficient;  //preiau frecventa(coeficient DCT) unde ascund bitul
                        if (color == 0)
                            coefficient = cbDct[coefficientY, coefficientX];
                        else
                            coefficient = crDct[coefficientY, coefficientX];


                        //facem quantificare ca să lucrăm cu trepte de 20. Practic, coeficientul DCT este mutat către o valoare apropiată de un multiplu de 20.
                        //daca avem valoarea 87.34  /20 = 4   in binar 1 0 0  + ascund bitul 1 => 1 0 1 ( adica 5)  si dupa coeficientul devine *20 =>100 
                        //facem asta pt a nu pierde date la conversii ....
                        int quantizedCoefficient = (int)Math.Round(coefficient / coefficientStep); //LSB se aplica pe intregi

                        // If the coefficient is negative, apply LSB on its absolute value.
                        if (quantizedCoefficient < 0)
                        {
                            quantizedCoefficient = -quantizedCoefficient;
                            quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;
                            quantizedCoefficient = -quantizedCoefficient;
                        }
                        else
                            quantizedCoefficient = (quantizedCoefficient & ~1) | bitToEmbed;  //ascundere bit
                        

                        // Write the modified coefficient back into the correct DCT matrix.
                        //scriere coeficient modificat in matricele Cb si Cr (cele DCT)
                        if (color == 0)
                            cbDct[coefficientY, coefficientX] = quantizedCoefficient * coefficientStep;
                        else
                            crDct[coefficientY, coefficientX] = quantizedCoefficient * coefficientStep;
                        
                        embeddedBitIndex++;//urmatorul bit de ascuns din mesaj
                    }


                    //========================  IDCT  ============================
                    // block = C^T * DCT * C
                    var newCbBlock = inverseDctMatrix * cbDct * dctMatrix;
                    var newCrBlock = inverseDctMatrix * crDct * dctMatrix;


                  

                    //Y Cb Cr   to   RGB
                    for (int y = 0; y < blockSize; y++)  //row
                    {
                        for (int x = 0; x < blockSize; x++)  //column
                        {
                            double yValue = yBlock[y, x];
                            double cbValue = newCbBlock[y, x] + 128.0;
                            double crValue = newCrBlock[y, x] + 128.0;

                            double redValue = yValue + 1.402 * (crValue - 128.0);
                            double greenValue = yValue - 0.344136 * (cbValue - 128.0) - 0.714136 * (crValue - 128.0);
                            double blueValue = yValue + 1.772 * (cbValue - 128.0);

                            int red = (int)Math.Round(redValue);  //pixeli trebuie sa fie nr intregi
                            int green = (int)Math.Round(greenValue);
                            int blue = (int)Math.Round(blueValue);

                            red = Math.Max(0, Math.Min(255, red));
                            green = Math.Max(0, Math.Min(255, green));
                            blue = Math.Max(0, Math.Min(255, blue));


                            //Dacă transformările inverse + conversia în RGB + clamp-ul modifică valorile prea mult, atunci la extragere
                            //coeficientul DCT poate ajunge într-o altă treaptă de cuantizare, iar LSB-ul poate ieși greșit.
                            //20 este pasul de cuantizare. El creează trepte de genul: ..., -40, -20, 0, 20, 40, 60, 80, 100, ...
                            //Dacă modificările produse de reconstrucție sunt mici, 20 este suficient. Dacă modificările sunt mari, bitul poate fi pierdut.
                            //În acel caz, un coefficientStep mai mare, de exemplu 30 sau 40, poate face mesajul mai stabil, dar degradează mai mult imaginea.

                            bitmap.SetPixel(blockX + x, blockY + y, Color.FromArgb(red, green, blue));  //puting the redone RGB picture in the bitmap
                        }
                    }
                }
            }
        }

        public static byte[] GetBits(byte b)
        {
            byte[] bits = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                bits[i] = (byte)((b & (1 << i)) != 0 ? 1 : 0);
            }
            return bits;
        }

        public static void ExtractMessageInBitmap(Stream messageStream, Bitmap bitmap, Stream keyStream)
        {
            int blockSize = 8;
            double coefficientStep = 20.0;

            //KEY for XOR -----------------------------------------------------------
            byte[] keyBytes = new byte[keyStream.Length];
            keyStream.Seek(0, SeekOrigin.Begin);
            keyStream.Read(keyBytes, 0, keyBytes.Length);



            // WIDTH AND HEIGHT THAT CAN BE USED -------------------------------------
            int usableWidth = bitmap.Width - (bitmap.Width % blockSize);
            int usableHeight = bitmap.Height - (bitmap.Height % blockSize);  //removing the margins


            int availableBits = (usableWidth / blockSize) * (usableHeight / blockSize) * 2; //number of blocks possible *2 because we are reading one bit from Cb and one from Cr
            int maximumPayloadBytes = (availableBits / 8) - 4; //maximum possible message size, without the first 4 bytes used for length



            //---------------------------------------------------------------------------
            //DCT matrix         DCT = C * block * C^T
            var matrixBuilder = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build;
            var dctMatrix = matrixBuilder.Dense(blockSize, blockSize, (u, x) =>
            {
                double alpha = (u == 0) ? Math.Sqrt(1.0 / blockSize) : Math.Sqrt(2.0 / blockSize);
                return alpha * Math.Cos(((2 * x + 1) * u * Math.PI) / (2.0 * blockSize));
            });
            var inverseDctMatrix = dctMatrix.Transpose();





            //MESSAGE RECONSTRUCTION ----------------------------------------------------
            byte[] lengthBytes = new byte[4];  //first 4 extracted bytes represent the message length
            System.Collections.Generic.List<byte> decodedMessageBytes = new System.Collections.Generic.List<byte>(); //actual message bytes

            int extractedBitIndex = 0; //index of the current extracted bit
            int currentByte = 0;       //byte that is reconstructed bit by bit
            int currentBitInByte = 0;  //position of the current bit inside currentByte
            int messageLength = -1;    //the message length is unknown until the first 4 bytes are extracted



            for (int blockY = 0; blockY < usableHeight; blockY += blockSize)  //vertical
            {
                for (int blockX = 0; blockX < usableWidth; blockX += blockSize) //horizontal
                {
                    var cbBlock = matrixBuilder.Dense(blockSize, blockSize);
                    var crBlock = matrixBuilder.Dense(blockSize, blockSize);



                    // Y Cb Cr CONVERSION WITH THE MATRIX
                    // We only need Cb and Cr because the hidden bits are stored in chroma, not in Y.
                    for (int y = 0; y < blockSize; y++) //each row inside block
                    {
                        for (int x = 0; x < blockSize; x++)  //column
                        {
                            Color pixelColor = bitmap.GetPixel(blockX + x, blockY + y);  //current pixel
                            double red = pixelColor.R;
                            double green = pixelColor.G;
                            double blue = pixelColor.B;


                            double cbValue = -0.169 * red - 0.331 * green + 0.500 * blue + 128.0; //Cb
                            double crValue = 0.500 * red - 0.419 * green - 0.081 * blue + 128.0; //Cr
                                                                                                 //values are in the 0-255 range


                            //normalizing around zero before applying DCT
                            cbBlock[y, x] = cbValue - 128.0;
                            crBlock[y, x] = crValue - 128.0;
                        }
                    }



                    //=====================  DCT  ========================
                    // DCT = C * block * C^T      CREATING DCT FOR Cb AND Cr
                    var cbDct = dctMatrix * cbBlock * inverseDctMatrix;
                    var crDct = dctMatrix * crBlock * inverseDctMatrix;




                    //EXTRACTING 2 bits per block, one from Cb and one from Cr
                    // color = 0 means Cb, color = 1 means Cr.
                    for (int color = 0; color < 2; color++)
                    {
                        //same middle frequency coefficients used in HideMessageInBitmap
                        int coefficientY = (color == 0) ? 3 : 4;
                        int coefficientX = (color == 0) ? 4 : 3;

                        double coefficient;  //the DCT coefficient where the bit was hidden
                        if (color == 0)
                            coefficient = cbDct[coefficientY, coefficientX];
                        else
                            coefficient = crDct[coefficientY, coefficientX];



                        //quantizing exactly like in HideMessageInBitmap
                        //LSB is read from this integer value
                        int quantizedCoefficient = (int)Math.Round(coefficient / coefficientStep);

                        //reading the least significant bit from the quantized coefficient
                        int extractedBit = Math.Abs(quantizedCoefficient) & 1;



                        //the key bit, repeating if the message is larger than the key
                        int keyByteIndex = (extractedBitIndex / 8) % keyBytes.Length;
                        int keyBitIndex = extractedBitIndex % 8;
                        int keyBit = (keyBytes[keyByteIndex] >> keyBitIndex) & 1;

                        int decodedBit = extractedBit ^ keyBit;  //undo XOR



                        //putting the decoded bit inside the current byte
                        currentByte |= decodedBit << currentBitInByte;

                        currentBitInByte++; //next bit position inside currentByte
                        extractedBitIndex++; //next extracted bit from the image



                        //when we have 8 bits, we reconstructed one full byte
                        if (currentBitInByte == 8)
                        {
                            //first 4 bytes represent the message length
                            if (extractedBitIndex <= 32)
                            {
                                lengthBytes[(extractedBitIndex / 8) - 1] = (byte)currentByte;

                                //after 32 bits, we have all 4 bytes of the length
                                if (extractedBitIndex == 32)
                                {
                                    messageLength = BitConverter.ToInt32(lengthBytes, 0);

                                    //if the length is not valid, the image has no message or the key is wrong
                                    if (messageLength < 0 || messageLength > maximumPayloadBytes)
                                        throw new Exception("No valid DCT hidden message was found, or the key is incorrect.");

                                    //empty message case
                                    if (messageLength == 0)
                                    {
                                        messageStream.SetLength(0);
                                        messageStream.Seek(0, SeekOrigin.Begin);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                //after the first 4 bytes, the rest is the actual hidden message
                                decodedMessageBytes.Add((byte)currentByte);

                                //stop when we extracted the full message
                                if (messageLength >= 0 && decodedMessageBytes.Count == messageLength)
                                {
                                    messageStream.SetLength(0); //clear output stream

                                    for (int i = 0; i < decodedMessageBytes.Count; i++)
                                    {
                                        messageStream.WriteByte(decodedMessageBytes[i]); //writing decoded message byte by byte
                                    }

                                    messageStream.Seek(0, SeekOrigin.Begin); //putting cursor at the beginning so Form1 can read it
                                    return;
                                }
                            }

                            currentByte = 0; //reset for the next byte
                            currentBitInByte = 0; //reset bit position
                        }
                    }
                }
            }

        }

    }

}
