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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
    public enum SteganographyType
    {
        Image,
        Audio
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

    public partial class AplicatieSteganografie : Form
    {   

        private System.Windows.Forms.PictureBox pictureBoxOriginal;
        private System.Windows.Forms.PictureBox pictureBoxModified;
        private System.Windows.Forms.Button btnCompareImages;
        private Bitmap originalImage;
        private Bitmap modifiedImage;
        // for audio comparison
        private byte[] originalSamples;
        private byte[] modifiedSamples;
        //private System.Windows.Forms.Button btnHideFile;
        //private System.Windows.Forms.Button btnExtractFile;

        private SteganographyMethod currentMethod = SteganographyMethod.StandardLSB;
        private SteganographyType currentType = SteganographyType.Image;
        private OperationMode currentMode = OperationMode.Message;
        /*Global variables.*/
        long maxCapacity;
        string processingPath;
        public AplicatieSteganografie()
        {
            InitializeComponent();
            cmbType.SelectedIndex = 0; 
            cmbOutputFormat.SelectedIndex = 0; 
            currentMethod = SteganographyMethod.StandardLSB;
            // Initialize UI based on mode
            SetOperationMode(OperationMode.Message);
            cmbMethod.SelectedIndexChanged += (sender, e) =>
            {
                currentMethod = (SteganographyMethod)cmbMethod.SelectedIndex;
            };
            cmbType.SelectedIndexChanged += (sender, e) =>
            {
                currentType = (SteganographyType)cmbType.SelectedIndex;
            };
        }
        private void SetOperationMode(OperationMode mode)
        {
            currentMode = mode;

            // Show/hide controls based on mode
            txtMessage.Visible = mode == OperationMode.Message;
            label3.Visible = mode == OperationMode.Message;
            //txtOutput_path.Visible = mode == OperationMode.File;
            // Update UI text
            btnHideMessage.Text = mode == OperationMode.Message ? "Hide Message" : "Hide File";
            btnRevealMessage.Text = mode == OperationMode.Message ? "Extract Message" : "Extract File";
        }

        private void update_operation_type()
        {
            cmbOutputFormat.Visible = currentType == SteganographyType.Image;
            labeSaveOutput.Visible = currentType == SteganographyType.Image;
            btnCompareImages.Text = currentType == SteganographyType.Image ? "Compare Images" : "Compare audio";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            // 1) Choose the right filter
            string filter = currentType == SteganographyType.Audio
                ? "WAV files|*.wav|All Files|*.*"
                : "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif|All Files|*.*";

            // 2) Show dialog
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = filter;
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                string inputPath = ofd.FileName;
                txtInputPath.Text = inputPath;
                processingPath = inputPath;
            }

            if (currentType == SteganographyType.Image)
            {
                // --- Image path handling (unchanged) ---
                // Always convert to 24-bpp BMP
                string bmpPath = ConvertTo24bppBmp(processingPath);
                if (bmpPath == null)
                {
                    LogActivity("Error: Failed to convert image to 24-bpp BMP.");
                    return;
                }
                processingPath = bmpPath;
                originalImage = new Bitmap(processingPath);
                pictureBoxOriginal.Image = originalImage;
                UpdateCapacityInfo(processingPath);
            }
            else
            {
                // --- WAV path handling ---
                try
                {
                    // read header + skip directly to samples
                    using (var fs = new FileStream(processingPath, FileMode.Open, FileAccess.Read))
                    using (var br = new BinaryReader(fs))
                    {
                        WavHeader hdr = WavHelper.ReadWavHeader(br);

                        // compute capacity: one bit per PCM sample
                        long bytesPerSample = hdr.BitsPerSample / 8;
                        long totalSamples = hdr.DataSize / bytesPerSample;
                        maxCapacity = totalSamples / 8;
                        if (hdr.BitsPerSample == 16)
                        {
                            maxCapacity = maxCapacity / 2;
                        }
                        lblCapacityInfo.Text =
                            $"Can hide: ~{maxCapacity} bytes ({maxCapacity / 1024.0:N1} KB)";
                        lblCapacityInfo.ForeColor =
                            maxCapacity > 0 ? SystemColors.ControlText : Color.Red;

                        // render WAV metadata into pictureBoxOriginal
                        var lines = new[]
                        {
            $"File: {Path.GetFileName(processingPath)}",
            $"Channels: {hdr.NumChannels}",
            $"Bits/Sample: {hdr.BitsPerSample}",
            $"Sample Rate: {hdr.SampleRate} Hz",
            $"Data Size: {hdr.DataSize} bytes"
        };
                        // ensure we have a minimally sensible render area
                        var target = pictureBoxOriginal.ClientSize;
                        if (target.Width < 10) target.Width = 200;
                        if (target.Height < 10) target.Height = 120;
                        pictureBoxOriginal.Image = RenderTextInfo(target, lines);

                        LogActivity(
                            $"Loaded WAV: {hdr.NumChannels}ch, {hdr.BitsPerSample}-bit @ {hdr.SampleRate}Hz, data={hdr.DataSize}B"
                        );
                    }
                }
                catch (Exception ex)
                {
                    LogActivity("Error reading WAV: " + ex.Message);
                    lblCapacityInfo.Text = "Capacity info unavailable";
                    lblCapacityInfo.ForeColor = Color.Red;
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
                if(currentMode == OperationMode.File)
                {
                    maxCapacity -= 50;
                }
                lblCapacityInfo.Text = $"Can hide: ~{maxCapacity} bytes ({maxCapacity/1024} KB)";
                lblCapacityInfo.ForeColor = maxCapacity > 0 ? SystemColors.ControlText : Color.Red;
            }
            catch
            {
                lblCapacityInfo.Text = "Capacity info unavailable";
                lblCapacityInfo.ForeColor = Color.Red;
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

        private void hideFileInWav()
        {
            try
            {
                var inputPath = txtInputPath.Text;
                string password = get_shuffle_key();
                if (string.IsNullOrWhiteSpace(inputPath)) { LogActivity("Pick a WAV first"); return; }

                var (hdr, samples) = WavHelper.ReadWavFile(inputPath);
                originalSamples = samples;

                byte[] payload;
                string payloadName;
                using (var ofd = new OpenFileDialog { Title = "Select file to hide" })
                {
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    payload = File.ReadAllBytes(ofd.FileName);
                    payloadName = Path.GetFileName(ofd.FileName);
                }

                byte[] wavData = File.ReadAllBytes(inputPath);
                byte[] outputData = new byte[wavData.Length];
                if(payload.Length > maxCapacity)
                {
                    LogActivity("File is too large to be hidden!");
                    return;
                }
                var hW = GCHandle.Alloc(wavData, GCHandleType.Pinned);
                var hP = GCHandle.Alloc(payload, GCHandleType.Pinned);
                var hO = GCHandle.Alloc(outputData, GCHandleType.Pinned);
                try
                {
                    if (password.Length > 0)
                    {
                        SteganographyWrapper.hideFileShuffleInWav(
                            hW.AddrOfPinnedObject(), (uint)wavData.Length,
                            payloadName,
                            hP.AddrOfPinnedObject(), (uint)payload.Length,
                            password,  
                            hO.AddrOfPinnedObject()
                        );
                    }
                    else
                    {
                        SteganographyWrapper.hideFileInWav(
                            hW.AddrOfPinnedObject(), (uint)wavData.Length,
                            payloadName,
                            hP.AddrOfPinnedObject(), (uint)payload.Length,
                            hO.AddrOfPinnedObject()
                        );
                    }
                }
                finally { hW.Free(); hP.Free(); hO.Free(); }

                // 5) Save it
                var outPath = Path.Combine(
                    Path.GetDirectoryName(inputPath),
                    Path.GetFileNameWithoutExtension(inputPath) + "_hidden.wav"
                );
                File.WriteAllBytes(outPath, outputData);
                LogActivity($"WAV hidden → {outPath}");

                // 6) Render metadata + filename
                var lines = new[]
                {
            $"Song: {Path.GetFileName(inputPath)}",
            $"Channels: {hdr.NumChannels}",
            $"Bits/Sample: {hdr.BitsPerSample}",
            $"SampleRate: {hdr.SampleRate} Hz",
            $"DataSize: {hdr.DataSize} bytes",
            $"Payload: {payloadName} ({payload.Length:N0} B)"
        };
                var target = pictureBoxModified.ClientSize;
                if (target.Width < 10 || target.Height < 10)
                    target = new Size(200, 120);
                pictureBoxModified.Image = RenderTextInfo(target, lines);

                // stash samples for any future diff
                modifiedSamples = WavHelper.ReadWavFile(outPath).samples;
            }
            catch (Exception ex)
            {
                LogActivity($"Error in hideFileInWav: {ex.Message}");
            }
        }
        private void hide_message_wav()
        {
            try
            {
                // 1) sanity checks
                var inputPath = txtInputPath.Text;
                var message = txtMessage.Text;
                string password = get_shuffle_key();
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a WAV first!");
                    return;
                }
                if (string.IsNullOrEmpty(message))
                {
                    LogActivity("Please enter a message to hide!");
                    return;
                }

                // 2) load WAV + samples to compute capacity
                var (hdr, samples) = WavHelper.ReadWavFile(inputPath);
                originalSamples = samples;
                long capacity = samples.Length / 8;
                if (message.Length + 1 > capacity)
                {
                    MessageBox.Show(
                        $"Message too long: can hide {capacity:N0} bytes, tried {message.Length + 1:N0}.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return;
                }

                // 3) read raw bytes for P/Invoke
                byte[] wavData = File.ReadAllBytes(inputPath);
                byte[] outputData = new byte[wavData.Length];

                // 4) pin & call native hideMessageInWav
                var hIn = GCHandle.Alloc(wavData, GCHandleType.Pinned);
                var hOut = GCHandle.Alloc(outputData, GCHandleType.Pinned);
                try
                {
                    if (password.Length > 0)
                    {
                        SteganographyWrapper.hideMessageShuffleInWav(
                        hIn.AddrOfPinnedObject(),
                        (uint)wavData.Length,
                        message,
                        password,
                        hOut.AddrOfPinnedObject()
                    );
                    }
                    else
                    {
                        SteganographyWrapper.hideMessageInWav(
                            hIn.AddrOfPinnedObject(),
                            (uint)wavData.Length,
                            message,
                            hOut.AddrOfPinnedObject()
                        );
                    }
                }
                finally
                {
                    hIn.Free();
                    hOut.Free();
                }

                // 5) write out the stego‐WAV
                string outPath = Path.Combine(
                    Path.GetDirectoryName(inputPath),
                    Path.GetFileNameWithoutExtension(inputPath) + "_msg_hidden.wav"
                );
                File.WriteAllBytes(outPath, outputData);
                LogActivity($"WAV with hidden message written to: {outPath}");

                // 6) stash & render metadata
                modifiedSamples = WavHelper.ReadWavFile(outPath).samples;
                var lines = new[]
                {
            $"File: {Path.GetFileName(outPath)}",
            $"Channels:    {hdr.NumChannels}",
            $"Bits/Sample: {hdr.BitsPerSample}",
            $"SampleRate:  {hdr.SampleRate} Hz",
            $"DataSize:    {hdr.DataSize:N0} bytes",
            $"Message:     \"{message}\""
        };
                pictureBoxModified.Image = RenderTextInfo(pictureBoxModified.ClientSize, lines);
            }
            catch (Exception ex)
            {
                LogActivity($"Error in hide_message_wav: {ex.Message}");
            }
        }

        private void hide_text_message()
        {
            if (currentType == SteganographyType.Audio)
            {
                hide_message_wav();
                return;
            }
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
            if(currentType == SteganographyType.Audio)
            {
                hideFileInWav();
                return;
            }
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
                            {
                                SteganographyWrapper.hide_file_shuffle(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  password,
                                  oH.AddrOfPinnedObject()
                                );
                            }
                            else
                            {
                                SteganographyWrapper.hideFile(
                                  pH.AddrOfPinnedObject(), (uint)pixelData.Length,
                                  fileName,
                                  fH.AddrOfPinnedObject(), (uint)fileData.Length,
                                  oH.AddrOfPinnedObject()
                                );
                            }
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

        private void extractFileFromWav()
        {
            try
            {
                string inputPath = txtInputPath.Text;
                string password = get_shuffle_key();

                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a stego WAV!");
                    return;
                }

                // 1) Load the entire stego‐WAV
                byte[] wavData = File.ReadAllBytes(inputPath);

                // 2) Prepare extraction buffers
                var nameBuf = new StringBuilder(260);
                byte[] payloadBuf = new byte[wavData.Length];
                uint extractedSize;

                // 3) Pin and call into native DLL
                var hWav = GCHandle.Alloc(wavData, GCHandleType.Pinned);
                var hPay = GCHandle.Alloc(payloadBuf, GCHandleType.Pinned);
                try
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        SteganographyWrapper.extractFileShuffleFromWav(
                            hWav.AddrOfPinnedObject(),
                            (uint)wavData.Length,
                            password,
                            nameBuf,
                            (uint)nameBuf.Capacity,
                            hPay.AddrOfPinnedObject(),
                            (uint)payloadBuf.Length,
                            out extractedSize
                        );
                    }
                    else
                    {
                        SteganographyWrapper.extractFileFromWav(
                            hWav.AddrOfPinnedObject(),
                            (uint)wavData.Length,
                            nameBuf,
                            (uint)nameBuf.Capacity,
                            hPay.AddrOfPinnedObject(),
                            (uint)payloadBuf.Length,
                            out extractedSize
                        );
                    }
                }
                finally
                {
                    hWav.Free();
                    hPay.Free();
                }

                // 4) If nothing valid was decoded, warn and bail out
                string extractedName = nameBuf.ToString();
                if (string.IsNullOrWhiteSpace(extractedName) || extractedSize == 0)
                {
                    MessageBox.Show(
                        "No hidden file found (either none was embedded, or the password is wrong).",
                        "Nothing to Extract",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // 5) Otherwise write out the real file
                string outFile = Path.Combine(
                    Path.GetDirectoryName(inputPath),
                    extractedName
                );
                File.WriteAllBytes(outFile, payloadBuf.Take((int)extractedSize).ToArray());
                LogActivity($"File extracted from WAV to: {outFile}");
                txtOutput.Text = outFile;
            }
            catch (Exception ex)
            {
                LogActivity($"Error in extractFileFromWav: {ex.Message}");
            }
        }

        private void extract_message_wav()
        {
            try
            {
                // 1) sanity check
                var inputPath = txtInputPath.Text;
                string password = get_shuffle_key();
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    LogActivity("Please select a stego‐WAV first!");
                    return;
                }

                // 2) read raw bytes
                byte[] wavData = File.ReadAllBytes(inputPath);
                // use previously computed maxCapacity (samples.Length/8)
                var sb = new StringBuilder((int)maxCapacity);

                // 3) pin & call native revealMessageFromWav
                var hIn = GCHandle.Alloc(wavData, GCHandleType.Pinned);
                try
                {
                    if (password.Length > 0)
                    {
                        SteganographyWrapper.revealMessageShuffleFromWav(
                            hIn.AddrOfPinnedObject(),
                            (uint)wavData.Length,
                            password,
                            sb,
                            (uint)sb.Capacity
                        );
                    }
                    else
                    {
                        SteganographyWrapper.revealMessageFromWav(
                            hIn.AddrOfPinnedObject(),
                            (uint)wavData.Length,
                            sb,
                            (uint)sb.Capacity
                        );
                    }
                }
                finally
                {
                    hIn.Free();
                }

                // 4) display
                txtOutput.Text = sb.ToString().TrimEnd('\0');
                LogActivity("Message extracted successfully from WAV.");
            }
            catch (Exception ex)
            {
                LogActivity($"Error in extract_message_wav: {ex.Message}");
            }
        }

        private void extract_message()
        {
            if(currentType == SteganographyType.Audio)
            {
                extract_message_wav();
                return;
            }
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
            if (currentType == SteganographyType.Audio)
            {
                extractFileFromWav();
                return;
            }
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
                        txtOutput.Text = outputPath;
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
            btnCompareImages.Text = "Comparing...";
            btnCompareImages.Enabled = false;
            progressBarCompare.Visible = true;

            if (currentType == SteganographyType.Audio)
            {
                if (originalSamples == null || modifiedSamples == null)
                {
                    MessageBox.Show("Please load and process both audio files first.");
                }
                else
                {
                    int N = Math.Min(originalSamples.Length, modifiedSamples.Length);
                    int diffs = 0;
                    for (int i = 0; i < N; i++)
                        if ((originalSamples[i] & 1) != (modifiedSamples[i] & 1))
                            diffs++;

                    double pct = 100.0 * diffs / N;
                    MessageBox.Show(
                        $"{diffs:N0} samples out of {N:N0} changed their LSBs\n({pct:F2}% difference)",
                        "Audio LSB Difference",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                // restore UI state
                btnCompareImages.Text = "Compare";
                btnCompareImages.Enabled = true;
                progressBarCompare.Visible = false;
                return;
            }

            // --- IMAGE CASE (your existing code) ---
            if (originalImage == null || modifiedImage == null)
            {
                MessageBox.Show("Please load and process both images first.");
                goto CLEANUP;
            }

            progressBarCompare.Minimum = 0;
            progressBarCompare.Maximum = originalImage.Height;
            progressBarCompare.Value = 0;

            var diffImage = new Bitmap(originalImage.Width, originalImage.Height);

            await Task.Run(() =>
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    for (int x = 0; x < originalImage.Width; x++)
                    {
                        var pO = originalImage.GetPixel(x, y);
                        var pM = modifiedImage.GetPixel(x, y);

                        if (pO != pM)
                            diffImage.SetPixel(x, y, Color.Red);
                        else
                            diffImage.SetPixel(x, y, pO);
                    }
                    Invoke((Action)(() => progressBarCompare.Value = y + 1));
                }
            });

            progressBarCompare.Visible = false;

            // show diff
            {
                var preview = new Form
                {
                    Text = "Differences Highlighted",
                    StartPosition = FormStartPosition.CenterScreen,
                    ClientSize = new Size(1000, 700)
                };
                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = diffImage,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                preview.Controls.Add(pb);
                preview.Show();
            }

        DONE:
            btnCompareImages.Text = "Compare";
            btnCompareImages.Enabled = true;
            return;

        CLEANUP:
            progressBarCompare.Visible = false;
            btnCompareImages.Text = "Compare";
            btnCompareImages.Enabled = true;
        }

        private void rdbMessage_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbMessage.Checked)
            {
                SetOperationMode(OperationMode.Message);
                label5.Text = "Hidden message";
            }
            update_operation_type();
        }

        private void rdbFile_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbFile.Checked)
            {
                SetOperationMode(OperationMode.File);
                cmbOutputFormat.Visible = false; 
                labeSaveOutput.Visible = false;
                label5.Text = "Extracted file path";
            }
            update_operation_type();
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
            cmbType.SelectedIndex = 0; // Reset to Image type
            // Reset internal variables
            currentMethod = SteganographyMethod.StandardLSB;
            currentMode = OperationMode.Message;
            originalImage = null;
            modifiedImage = null;
            textboxShuffle.Text = Constants.DEFAULT_SHUFFLE_TEXT;
            textboxShuffle.ForeColor = Color.Gray; // Set text color to gray to indicate placeholder
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


        private (string ext, ImageFormat fmt) GetOutputSettings()
        {
            if (cmbOutputFormat.SelectedItem.ToString() == "PNG")
                return (".png", ImageFormat.Png);
            else
                return (".bmp", ImageFormat.Bmp);
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
            if (!string.IsNullOrEmpty(processingPath))
            {
                UpdateCapacityInfo(processingPath);
            }
        }
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1) Update your enum & log
            currentType = (SteganographyType)cmbType.SelectedIndex;
            LogActivity(currentType == SteganographyType.Audio ? "Switched to audio."
                                                                : "Switched to image.");

            // 2) Rebuild the Technique list
            cmbMethod.BeginUpdate();
            cmbMethod.Items.Clear();
            cmbMethod.Items.Add("Standard LSB (Blue Channel)");
            if (currentType == SteganographyType.Image)
            {
                cmbMethod.Items.Add("Multi-Channel LSB (R+G+B)");
            }
            cmbMethod.EndUpdate();

            // 3) Always pick the first entry so we never have an invalid selection
            cmbMethod.SelectedIndex = 0;

            // 4) Tweak any other UI bits
            update_operation_type();
        }
        private Image RenderTextInfo(Size sz, string[] lines)
        {
            // ensure we have a valid drawing area
            int w = Math.Max(1, sz.Width);
            int h = Math.Max(1, sz.Height);

            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);

                // set up font and brush
                using (var font = new Font("Consolas", 10))
                using (var brush = new SolidBrush(Color.Black))
                {
                    // combine all your lines into one string
                    string text = string.Join(Environment.NewLine, lines);

                    // define a rectangle inset by 4px on all sides
                    var layout = new RectangleF(4, 4, w - 8, h - 8);

                    // a StringFormat that wraps text at word boundaries
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near,
                        FormatFlags = 0,                    // no special flags
                        Trimming = StringTrimming.Word      // trim by word if it absolutely overflows
                    };

                    g.DrawString(text, font, brush, layout, sf);
                }
            }

            return bmp;
        }

        string ConvertTo24bppBmp(string inputPath)
        {
            try
            {
                string bmpPath = Path.ChangeExtension(Path.GetTempFileName(), ".bmp");
                using (var src = Image.FromFile(inputPath))
                using (var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb))
                using (var g = Graphics.FromImage(dst))
                {
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                    dst.Save(bmpPath, ImageFormat.Bmp);
                }
                return bmpPath;
            }
            catch (Exception ex)
            {
                LogActivity($"Conversion failed: {ex.Message}");
                return null;
            }
        }

        private void textboxEncryptionKey_Enter(object sender, EventArgs e)
        {

        }

        private void textboxEncryptionKey_Leave(object sender, EventArgs e)
        {

        }
    }

    static class Constants
    {
        public const string DEFAULT_SHUFFLE_TEXT = "Shuffle key";
        public const string DEFAULT_ENCRYPTION_TEXT = "Encryption key";
    }


        public struct WavHeader
        {
            // RIFF chunk descriptor
            public uint FileSize;      // overall file size minus 8 bytes
            public uint Format;        // "WAVE" as uint32

            // fmt sub-chunk
            public ushort AudioFormat;   // PCM = 1
            public ushort NumChannels;   // mono = 1, stereo = 2
            public uint SampleRate;    // 44100, 48000, etc
            public uint ByteRate;      // == SampleRate * NumChannels * BitsPerSample/8
            public ushort BlockAlign;    // == NumChannels * BitsPerSample/8
            public ushort BitsPerSample; // 8, 16, 24, etc

            // data sub-chunk
            public uint DataSize;      // number of bytes of PCM data
        }

    public static class WavHelper
    {
        public static WavHeader ReadWavHeader(BinaryReader br)
        {
            // 1) RIFF header
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "RIFF")
                throw new InvalidDataException("Not a RIFF file");
            br.ReadUInt32(); // skip file size
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "WAVE")
                throw new InvalidDataException("Not a WAVE file");

            var hdr = new WavHeader();

            // 2) scan chunks until we find "data"
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                string chunkId = Encoding.ASCII.GetString(br.ReadBytes(4));
                uint chunkSize = br.ReadUInt32();

                if (chunkId == "fmt ")
                {
                    hdr.AudioFormat = br.ReadUInt16();
                    hdr.NumChannels = br.ReadUInt16();
                    hdr.SampleRate = br.ReadUInt32();
                    hdr.ByteRate = br.ReadUInt32();
                    hdr.BlockAlign = br.ReadUInt16();
                    hdr.BitsPerSample = br.ReadUInt16();
                    if (chunkSize > 16)
                        br.ReadBytes((int)(chunkSize - 16));
                }
                else if (chunkId == "data")
                {
                    hdr.DataSize = chunkSize;
                    return hdr;
                }
                else
                {
                    br.ReadBytes((int)chunkSize);
                }
            }

            throw new InvalidDataException("WAV data chunk not found");
        }

        public static void WriteWavHeader(BinaryWriter bw, WavHeader hdr)
        {
            // RIFF
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            // file-size = 4("WAVE") + (8+16) + (8+dataSize)
            uint fileSizeMinus8 = 4 + (8 + 16) + (8 + hdr.DataSize);
            bw.Write(fileSizeMinus8);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write((uint)16);
            bw.Write(hdr.AudioFormat);
            bw.Write(hdr.NumChannels);
            bw.Write(hdr.SampleRate);
            bw.Write(hdr.ByteRate);
            bw.Write(hdr.BlockAlign);
            bw.Write(hdr.BitsPerSample);

            // data chunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(hdr.DataSize);
        }

        public static (WavHeader header, byte[] samples) ReadWavFile(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                WavHeader hdr = ReadWavHeader(br);
                byte[] samples = br.ReadBytes((int)hdr.DataSize);
                return (hdr, samples);
            }
        }

        public static void WriteWavFile(string path, WavHeader hdr, byte[] samples)
        {
            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                hdr.DataSize = (uint)samples.Length;
                hdr.ByteRate = (uint)(hdr.SampleRate * hdr.NumChannels * hdr.BitsPerSample / 8);
                hdr.BlockAlign = (ushort)(hdr.NumChannels * hdr.BitsPerSample / 8);

                WriteWavHeader(bw, hdr);
                bw.Write(samples);
            }
        }
    }
}


