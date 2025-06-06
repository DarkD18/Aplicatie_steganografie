using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BMPHeader
    {
        public ushort signature;          // File signature ('BM')
        public uint file_size;            // File size in bytes
        public ushort reserved1;          // Reserved (0)
        public ushort reserved2;          // Reserved (0)
        public uint data_offset;          // Offset to pixel data
        public uint header_size;          // Header size (40 bytes)
        public int width;                 // Image width
        public int height;                // Image height
        public ushort planes;             // Number of planes (1)
        public ushort bit_count;          // Bits per pixel (e.g., 24 for RGB)
        public uint compression;          // Compression (0 for uncompressed)
        public uint image_size;           // Image size (may be 0 for uncompressed)
        public int x_pixels_per_meter;    // Horizontal resolution
        public int y_pixels_per_meter;    // Vertical resolution
        public uint colors_used;          // Number of colors used
        public uint colors_important;     // Number of important colors
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
        /*Global variables.*/
        long maxCapacity;
        string processingPath;
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
            label5.Visible = mode == OperationMode.Message;
            txtOutput.Visible = mode == OperationMode.Message;
            txtOutput_path.Visible = mode == OperationMode.File;
            label_extracted_output.Visible = mode == OperationMode.File;

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
                    processingPath = inputPath;
                    if (!inputPath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    {
                        string bmpPath = ConvertToBmp(inputPath);
                        if (bmpPath != null)
                        {
                            processingPath = bmpPath;
                        }
                        else
                        {
                            LogActivity("Error: Failed to convert image to BMP.");
                            return;
                        }
                    }

                    txtInputPath.Text = inputPath; // Show the original path to the user
                    originalImage = new Bitmap(processingPath); // Load the BMP for internal processing
                    pictureBoxOriginal.Image = originalImage;

                    // Update capacity info under the path
                    UpdateCapacityInfo(processingPath);
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
                long pixels = pixelDataSize / 3;
                maxCapacity = currentMethod == SteganographyMethod.StandardLSB
                    ? pixels / 8 // 1 bit per pixel for Standard LSB
                    : (pixels / 8) * 3; // 3 channels = 3x capacity for Multi-Channel LSB
                maxCapacity -= 1;
                lblCapacityInfo.Text = $"Can hide: ~{maxCapacity} bytes ({maxCapacity/1024} KB)";
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
                string bmpPath = Path.ChangeExtension(Path.GetTempFileName(), ".bmp");
                using (var image = Image.FromFile(imagePath))
                {
                    image.Save(bmpPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                return bmpPath;
            }
            catch (Exception ex)
            {
                LogActivity($"Conversion to BMP failed: {ex.Message}");
                return null;
            }
        }
       
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

                // Convert the input image to BMP format for processing
                string bmpPath = ConvertToBmp(inputPath);
                if (bmpPath == null)
                {
                    LogActivity("Error: Failed to convert image to BMP.");
                    return;
                }

                using (var image = Image.FromFile(bmpPath))
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    byte[] bmpData = memoryStream.ToArray();

                    int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                    BMPHeader header;
                    byte[] pixelData = new byte[bmpData.Length - headerSize];

                    using (var reader = new BinaryReader(new MemoryStream(bmpData)))
                    {
                        IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                        Marshal.Copy(reader.ReadBytes(headerSize), 0, headerPtr, headerSize);
                        header = Marshal.PtrToStructure<BMPHeader>(headerPtr);
                        Marshal.FreeHGlobal(headerPtr);

                        reader.Read(pixelData, 0, pixelData.Length);
                    }

                    byte[] outputData = new byte[pixelData.Length];
                    if(message.Length > maxCapacity)
                        {
                             LogActivity("Message is too big to be hidden!");
                            return;
                        }
                    GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                    GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                    GCHandle outputDataHandle = GCHandle.Alloc(outputData, GCHandleType.Pinned);

                    try
                    {
                        if (currentMethod == SteganographyMethod.StandardLSB)
                        {
                            // Call the single-channel hiding function
                            SteganographyWrapper.hideMessage(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                message,
                                outputDataHandle.AddrOfPinnedObject()
                            );
                        }
                        else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                        {
                            // Call the multi-channel hiding function
                            SteganographyWrapper.hide_message_multichannel(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                message,
                                outputDataHandle.AddrOfPinnedObject()
                            );
                        }
                    }
                    finally
                    {
                        headerHandle.Free();
                        pixelDataHandle.Free();
                        outputDataHandle.Free();
                    }

                    using (var outputStream = new MemoryStream())
                    {
                        outputStream.Write(bmpData, 0, headerSize);
                        outputStream.Write(outputData, 0, outputData.Length);

                        using (var outputImage = Image.FromStream(outputStream))
                        {
                            string outputPath = Path.ChangeExtension(inputPath, "_hidden.bmp");
                            outputImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Bmp);

                            // Update the modified image and display it in the PictureBox
                            modifiedImage = new Bitmap(outputImage);
                            pictureBoxModified.Image = modifiedImage;

                            LogActivity($"Image saved as BMP at: {outputPath}");
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

                using (var image = Image.FromFile(inputPath))
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    byte[] bmpData = memoryStream.ToArray();

                    int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                    BMPHeader header;
                    byte[] pixelData = new byte[bmpData.Length - headerSize];

                    using (var reader = new BinaryReader(new MemoryStream(bmpData)))
                    {
                        IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                        Marshal.Copy(reader.ReadBytes(headerSize), 0, headerPtr, headerSize);
                        header = Marshal.PtrToStructure<BMPHeader>(headerPtr);
                        Marshal.FreeHGlobal(headerPtr);

                        reader.Read(pixelData, 0, pixelData.Length);
                    }

                    using (OpenFileDialog fileDialog = new OpenFileDialog())
                    {
                        fileDialog.Title = "Select file to hide";
                        if (fileDialog.ShowDialog() != DialogResult.OK) return;

                        byte[] fileData = File.ReadAllBytes(fileDialog.FileName);
                        string fileName = Path.GetFileName(fileDialog.FileName);

                        // Calculate available capacity
                        if (fileData.Length > maxCapacity)
                        {
                            MessageBox.Show("The file is too large to hide in the selected image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogActivity($"Error: File '{fileName}' is too large to hide. Available capacity: {maxCapacity} bytes, File size: {fileData.Length} bytes.");
                            return;
                        }

                        byte[] outputData = new byte[pixelData.Length];

                        GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                        GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                        GCHandle fileDataHandle = GCHandle.Alloc(fileData, GCHandleType.Pinned);
                        GCHandle outputDataHandle = GCHandle.Alloc(outputData, GCHandleType.Pinned);

                        try
                        {
                            if (currentMethod == SteganographyMethod.StandardLSB)
                            {
                                // Call the single-channel hiding function
                                SteganographyWrapper.hideFile(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                    (uint)pixelData.Length,
                                    fileName,
                                    fileDataHandle.AddrOfPinnedObject(),
                                    (uint)fileData.Length,
                                    outputDataHandle.AddrOfPinnedObject()
                                );
                            }
                            else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                            {
                                // Call the multi-channel hiding function
                                SteganographyWrapper.hide_file_multichannel(
                              pixelDataHandle.AddrOfPinnedObject(),
                             (uint)pixelData.Length,
                              fileName,
                              fileDataHandle.AddrOfPinnedObject(),
                              (uint)fileData.Length,
                              outputDataHandle.AddrOfPinnedObject()
                          );
                            }
                        }
                        finally
                        {
                            headerHandle.Free();
                            pixelDataHandle.Free();
                            fileDataHandle.Free();
                            outputDataHandle.Free();
                        }

                       
                        string outputPath = Path.ChangeExtension(inputPath, "_hidden.bmp");
                        using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(bmpData, 0, headerSize);
                            fs.Write(outputData, 0, outputData.Length);
                        }
                        LogActivity($"Image saved as BMP at: {outputPath}");
                        
                        using (var bmp = new Bitmap(outputPath))
                        {
                            modifiedImage = new Bitmap(bmp);
                            pictureBoxModified.Image = modifiedImage;
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

                // Ensure the input file is a BMP
                if (!inputPath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    LogActivity("Error: Extraction is only supported for BMP files.");
                    return;
                }

                using (var image = Image.FromFile(inputPath))
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    byte[] bmpData = memoryStream.ToArray();

                    int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                    BMPHeader header;
                    byte[] pixelData = new byte[bmpData.Length - headerSize];

                    using (var reader = new BinaryReader(new MemoryStream(bmpData)))
                    {
                        IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                        Marshal.Copy(reader.ReadBytes(headerSize), 0, headerPtr, headerSize);
                        header = Marshal.PtrToStructure<BMPHeader>(headerPtr);
                        Marshal.FreeHGlobal(headerPtr);

                        reader.Read(pixelData, 0, pixelData.Length);
                    }

                    StringBuilder output = new StringBuilder((int)maxCapacity); // Adjust size as needed

                    GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                    GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);

                    try
                    {
                        if (currentMethod == SteganographyMethod.StandardLSB)
                        {
                            // Call the single-channel extraction function
                            SteganographyWrapper.revealMessage(
                                pixelDataHandle.AddrOfPinnedObject(),
                               (uint)pixelData.Length,
                                output,
                                (uint)output.Capacity
                            );
                        }
                        else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                        {
                            // Call the multi-channel extraction function
                         SteganographyWrapper.reveal_message_multichannel(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                output,
                               (uint)output.Capacity
                            );
                        }

                        // Ensure the message is null-terminated
                        txtOutput.Text = output.ToString().TrimEnd('\0');
                        LogActivity("Message extracted successfully!");
                    }
                    finally
                    {
                        headerHandle.Free();
                        pixelDataHandle.Free();
                    }
                }
            }
            catch (Exception ex)
            {
                txtActivity.Text = $"Error: {ex.Message}";
            }
        }

        private void extract_file()
        {
            try
            {
                string inputPath = txtInputPath.Text;
                uint fileSize;
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a stego image!");
                    return;
                }

                using (var image = Image.FromFile(inputPath))
                using (var memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                    byte[] bmpData = memoryStream.ToArray();

                    int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                    BMPHeader header;
                    byte[] pixelData = new byte[bmpData.Length - headerSize];

                    using (var reader = new BinaryReader(new MemoryStream(bmpData)))
                    {
                        IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                        Marshal.Copy(reader.ReadBytes(headerSize), 0, headerPtr, headerSize);
                        header = Marshal.PtrToStructure<BMPHeader>(headerPtr);
                        Marshal.FreeHGlobal(headerPtr);

                        reader.Read(pixelData, 0, pixelData.Length);
                    }

                    StringBuilder fileName = new StringBuilder(256); // Adjust size as needed
                    byte[] fileData = new byte[pixelData.Length];
                    fileSize = 0;

                    GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                    GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                    GCHandle fileDataHandle = GCHandle.Alloc(fileData, GCHandleType.Pinned);

                    try
                    {


                        if (currentMethod == SteganographyMethod.StandardLSB)
                        {
                            // Call the single-channel hiding function
                            SteganographyWrapper.extractFile(
                                pixelDataHandle.AddrOfPinnedObject(),
                               (uint)pixelData.Length,
                                fileName,
                                fileDataHandle.AddrOfPinnedObject(),
                                ref fileSize
                            );
                        }
                        else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                        {
                            // Call the multi-channel hiding function
                            uint fileSizePtr = (uint)fileData.Length;
                            SteganographyWrapper.extract_file_multichannel(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                fileName,
                                fileDataHandle.AddrOfPinnedObject(),
                                ref fileSizePtr
                            );
                            fileSize = (uint)fileSizePtr;
                        }
                        string outputPath = Path.Combine(Path.GetDirectoryName(inputPath), fileName.ToString());
                        File.WriteAllBytes(outputPath, fileData.Take((int)fileSize).ToArray());
                        LogActivity($"File extracted to: {outputPath}");
                        txtOutput_path.Text = outputPath;
                    }
                    finally
                    {
                        headerHandle.Free();
                        pixelDataHandle.Free();
                        fileDataHandle.Free();
                    }
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error: {ex.Message}");
            }
        }

        private void btn_extract(object sender, EventArgs e)
        {
            try
            {
                if (currentMode == OperationMode.Message)
                {
                    extract_message(); // Extract a hidden text message from the image
                }
                else if (currentMode == OperationMode.File)
                {
                    extract_file(); // Extract a hidden file from the image
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error: {ex.Message}");
            }
        }

        private void txtActivity_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void btnCompareImages_Click(object sender, EventArgs e)
        {
            if (originalImage == null || modifiedImage == null)
            {
                MessageBox.Show("Please load and process both images first.");
                return;
            }

            btnCompareImages.Text = "Comparing...";
            btnCompareImages.Enabled = false;
            progressBarCompare.Visible = true;
            progressBarCompare.Minimum = 0;
            progressBarCompare.Maximum = originalImage.Height;
            progressBarCompare.Value = 0;

            Bitmap diffImage = new Bitmap(originalImage.Width, originalImage.Height);

            await Task.Run(() =>
            {
                int singleChannelModifications = 0;
                int multiChannelModifications = 0;

                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        Color pixelOriginal = originalImage.GetPixel(x, y);
                        Color pixelModified = modifiedImage.GetPixel(x, y);

                        // Count modified channels (R, G, or B)
                        if (pixelOriginal.R != pixelModified.R) singleChannelModifications++;
                        if (pixelOriginal.G != pixelModified.G) singleChannelModifications++;
                        if (pixelOriginal.B != pixelModified.B) singleChannelModifications++;

                        // If ANY channel differs, mark the pixel as changed
                        if (pixelOriginal != pixelModified)
                        {
                            diffImage.SetPixel(x, y, Color.Red);
                            multiChannelModifications++; // Counts unique pixels modified
                        }
                        else
                        {
                            diffImage.SetPixel(x, y, pixelOriginal);
                        }
                    }
                    Invoke(new Action(() => progressBarCompare.Value = y + 1));
                }
            });

            progressBarCompare.Visible = false;
            btnCompareImages.Text = "Compare Images";
            btnCompareImages.Enabled = true;

            // Show the result as before
            Form previewForm = new Form();
            previewForm.Text = "Differences Highlighted";
            previewForm.StartPosition = FormStartPosition.CenterScreen;
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
            try
            {
                // Step 1: Select the cover image
                using (OpenFileDialog coverDialog = new OpenFileDialog())
                {
                    coverDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif|All Files|*.*";
                    coverDialog.Title = "Select the cover image";
                    if (coverDialog.ShowDialog() != DialogResult.OK) return;

                    string inputPath = coverDialog.FileName;

                    // Step 2: Select the file to hide
                    using (OpenFileDialog fileDialog = new OpenFileDialog())
                    {
                        fileDialog.Title = "Select the file to hide";
                        if (fileDialog.ShowDialog() != DialogResult.OK) return;

                        string filePath = fileDialog.FileName;
                        byte[] fileData = File.ReadAllBytes(filePath);
                        string fileName = Path.GetFileName(filePath);

                        // Step 3: Convert the cover image to BMP format in memory
                        using (var image = Image.FromFile(inputPath))
                        using (var memoryStream = new MemoryStream())
                        {
                            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                            byte[] bmpData = memoryStream.ToArray();

                            // Extract the BMP header and pixel data
                            int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                            BMPHeader header;
                            byte[] pixelData = new byte[bmpData.Length - headerSize];

                            using (var reader = new BinaryReader(new MemoryStream(bmpData)))
                            {
                                IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                                Marshal.Copy(reader.ReadBytes(headerSize), 0, headerPtr, headerSize);
                                header = Marshal.PtrToStructure<BMPHeader>(headerPtr);
                                Marshal.FreeHGlobal(headerPtr);

                                reader.Read(pixelData, 0, pixelData.Length);
                            }

                            // Step 4: Allocate memory for the output pixel data
                            byte[] outputData = new byte[pixelData.Length];

                            GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                            GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                            GCHandle fileDataHandle = GCHandle.Alloc(fileData, GCHandleType.Pinned);
                            GCHandle outputDataHandle = GCHandle.Alloc(outputData, GCHandleType.Pinned);

                            try
                            {
                                SteganographyWrapper.hideFile(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                    (uint)pixelData.Length,
                                    fileName,
                                    fileDataHandle.AddrOfPinnedObject(),
                                    (uint)fileData.Length,
                                    outputDataHandle.AddrOfPinnedObject()
                                );
                            }
                            finally
                            {
                                headerHandle.Free();
                                pixelDataHandle.Free();
                                fileDataHandle.Free();
                                outputDataHandle.Free();
                            }

                            // Step 6: Save the modified image
                            using (var outputStream = new MemoryStream())
                            {
                                outputStream.Write(bmpData, 0, headerSize); // Write the header
                                outputStream.Write(outputData, 0, outputData.Length); // Write the modified pixel data

                                using (var outputImage = Image.FromStream(outputStream))
                                {
                                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                                    {
                                        saveDialog.Filter = "Bitmap Files (*.bmp)|*.bmp";
                                        saveDialog.Title = "Save the stego image";
                                        saveDialog.FileName = Path.GetFileNameWithoutExtension(inputPath) + "_stego.bmp";

                                        if (saveDialog.ShowDialog() == DialogResult.OK)
                                        {
                                            string outputPath = saveDialog.FileName;
                                            outputImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Bmp);
                                            LogActivity($"File hidden successfully! Image saved at: {outputPath}");
                                        }
                                    }
                                }
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
            // Update the currentMethod based on the selected index
            if (cmbMethod.SelectedIndex == 0)
            {
                currentMethod = SteganographyMethod.StandardLSB; // Standard LSB (Blue Channel)
                LogActivity("Switched to Standard LSB (Blue Channel) method.");
            }
            else if (cmbMethod.SelectedIndex == 1)
            {
                currentMethod = SteganographyMethod.MultiChannelLSB; // Multi-Channel LSB (R+G+B)
                LogActivity("Switched to Multi-Channel LSB (R+G+B) method.");
            }
            if(!string.IsNullOrEmpty(processingPath))
            { 
                UpdateCapacityInfo(processingPath);
            }
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            // Clear text fields
            txtInputPath.Clear();
            txtMessage.Clear();
            txtOutput.Clear();
            txtActivity.Clear();

            // Reset PictureBoxes to placeholder images
            pictureBoxOriginal.Image = Properties.Resources.placeholder_image_original; // Ensure you have a placeholder image
            pictureBoxModified.Image = Properties.Resources.placeholder_image_result;   // Ensure you have a placeholder image

            // Reset ComboBox and RadioButtons
            cmbMethod.SelectedIndex = 0; // Reset to the first method (StandardLSB)
            rdbMessage.Checked = true;  // Reset to "Hide Message" mode

            // Reset internal variables
            currentMethod = SteganographyMethod.StandardLSB;
            currentMode = OperationMode.Message;
            originalImage = null;
            modifiedImage = null;

            // Reset capacity info
            lblCapacityInfo.Text = string.Empty;

            // Log reset action
            LogActivity("Application reset to initial state.");
        }

    }
}
