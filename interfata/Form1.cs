using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace interfata
{
    public enum SteganographyMethod
    {
        StandardLSB,
        MultiChannelLSB
    }
    public enum OperationMode
    {
        Message,
        File
    }
    public partial class Form1 : Form
    {
        private System.Windows.Forms.PictureBox pictureBoxOriginal;
        private System.Windows.Forms.PictureBox pictureBoxModified;
        private System.Windows.Forms.Button btnCompareImages;
        private Bitmap originalImage;
        private Bitmap modifiedImage;
        //private System.Windows.Forms.Button btnHideFile;
        //private System.Windows.Forms.Button btnExtractFile;

        private SteganographyMethod currentMethod = SteganographyMethod.StandardLSB;
        private OperationMode currentMode = OperationMode.Message;
        public Form1()
        {
            InitializeComponent();
            cmbMethod.SelectedIndex = 0; 
            currentMethod = SteganographyMethod.StandardLSB;
            // Initialize UI based on mode
            SetOperationMode(OperationMode.Message);
            cmbMethod.SelectedIndexChanged += (sender, e) =>
            {
                currentMethod = (SteganographyMethod)cmbMethod.SelectedIndex;
            };
        }
        private void SetOperationMode(OperationMode mode)
        {
            currentMode = mode;

            // Show/hide controls based on mode
            txtMessage.Visible = mode == OperationMode.Message;
            label3.Visible = mode == OperationMode.Message;
            
            //btnHideFile.Visible = mode == OperationMode.File;
            //btnExtractFile.Visible = mode == OperationMode.File;

            // Update UI text
            btnHideMessage.Text = mode == OperationMode.Message ? "Hide Message" : "Hide File";
            btnRevealMessage.Text = mode == OperationMode.Message ? "Extract Message" : "Extract File";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string inputPath = ofd.FileName;

                    // Convert to BMP if needed
                    if (!inputPath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    {
                        string bmpPath = ConvertToBmp(inputPath);
                        if (bmpPath != null)
                        {
                            inputPath = bmpPath;
                            LogActivity($"Image converted to BMP: {bmpPath}");
                        }
                    }

                    txtInputPath.Text = inputPath;
                    originalImage = new Bitmap(inputPath);
                    pictureBoxOriginal.Image = originalImage;
                    // Update capacity info under the path
                    UpdateCapacityInfo(inputPath);
                }
            }
        }
        private void UpdateCapacityInfo(string imagePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(imagePath);
                long imageSize = fileInfo.Length;
                long headerSize = 54; // BMP header
                long pixelDataSize = imageSize - headerSize;
                long maxCapacity = pixelDataSize / 8; // 1 bit per byte

                lblCapacityInfo.Text = $"Can hide: ~{maxCapacity} bytes";
                lblCapacityInfo.ForeColor = maxCapacity > 0 ? SystemColors.ControlText : Color.Red;
            }
            catch
            {
                lblCapacityInfo.Text = "Capacity info unavailable";
                lblCapacityInfo.ForeColor = Color.Red;
            }
        }
        private string ConvertToBmp(string imagePath)
        {
            try
            {
                string bmpPath = Path.ChangeExtension(imagePath, ".bmp");
                using (var image = Image.FromFile(imagePath))
                {
                    image.Save(bmpPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                return bmpPath;
            }
            catch (Exception ex)
            {
                LogActivity($"Conversion failed: {ex.Message}");
                return null;
            }
        }
        //private void CheckImageCapacity(string imagePath)
        //{
        //    try
        //    {
        //        FileInfo fileInfo = new FileInfo(imagePath);
        //        long imageSize = fileInfo.Length;
        //        long headerSize = 54; // Standard BMP header size
        //        long pixelDataSize = imageSize - headerSize;

        //        if (currentMode == OperationMode.Message)
        //        {
        //            long maxMessageSize = pixelDataSize / 8; // 1 bit per byte for LSB
        //            LogActivity($"Image can hide up to {maxMessageSize} bytes");
        //        }
        //        else
        //        {
        //            // For files we need more space for metadata
        //            long maxFileSize = (pixelDataSize / 8) - 256; // Reserve 256 bytes for metadata
        //            LogActivity($"Image can hide files up to {maxFileSize} bytes");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogActivity($"Capacity check failed: {ex.Message}");
        //    }
        //}

        //private void btnBrowseOutput_Click(object sender, EventArgs e)
        //{
        //    using (SaveFileDialog sfd = new SaveFileDialog())
        //    {
        //        sfd.Filter = "Bitmap Files (*.bmp)|*.bmp";
        //        if (sfd.ShowDialog() == DialogResult.OK)
        //        {
        //            txtOutputPath.Text = sfd.FileName;
        //        }
        //    }
        //}

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hideMessage(string imagePath, string message, string outputPath);


        private void btn_hide(object sender, EventArgs e)
        {
            if (currentMode == OperationMode.Message)
            {
                hide_text_message();
            }
            else
            {
                hide_file();
            }
        }
        private void hide_text_message()
        {
            try
            {
                string inputPath = txtInputPath.Text;
                string message = txtMessage.Text;

                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select an image first!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    LogActivity("Please enter a message to hide!");
                    return;
                }
                FileInfo fileInfo = new FileInfo(inputPath);
                long imageSize = fileInfo.Length;
                long headerSize = 54; // Standard BMP header size
                long pixelDataSize = imageSize - headerSize;
                long maxMessageSize = pixelDataSize / 8; // 1 bit per byte for LSB

                if (message.Length > maxMessageSize)
                {
                    LogActivity($"Error: The message is too large to fit in the selected image. Max capacity: {maxMessageSize} bytes.");
                    return;
                }


                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Bitmap Files (*.bmp)|*.bmp";
                    sfd.FileName = Path.GetFileNameWithoutExtension(inputPath) + "_hidden.bmp";
                    sfd.Title = "Save image with hidden content";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string outputPath = sfd.FileName;

                        if (currentMethod == SteganographyMethod.StandardLSB)
                        {
                            SteganographyWrapper.hideMessage(inputPath, message, outputPath);
                        }
                        else
                        {
                            SteganographyWrapper.hide_message_multichannel(inputPath, message, outputPath);
                        }

                        LogActivity("Content hidden successfully!");

                        if (File.Exists(outputPath))
                        {
                            modifiedImage = new Bitmap(outputPath);
                            pictureBoxModified.Image = modifiedImage;
                        }
                        else
                        {
                            LogActivity("Error: The output file was not created. Please check the DLL implementation or file permissions.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error: {ex.Message}");
            }
        }
        private void hide_file()
        {
            try
            {
                string inputPath = txtInputPath.Text;

                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a cover image!");
                    return;
                }

                using (OpenFileDialog fileDialog = new OpenFileDialog())
                {
                    fileDialog.Title = "Select file to hide";
                    if (fileDialog.ShowDialog() != DialogResult.OK) return;

                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
                        saveDialog.Title = "Save stego image";
                        saveDialog.FileName = Path.GetFileNameWithoutExtension(inputPath) + "_stego.bmp";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Check capacity
                            FileInfo fileInfo = new FileInfo(fileDialog.FileName);
                            long requiredSize = fileInfo.Length + 256; // File size + metadata

                            if (!CheckFileCapacity(inputPath, requiredSize))
                            {
                                LogActivity("File is too large to hide in this image");
                                return;
                            }

                            SteganographyWrapper.hide_file(inputPath, fileDialog.FileName, saveDialog.FileName);
                            LogActivity("File hidden successfully!");

                            if (File.Exists(saveDialog.FileName))
                            {
                                modifiedImage = new Bitmap(saveDialog.FileName);
                                pictureBoxModified.Image = modifiedImage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error: {ex.Message}");
            }
        }

        private void LogActivity(string message)
        {
            txtActivity.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            txtActivity.ScrollToCaret();
        }

        private bool CheckFileCapacity(string imagePath, long requiredSize)
        {
            try
            {
                FileInfo imageInfo = new FileInfo(imagePath);
                long imageSize = imageInfo.Length;
                long headerSize = 54; // BMP header
                long availableSize = (imageSize - headerSize) / 8; // 1 bit per byte

                return availableSize >= requiredSize;
            }
            catch
            {
                return false;
            }
        }

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void revealMessage(string imagePath, IntPtr output, int maxLen);

        private void extract_message()
        {
            try
            {
                string inputPath = txtInputPath.Text;
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    txtOutput.Text = "Please provide the input image path!";
                    return;
                }

                // Determine the maximum capacity of the image
                FileInfo fileInfo = new FileInfo(inputPath);
                long imageSize = fileInfo.Length;
                long headerSize = 54; // Standard BMP header size
                long pixelDataSize = imageSize - headerSize;
                long maxMessageSize = pixelDataSize / 8; // 1 bit per byte for LSB

                // Allocate a buffer based on the maximum capacity
                StringBuilder buffer = new StringBuilder((int)maxMessageSize);

                if (currentMethod == SteganographyMethod.StandardLSB)
                {
                    SteganographyWrapper.revealMessage(inputPath, buffer, buffer.Capacity);
                }
                else
                {
                    SteganographyWrapper.reveal_message_multichannel(inputPath, buffer, buffer.Capacity);
                }

                // Display the extracted message
                txtOutput.Text = buffer.ToString();
            }
            catch (Exception ex)
            {
                txtActivity.Text = $"Error: {ex.Message}";
            }
        }

        private void extract_file()
        {
            OpenFileDialog stegoDialog = new OpenFileDialog();
            stegoDialog.Filter = "BMP Files (*.bmp)|*.bmp";
            stegoDialog.Title = "Selectează imaginea stego";
            if (stegoDialog.ShowDialog() != DialogResult.OK) return;

            string embeddedName = SteganographyWrapper.GetEmbeddedFileName(stegoDialog.FileName);
            string embeddedExt = Path.GetExtension(embeddedName);


            //SaveFileDialog saveDialog = new SaveFileDialog();
            //saveDialog.Title = "Salvează fișierul extras";
            //saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            //saveDialog.FileName = Path.GetFileNameWithoutExtension(embeddedName);
            //if (saveDialog.ShowDialog() != DialogResult.OK) return;

            string folder = Path.GetDirectoryName(stegoDialog.FileName); // sau altă sursă
            SteganographyWrapper.extract_file(stegoDialog.FileName, folder); // path-ul e ignorat în DLL
            MessageBox.Show($"Fișierul ascuns ({embeddedName}) a fost extras cu succes!", "Succes");
        }
        private void btn_extract(object sender, EventArgs e)
        {
            if (currentMode == OperationMode.Message)
            {
                extract_message();
            }
            else
            {
                extract_file();
            }
            
        }

        private void txtActivity_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCompareImages_Click(object sender, EventArgs e)
        {
            if (originalImage == null || modifiedImage == null)
            {
                MessageBox.Show("Please load and process both images first.");
                return;
            }

            Bitmap diffImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color pixelOriginal = originalImage.GetPixel(x, y);
                    Color pixelModified = modifiedImage.GetPixel(x, y);

                    if (pixelOriginal != pixelModified)
                    {
                        diffImage.SetPixel(x, y, Color.Red); // marcăm diferența
                    }
                    else
                    {
                        diffImage.SetPixel(x, y, Color.FromArgb(255,
                            (pixelOriginal.R + pixelModified.R) / 2,
                            (pixelOriginal.G + pixelModified.G) / 2,
                            (pixelOriginal.B + pixelModified.B) / 2)); // medie între cele două
                    }
                }
            }

            // Deschidem fereastra nouă
            Form previewForm = new Form();
            previewForm.Text = "Differences Highlighted";
            previewForm.StartPosition = FormStartPosition.CenterScreen;

            // Fie se face cât imaginea (dacă nu e uriașă), fie fixă
            int width = Math.Min(diffImage.Width + 40, 1000);
            int height = Math.Min(diffImage.Height + 60, 800);
            previewForm.ClientSize = new Size(1000, 700);

            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Image = diffImage;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            previewForm.Controls.Add(pictureBox);
            previewForm.Show();
        }

        private void btnHideFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog coverDialog = new OpenFileDialog();
            coverDialog.Filter = "BMP Files (*.bmp)|*.bmp";
            coverDialog.Title = "Selectează imaginea în care să ascunzi fișierul";
            if (coverDialog.ShowDialog() != DialogResult.OK) return;

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Selectează fișierul de ascuns";
            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
            saveDialog.Title = "Salvează imaginea stego";
            saveDialog.FileName = Path.GetFileNameWithoutExtension(coverDialog.FileName) + "_stego.bmp";
            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (FileStream fs = new FileStream(coverDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(10, SeekOrigin.Begin); // offset to pixel data
                    byte[] offsetBytes = new byte[4];
                    fs.Read(offsetBytes, 0, 4);
                    int pixelDataOffset = BitConverter.ToInt32(offsetBytes, 0);

                    fs.Seek(0, SeekOrigin.End);
                    long totalSize = fs.Length;
                    long imageCapacityBytes = totalSize - pixelDataOffset;

                    FileInfo fileInfo = new FileInfo(fileDialog.FileName);
                    long messageSizeBytes = 4 + fileInfo.Name.Length + 4 + fileInfo.Length; // nameLen + name + fileSize + data

                    if (messageSizeBytes * 8 > imageCapacityBytes * 1) // LSB = 1 bit per byte
                    {
                        MessageBox.Show("Fișierul este prea mare pentru a fi ascuns în imaginea selectată!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                SteganographyWrapper.hide_file(coverDialog.FileName, fileDialog.FileName, saveDialog.FileName);
                MessageBox.Show("Fișierul a fost ascuns cu succes!");

                if (File.Exists(saveDialog.FileName))
                {
                    modifiedImage = new Bitmap(saveDialog.FileName);
                    pictureBoxModified.Image = modifiedImage;
                }

                if (File.Exists(coverDialog.FileName))
                {
                    originalImage = new Bitmap(coverDialog.FileName);
                    pictureBoxOriginal.Image = originalImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnExtractFile_Click(object sender, EventArgs e)
        {
            
        }

        private void btnConvertToBmp_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "All images (*.png;*.jpg;*.jpeg;*.gif;*.tif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.tif;*.bmp|All files (*.*)|*.*";
            openDialog.Title = "Selectează imaginea pentru conversie";

            if (openDialog.ShowDialog() != DialogResult.OK) return;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Bitmap files (*.bmp)|*.bmp";
            saveDialog.Title = "Salvează ca BMP";
            saveDialog.FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + ".bmp";

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                using (var image = System.Drawing.Image.FromFile(openDialog.FileName))
                {
                    if (System.Drawing.Imaging.ImageFormat.Bmp.Equals(image.RawFormat))
                    {
                        MessageBox.Show("Imaginea este deja BMP.", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    image.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                MessageBox.Show("Imaginea a fost convertită cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la conversie: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void rdbMessage_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMessage.Checked)
            {
                SetOperationMode(OperationMode.Message);
            }
        }

        private void rdbFile_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbFile.Checked)
            {
                SetOperationMode(OperationMode.File);
            }
        }

        private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
