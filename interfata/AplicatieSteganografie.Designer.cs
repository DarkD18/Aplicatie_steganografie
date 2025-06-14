using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace interfata
{
    partial class AplicatieSteganografie
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnHideMessage = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.btnRevealMessage = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtActivity = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.pictureBoxOriginal = new System.Windows.Forms.PictureBox();
            this.pictureBoxModified = new System.Windows.Forms.PictureBox();
            this.btnCompareImages = new System.Windows.Forms.Button();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.grpOperationMode = new System.Windows.Forms.GroupBox();
            this.rdbMessage = new System.Windows.Forms.RadioButton();
            this.rdbFile = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.lblCapacityInfo = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.progressBarCompare = new System.Windows.Forms.ProgressBar();
            this.textboxShuffle = new System.Windows.Forms.TextBox();
            this.labeSaveOutput = new System.Windows.Forms.Label();
            this.cmbOutputFormat = new System.Windows.Forms.ComboBox();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupPaths = new System.Windows.Forms.GroupBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip3 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip4 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip5 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip6 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip7 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip8 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModified)).BeginInit();
            this.grpOperationMode.SuspendLayout();
            this.groupPaths.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input path";
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(126, 102);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(443, 22);
            this.txtInputPath.TabIndex = 1;
            this.toolTip5.SetToolTip(this.txtInputPath, "Cale imagine sau audio cover");
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(575, 102);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseInput.TabIndex = 2;
            this.btnBrowseInput.Text = "browse";
            this.toolTip1.SetToolTip(this.btnBrowseInput, "Alege fișierul de intrare (imagine sau WAV)");
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Message";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(126, 192);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(443, 22);
            this.txtMessage.TabIndex = 7;
            this.toolTip3.SetToolTip(this.txtMessage, "Mesaj de ascuns");
            // 
            // btnHideMessage
            // 
            this.btnHideMessage.Location = new System.Drawing.Point(575, 188);
            this.btnHideMessage.Name = "btnHideMessage";
            this.btnHideMessage.Size = new System.Drawing.Size(129, 30);
            this.btnHideMessage.TabIndex = 8;
            this.btnHideMessage.Text = "hide message";
            this.btnHideMessage.UseVisualStyleBackColor = true;
            this.btnHideMessage.Click += new System.EventHandler(this.btn_hide);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(126, 235);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(443, 22);
            this.txtOutput.TabIndex = 9;
            this.toolTip4.SetToolTip(this.txtOutput, "Mesaj extras");
            // 
            // btnRevealMessage
            // 
            this.btnRevealMessage.Location = new System.Drawing.Point(575, 232);
            this.btnRevealMessage.Name = "btnRevealMessage";
            this.btnRevealMessage.Size = new System.Drawing.Size(129, 28);
            this.btnRevealMessage.TabIndex = 10;
            this.btnRevealMessage.Text = "Reveal message";
            this.btnRevealMessage.UseVisualStyleBackColor = true;
            this.btnRevealMessage.Click += new System.EventHandler(this.btn_extract);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(0, 238);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 16);
            this.label5.TabIndex = 12;
            this.label5.Text = "Hidden message";
            // 
            // txtActivity
            // 
            this.txtActivity.Location = new System.Drawing.Point(710, 64);
            this.txtActivity.Name = "txtActivity";
            this.txtActivity.Size = new System.Drawing.Size(729, 164);
            this.txtActivity.TabIndex = 14;
            this.txtActivity.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1087, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 16);
            this.label6.TabIndex = 15;
            this.label6.Text = "Activity log";
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(69)))), ((int)(((byte)(97)))));
            this.btnExit.Location = new System.Drawing.Point(1284, 773);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(155, 37);
            this.btnExit.TabIndex = 16;
            this.btnExit.Text = "EXIT";
            this.toolTip8.SetToolTip(this.btnExit, "Paraseste aplicatia");
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pictureBoxOriginal
            // 
            this.pictureBoxOriginal.Image = global::interfata.Properties.Resources.placeholder_image_original;
            this.pictureBoxOriginal.Location = new System.Drawing.Point(26, 293);
            this.pictureBoxOriginal.Name = "pictureBoxOriginal";
            this.pictureBoxOriginal.Size = new System.Drawing.Size(600, 500);
            this.pictureBoxOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxOriginal.TabIndex = 17;
            this.pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxModified
            // 
            this.pictureBoxModified.Image = global::interfata.Properties.Resources.placeholder_image_result;
            this.pictureBoxModified.Location = new System.Drawing.Point(662, 293);
            this.pictureBoxModified.Name = "pictureBoxModified";
            this.pictureBoxModified.Size = new System.Drawing.Size(600, 500);
            this.pictureBoxModified.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxModified.TabIndex = 18;
            this.pictureBoxModified.TabStop = false;
            // 
            // btnCompareImages
            // 
            this.btnCompareImages.Location = new System.Drawing.Point(1284, 343);
            this.btnCompareImages.Name = "btnCompareImages";
            this.btnCompareImages.Size = new System.Drawing.Size(155, 29);
            this.btnCompareImages.TabIndex = 19;
            this.btnCompareImages.Text = "Compare images";
            this.toolTip6.SetToolTip(this.btnCompareImages, "Vezi diferentele dintre original si modificat");
            this.btnCompareImages.Click += new System.EventHandler(this.btnCompareImages_Click);
            // 
            // cmbMethod
            // 
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Items.AddRange(new object[] {
            "Standard LSB (Blue Channel)",
            "Multi-Channel LSB (R+G+B)"});
            this.cmbMethod.Location = new System.Drawing.Point(353, 30);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(250, 24);
            this.cmbMethod.TabIndex = 23;
            this.cmbMethod.SelectedIndexChanged += new System.EventHandler(this.cmbMethod_SelectedIndexChanged);
            // 
            // grpOperationMode
            // 
            this.grpOperationMode.Controls.Add(this.rdbMessage);
            this.grpOperationMode.Controls.Add(this.rdbFile);
            this.grpOperationMode.Location = new System.Drawing.Point(20, 10);
            this.grpOperationMode.Name = "grpOperationMode";
            this.grpOperationMode.Size = new System.Drawing.Size(152, 56);
            this.grpOperationMode.TabIndex = 24;
            this.grpOperationMode.TabStop = false;
            this.grpOperationMode.Text = "Operation Mode";
            // 
            // rdbMessage
            // 
            this.rdbMessage.AutoSize = true;
            this.rdbMessage.Checked = true;
            this.rdbMessage.Location = new System.Drawing.Point(6, 21);
            this.rdbMessage.Name = "rdbMessage";
            this.rdbMessage.Size = new System.Drawing.Size(85, 20);
            this.rdbMessage.TabIndex = 0;
            this.rdbMessage.TabStop = true;
            this.rdbMessage.Text = "Message";
            this.rdbMessage.UseVisualStyleBackColor = true;
            this.rdbMessage.CheckedChanged += new System.EventHandler(this.rdbMessage_CheckedChanged);
            // 
            // rdbFile
            // 
            this.rdbFile.AutoSize = true;
            this.rdbFile.Location = new System.Drawing.Point(100, 21);
            this.rdbFile.Name = "rdbFile";
            this.rdbFile.Size = new System.Drawing.Size(50, 20);
            this.rdbFile.TabIndex = 1;
            this.rdbFile.Text = "File";
            this.rdbFile.UseVisualStyleBackColor = true;
            this.rdbFile.CheckedChanged += new System.EventHandler(this.rdbFile_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(409, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 16);
            this.label4.TabIndex = 25;
            this.label4.Text = "Choose tehnique";
            // 
            // lblCapacityInfo
            // 
            this.lblCapacityInfo.AutoSize = true;
            this.lblCapacityInfo.Location = new System.Drawing.Point(196, 67);
            this.lblCapacityInfo.Name = "lblCapacityInfo";
            this.lblCapacityInfo.Size = new System.Drawing.Size(0, 16);
            this.lblCapacityInfo.TabIndex = 25;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(1289, 737);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(150, 30);
            this.btnReset.TabIndex = 26;
            this.btnReset.Text = "Reset";
            this.toolTip7.SetToolTip(this.btnReset, "Reseteaza aplicatia la starea initiala");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // progressBarCompare
            // 
            this.progressBarCompare.Location = new System.Drawing.Point(1284, 378);
            this.progressBarCompare.Name = "progressBarCompare";
            this.progressBarCompare.Size = new System.Drawing.Size(155, 22);
            this.progressBarCompare.TabIndex = 28;
            this.progressBarCompare.Visible = false;
            // 
            // textboxShuffle
            // 
            this.textboxShuffle.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textboxShuffle.Location = new System.Drawing.Point(297, 67);
            this.textboxShuffle.Name = "textboxShuffle";
            this.textboxShuffle.Size = new System.Drawing.Size(176, 22);
            this.textboxShuffle.TabIndex = 30;
            this.textboxShuffle.Text = "Shuffle key";
            this.textboxShuffle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip2.SetToolTip(this.textboxShuffle, "Cheie bazata pe care modificarea pixelilor va fi distribuita");
            this.textboxShuffle.Enter += new System.EventHandler(this.textboxShuffle_Enter);
            this.textboxShuffle.Leave += new System.EventHandler(this.textboxShuffle_Leave);
            // 
            // labeSaveOutput
            // 
            this.labeSaveOutput.AutoSize = true;
            this.labeSaveOutput.Location = new System.Drawing.Point(6, 70);
            this.labeSaveOutput.Name = "labeSaveOutput";
            this.labeSaveOutput.Size = new System.Drawing.Size(96, 16);
            this.labeSaveOutput.TabIndex = 33;
            this.labeSaveOutput.Text = "Save output as";
            // 
            // cmbOutputFormat
            // 
            this.cmbOutputFormat.FormattingEnabled = true;
            this.cmbOutputFormat.Items.AddRange(new object[] {
            "BMP",
            "PNG"});
            this.cmbOutputFormat.Location = new System.Drawing.Point(114, 65);
            this.cmbOutputFormat.Name = "cmbOutputFormat";
            this.cmbOutputFormat.Size = new System.Drawing.Size(121, 24);
            this.cmbOutputFormat.TabIndex = 34;
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Image",
            "Audio"});
            this.cmbType.Location = new System.Drawing.Point(225, 30);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(108, 24);
            this.cmbType.TabIndex = 35;
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(240, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 16);
            this.label2.TabIndex = 36;
            this.label2.Text = "Choose type";
            // 
            // groupPaths
            // 
            this.groupPaths.Controls.Add(this.label1);
            this.groupPaths.Controls.Add(this.labeSaveOutput);
            this.groupPaths.Controls.Add(this.textboxShuffle);
            this.groupPaths.Controls.Add(this.cmbOutputFormat);
            this.groupPaths.Location = new System.Drawing.Point(12, 86);
            this.groupPaths.Name = "groupPaths";
            this.groupPaths.Size = new System.Drawing.Size(692, 96);
            this.groupPaths.TabIndex = 0;
            this.groupPaths.TabStop = false;
            this.groupPaths.Text = "File / WAV Path";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 200;
            this.toolTip1.ShowAlways = true;
            // 
            // toolTip3
            // 
            this.toolTip3.AutoPopDelay = 5000;
            this.toolTip3.InitialDelay = 500;
            this.toolTip3.ReshowDelay = 200;
            this.toolTip3.ShowAlways = true;
            // 
            // toolTip4
            // 
            this.toolTip4.AutoPopDelay = 5000;
            this.toolTip4.InitialDelay = 500;
            this.toolTip4.ReshowDelay = 200;
            this.toolTip4.ShowAlways = true;
            // 
            // toolTip5
            // 
            this.toolTip5.AutoPopDelay = 5000;
            this.toolTip5.InitialDelay = 500;
            this.toolTip5.ReshowDelay = 200;
            this.toolTip5.ShowAlways = true;
            // 
            // toolTip6
            // 
            this.toolTip6.AutoPopDelay = 5000;
            this.toolTip6.InitialDelay = 500;
            this.toolTip6.ReshowDelay = 200;
            this.toolTip6.ShowAlways = true;
            // 
            // toolTip7
            // 
            this.toolTip7.AutoPopDelay = 5000;
            this.toolTip7.InitialDelay = 500;
            this.toolTip7.ReshowDelay = 200;
            this.toolTip7.ShowAlways = true;
            // 
            // toolTip8
            // 
            this.toolTip8.AutoPopDelay = 5000;
            this.toolTip8.InitialDelay = 500;
            this.toolTip8.ReshowDelay = 200;
            this.toolTip8.ShowAlways = true;
            // 
            // AplicatieSteganografie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1450, 819);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.progressBarCompare);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblCapacityInfo);
            this.Controls.Add(this.grpOperationMode);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtActivity);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnRevealMessage);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.btnHideMessage);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.pictureBoxOriginal);
            this.Controls.Add(this.pictureBoxModified);
            this.Controls.Add(this.btnCompareImages);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.groupPaths);
            this.Name = "AplicatieSteganografie";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aplicatie Steganografie";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModified)).EndInit();
            this.grpOperationMode.ResumeLayout(false);
            this.grpOperationMode.PerformLayout();
            this.groupPaths.ResumeLayout(false);
            this.groupPaths.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Button btnBrowseInput;
        //private System.Windows.Forms.Label label2;
        //private System.Windows.Forms.TextBox txtOutputPath;
        //private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnHideMessage;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnRevealMessage;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox txtActivity;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.GroupBox grpOperationMode;
        private System.Windows.Forms.RadioButton rdbMessage;
        private System.Windows.Forms.RadioButton rdbFile;
        private System.Windows.Forms.Label lblCapacityInfo;
        private System.Windows.Forms.Button btnReset;
        private Label label4;
        private ProgressBar progressBarCompare;
        private TextBox textboxShuffle;
        private Label labeSaveOutput;
        private ComboBox cmbOutputFormat;
        private ComboBox cmbType;
        private Label label2;
        private GroupBox groupPaths;
        private ToolTip toolTip1;
        private ToolTip toolTip2;
        private ToolTip toolTip3;
        private ToolTip toolTip4;
        private ToolTip toolTip5;
        private ToolTip toolTip6;
        private ToolTip toolTip7;
        private ToolTip toolTip8;
    }
}

