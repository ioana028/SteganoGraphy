using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using System.Web;

namespace SteganoGraphy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //((Control)this.tabControl2.TabPages["tbpExtract"]).Enabled = false;
        }

        private void btnHide_Click(object sender, System.EventArgs e)
        {
            Bitmap bitmap = (Bitmap)picImage.Image;
            string msg = textBox1.Text;
            Stream messageStream = GetMessageStream(msg);
            if (messageStream == null)
            {
                MessageBox.Show("Message to embed was not found!");
            }
            else
            {
                String keyString = txtKeyText.Text;
                StreamReader reader = new StreamReader(messageStream, true);
                string message = reader.ReadToEnd();
                MessageBox.Show("Message is: " + message);
                Stream keyStream = GetKeyStream(keyString);
                StreamReader keyReader = new StreamReader(keyStream, true);
                string key = keyReader.ReadToEnd();
                MessageBox.Show("Key is: " + key);
                if (keyStream.Length == 0)
                {
                    MessageBox.Show("Please enter a password or select a key file.");
                    txtKeyText.Focus();
                }
                else
                {
                    try
                    {
                        SteganoGraphy.HideMessageInBitmap(messageStream, bitmap, keyStream);
                        picImage.Image = bitmap;
                        SaveFileTo24RGB();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception:\r\n" + ex.Message);
                    }
                }
                messageStream.Close();
            }
            bitmap = null;
            btnDecode.Enabled = true;
        }

        private Stream GetMessageStream(string keyText)
        {
            BinaryWriter messageWriter = new BinaryWriter(new MemoryStream());
            //messageWriter.Write(keyText.Length);
            messageWriter.Write(Encoding.ASCII.GetBytes(keyText.Length.ToString()));
            messageWriter.Write(Encoding.ASCII.GetBytes(keyText));
            messageWriter.Seek(0, SeekOrigin.Begin);
            return messageWriter.BaseStream;
        }

        private Stream GetKeyStream(string keyText)
        {
            BinaryWriter messageWriter = new BinaryWriter(new MemoryStream());
            messageWriter.Write(Encoding.ASCII.GetBytes(keyText));
            messageWriter.Seek(0, SeekOrigin.Begin);
            return messageWriter.BaseStream;
        }

        private void SetImage(String fileName, bool action)
        {
            if (action)
            {
                picImage.Image = new Bitmap(fileName);
            }
        }

        private String GetFileName(String filter)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            if (filter.Length > 0) { dlg.Filter = filter; }

            if (dlg.ShowDialog(this) != DialogResult.Cancel)
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        private void btnImageFile_Click(object sender, System.EventArgs e)
        {
            fileName = GetFileName("BitmapFile (*.bmp)|*.bmp");
            if (fileName != null)
            {
                if (hiex)
                    txtImageFile.Text = fileName;
                SetImage(fileName, hiex);
            }
            btnHide.Enabled = true;
        }

        private void SaveFileTo24RGB()
        {
            
            txtImageFile.Text = String.Empty;
            this.ResumeLayout();
            char[] trimChars = { '.', 'b', 'm', 'p' };
            string fileShort = fileName.TrimEnd(trimChars);
            // Create a new empty bitmap with 24 bits per pixel
            Bitmap bmp24bpp = new Bitmap(picImage.Image.Width, picImage.Image.Height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(bmp24bpp))
            {
                // Draw the PictureBox's image onto the new 24-bit bitmap
                g.DrawImage(picImage.Image, new Rectangle(0, 0, bmp24bpp.Width, bmp24bpp.Height));
            }

            // Save the new bitmap as a 24-bit BMP file
            bmp24bpp.Save(fileShort + "_encoded.bmp", ImageFormat.Bmp);
            this.SuspendLayout();
            picImage.Image.Dispose();
            picImage.Image = null;
        }

        private void txtImageFile_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetImage(txtImageFile.Text, true);
            }
        }

        private void tabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl2.SelectedTab.Text == "Encoding")
                hiex = true;
            else
                hiex = false;
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 0;
            hiex = true;
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 1;
            hiex = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = (Bitmap)picImage.Image;
            string msg = textBox1.Text;
            MemoryStream messageStream = new MemoryStream();

            String keyString = txtKeyText.Text;
            Stream keyStream = GetKeyStream(keyString);
            StreamReader keyReader = new StreamReader(keyStream, true);
            string key = keyReader.ReadToEnd();
            MessageBox.Show("Key is: " + key);
            if (keyStream.Length == 0)
            {
                MessageBox.Show("Please enter a password or select a key file.");
                txtKeyText.Focus();
            }
            else
            {
                try
                {
                    SteganoGraphy.ExtractMessageInBitmap(messageStream, bitmap, keyStream);
                    if (messageStream != null)
                    {
                        StreamReader reader = new StreamReader(messageStream, true);
                        messageStream.Seek(0, SeekOrigin.Begin);
                        string message = reader.ReadToEnd();
                        MessageBox.Show("Message is: " + message);
                    }
                    else
                        MessageBox.Show("No message decoded!\r\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception:\r\n" + ex.Message);
                }
            }
            messageStream.Close();

            bitmap = null;
        }
    }
}
