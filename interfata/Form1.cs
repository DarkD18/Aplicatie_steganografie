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
            cmbOutputFormat.SelectedIndex = 0; 
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
                string password = get_shuffle_key();

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

                // 1) Convert to 24-bpp BMP bytes
                byte[] bmpData;
                int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                BMPHeader header;
                byte[] pixelData;
                using (var orig = Image.FromFile(inputPath))
                using (var bmp24 = new Bitmap(orig.Width, orig.Height, PixelFormat.Format24bppRgb))
                using (var g = Graphics.FromImage(bmp24))
                using (var ms = new MemoryStream())
                {
                    g.DrawImage(orig, 0, 0, bmp24.Width, bmp24.Height);
                    bmp24.Save(ms, ImageFormat.Bmp);
                    bmpData = ms.ToArray();
                }

                // extract header struct + raw pixels
                pixelData = new byte[bmpData.Length - headerSize];
                using (var rdr = new BinaryReader(new MemoryStream(bmpData)))
                {
                    var hdrBytes = rdr.ReadBytes(headerSize);
                    IntPtr ph = Marshal.AllocHGlobal(headerSize);
                    Marshal.Copy(hdrBytes, 0, ph, headerSize);
                    header = Marshal.PtrToStructure<BMPHeader>(ph);
                    Marshal.FreeHGlobal(ph);

                    rdr.Read(pixelData, 0, pixelData.Length);
                }

                // 2) LSB-encode
                if (message.Length > maxCapacity)
                {
                    LogActivity("Message is too big to be hidden!");
                    return;
                }

                byte[] outputData = new byte[pixelData.Length];
                var hH = GCHandle.Alloc(header, GCHandleType.Pinned);
                var pH = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                var oH = GCHandle.Alloc(outputData, GCHandleType.Pinned);
                try
                {
                    if (currentMethod == SteganographyMethod.StandardLSB)
                    {
                        if (!string.IsNullOrEmpty(password))
                            SteganographyWrapper.hide_message_shuffle(
                                pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                message, password,
                                oH.AddrOfPinnedObject()
                            );
                        else
                            SteganographyWrapper.hideMessage(
                                pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                message,
                                oH.AddrOfPinnedObject()
                            );
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(password))
                            SteganographyWrapper.hide_message_multichannel_shuffle(
                                pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                message, password,
                                oH.AddrOfPinnedObject()
                            );
                        else
                            SteganographyWrapper.hide_message_multichannel(
                                pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                message,
                                oH.AddrOfPinnedObject()
                            );
                    }
                }
                finally
                {
                    hH.Free(); pH.Free(); oH.Free();
                }

                // 3) Re-assemble and save as BMP or PNG
                var (ext, fmt) = GetOutputSettings();
                string outputPath = Path.ChangeExtension(inputPath, "_hidden" + ext);

                using (var outMs = new MemoryStream())
                {
                    outMs.Write(bmpData, 0, headerSize);
                    outMs.Write(outputData, 0, outputData.Length);
                    outMs.Position = 0;

                    using (var outImg = Image.FromStream(outMs))
                        outImg.Save(outputPath, fmt);
                }

                LogActivity($"Image saved at: {outputPath}");
                using (var bmp = new Bitmap(outputPath))
                {
                    modifiedImage = new Bitmap(bmp);
                    pictureBoxModified.Image = new Bitmap(bmp);
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
                string password = get_shuffle_key();

                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a cover image!");
                    return;
                }

                // 1) Load & convert to 24-bpp BMP bytes
                byte[] bmpData;
                int headerSize = Marshal.SizeOf(typeof(BMPHeader));
                BMPHeader header;
                byte[] pixelData;
                using (var image = Image.FromFile(inputPath))
                using (var bmp24 = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
                using (var g = Graphics.FromImage(bmp24))
                using (var ms = new MemoryStream())
                {
                    g.DrawImage(image, 0, 0, bmp24.Width, bmp24.Height);
                    bmp24.Save(ms, ImageFormat.Bmp);
                    bmpData = ms.ToArray();
                }

                pixelData = new byte[bmpData.Length - headerSize];
                using (var rdr = new BinaryReader(new MemoryStream(bmpData)))
                {
                    // pull out the header struct so we can re-prepend it later
                    var headerBytes = rdr.ReadBytes(headerSize);
                    IntPtr hp = Marshal.AllocHGlobal(headerSize);
                    Marshal.Copy(headerBytes, 0, hp, headerSize);
                    header = Marshal.PtrToStructure<BMPHeader>(hp);
                    Marshal.FreeHGlobal(hp);

                    // the rest is raw RGB24 data
                    rdr.Read(pixelData, 0, pixelData.Length);
                }

                // 2) Ask for file, pin buffers & call your native hideFile
                using (var ofd = new OpenFileDialog() { Title = "Select file to hide" })
                {
                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    var fileData = File.ReadAllBytes(ofd.FileName);
                    var fileName = Path.GetFileName(ofd.FileName);

                    if (fileData.Length > maxCapacity)
                    {
                        MessageBox.Show(
                          "The file is too large to hide in the selected image.",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                        );
                        return;
                    }

                    var outputData = new byte[pixelData.Length];

                    var hH = GCHandle.Alloc(header, GCHandleType.Pinned);
                    var pH = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                    var fH = GCHandle.Alloc(fileData, GCHandleType.Pinned);
                    var oH = GCHandle.Alloc(outputData, GCHandleType.Pinned);
                    try
                    {
                        if (currentMethod == SteganographyMethod.StandardLSB)
                        {
                            if (password.Length > 0)
                                SteganographyWrapper.hide_file_shuffle(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  password,
                                  oH.AddrOfPinnedObject()
                                );
                            else
                                SteganographyWrapper.hideFile(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  oH.AddrOfPinnedObject()
                                );
                        }
                        else
                        {
                            if (password.Length > 0)
                                SteganographyWrapper.hide_file_multichannel_shuffle(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  password,
                                  oH.AddrOfPinnedObject()
                                );
                            else
                                SteganographyWrapper.hide_file_multichannel(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  oH.AddrOfPinnedObject()
                                );
                        }
                    }
                    finally
                    {
                        hH.Free(); pH.Free(); fH.Free(); oH.Free();
                    }

                    // 3) Compose the final image bytes & save in the chosen format
                    var (ext, fmt) = GetOutputSettings();
                    string outputPath = Path.ChangeExtension(inputPath, "_hidden" + ext);

                    // write header + LSB-modified pixels into a MemoryStream
                    using (var outMs = new MemoryStream())
                    {
                        outMs.Write(bmpData, 0, headerSize);
                        outMs.Write(outputData, 0, outputData.Length);
                        outMs.Position = 0;

                        // rehydrate an Image and then save it as BMP or PNG
                        using (var outImg = Image.FromStream(outMs))
                        {
                            modifiedImage = new Bitmap(outImg);
                            outImg.Save(outputPath, fmt);
                        }
                    }

                    LogActivity($"Image saved at: {outputPath}");

                    // update preview
                    using (var bmp = new Bitmap(outputPath))
                        pictureBoxModified.Image = new Bitmap(bmp);
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

        private string get_shuffle_key()
        {
            string shuffleKey = textboxShuffle.Text;
            if(textboxShuffle.Text == Constants.DEFAULT_SHUFFLE_TEXT)
            {
                return string.Empty; // Return empty if no shuffle key is provided
            }
            return shuffleKey;
        }
        private void extract_message()
        {
            try
            {
                string inputPath = txtInputPath.Text;
                string password = get_shuffle_key();
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    txtOutput.Text = "Please provide the input image path!";
                    return;
                }

                // Ensure the input file is a BMP
                if (!inputPath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) &&
                    !inputPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    LogActivity("Error: Extraction is only supported for BMP and PNG files.");
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
                            if (password.Length != 0)
                            {
                                SteganographyWrapper.reveal_message_shuffle(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                   (uint)pixelData.Length,
                                    password,
                                    output,
                                    (uint)output.Capacity
                                );
                            }
                            else
                            {
                                // Call the single-channel extraction function
                                SteganographyWrapper.revealMessage(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                   (uint)pixelData.Length,
                                    output,
                                    (uint)output.Capacity
                                );
                            }
                        }
                        else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                        {
                            if (password.Length != 0)
                            {                                // Call the multi-channel extraction function
                                SteganographyWrapper.reveal_message_multichannel_shuffle(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                password,
                                output,
                               (uint)output.Capacity
                            );
                            }
                            else
                            {
                                // Call the multi-channel extraction function
                                SteganographyWrapper.reveal_message_multichannel(
                                pixelDataHandle.AddrOfPinnedObject(),
                                (uint)pixelData.Length,
                                output,
                               (uint)output.Capacity
                            );
                            }
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
                // 1) Read directly from whatever path the user has in the textbox
                string inputPath = txtInputPath.Text;
                string password = get_shuffle_key();
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
                    fileSize = (uint)fileData.Length;

                    GCHandle headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                    GCHandle pixelDataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
                    GCHandle fileDataHandle = GCHandle.Alloc(fileData, GCHandleType.Pinned);


                try
                {
                    if (currentMethod == SteganographyMethod.StandardLSB)
                    {
                            if (password.Length != 0)
                            {
                                // Call the single-channel hiding function with password
                                SteganographyWrapper.extract_file_shuffle(
                                       pixelDataHandle.AddrOfPinnedObject(),
                                       (uint)pixelData.Length,
                                    password,
                                       fileName,
                                       (uint)fileName.Capacity,
                                       fileDataHandle.AddrOfPinnedObject(),
                                       (uint)fileData.Length,
                                    ref fileSize
                                );
                            }
                            else
                            {
                                SteganographyWrapper.extractFile(
                                           pixelDataHandle.AddrOfPinnedObject(),
                                           (uint)pixelData.Length,
                                           fileName,
                                           (uint)fileName.Capacity,
                                           fileDataHandle.AddrOfPinnedObject(),
                                           (uint)fileData.Length,
                                    ref fileSize
                                );

                            }
                    }
                        else if (currentMethod == SteganographyMethod.MultiChannelLSB)
                    {
                            if (password.Length != 0)
                            {
                                SteganographyWrapper.extract_file_multichannel_shuffle(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                    (uint)pixelData.Length,
                                    password,
                                    fileName,
                                    (uint)fileName.Capacity,
                                    fileDataHandle.AddrOfPinnedObject(),
                                    (uint)fileData.Length,
                                    ref fileSize
                                );
                            }
                            else
                                SteganographyWrapper.extract_file_multichannel(
                                    pixelDataHandle.AddrOfPinnedObject(),
                                    (uint)pixelData.Length,
                                    fileName,
                                    (uint)fileName.Capacity,
                                    fileDataHandle.AddrOfPinnedObject(),
                                    (uint)fileData.Length,
                                    ref fileSize
                                );
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
            cmbOutputFormat.SelectedIndex = 0;
            // Reset internal variables
            currentMethod = SteganographyMethod.StandardLSB;
            currentMode = OperationMode.Message;
            originalImage = null;
            modifiedImage = null;
            textboxShuffle.Text = Constants.DEFAULT_SHUFFLE_TEXT;
            textboxShuffle.ForeColor = Color.Gray; // Set text color to gray to indicate placeholder
            textboxEncryptionKey.Text= Constants.DEFAULT_ENCRYPTION_TEXT;
            textboxEncryptionKey.ForeColor = Color.Gray; // Set text color to gray to indicate placeholder
            // Reset capacity info
            lblCapacityInfo.Text = string.Empty;

            // Log reset action
            LogActivity("Application reset to initial state.");
        }

        private void textboxShuffle_Enter(object sender, EventArgs e)
        {
            if(textboxShuffle.Text == Constants.DEFAULT_SHUFFLE_TEXT)
            {
                textboxShuffle.Text = string.Empty;
                textboxShuffle.ForeColor = Color.Black; // Reset text color to default
            }
        }

        private void textboxShuffle_Leave(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textboxShuffle.Text))
            {
                textboxShuffle.Text = Constants.DEFAULT_SHUFFLE_TEXT;
                textboxShuffle.ForeColor = Color.Gray; // Set text color to gray to indicate placeholder
            }
        }

        private void textboxEncryptionKey_Enter(object sender, EventArgs e)
        {
            if (textboxEncryptionKey.Text == Constants.DEFAULT_ENCRYPTION_TEXT)
            {
                textboxEncryptionKey.Text = string.Empty;
                textboxEncryptionKey.ForeColor = Color.Black; // Reset text color to default
            }
        }

        private void textboxEncryptionKey_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textboxEncryptionKey.Text))
            {
                textboxEncryptionKey.Text = Constants.DEFAULT_ENCRYPTION_TEXT;
                textboxEncryptionKey.ForeColor = Color.Gray; // Set text color to gray to indicate placeholder
            }
        }
        private (string ext, ImageFormat fmt) GetOutputSettings()
        {
            if (cmbOutputFormat.SelectedItem.ToString() == "PNG")
                return (".png", ImageFormat.Png);
            else
                return (".bmp", ImageFormat.Bmp);
        }
    }

    static class Constants
    {
        public const string DEFAULT_SHUFFLE_TEXT = "Shuffle key";
        public const string DEFAULT_ENCRYPTION_TEXT = "Encryption key";
    }

}
