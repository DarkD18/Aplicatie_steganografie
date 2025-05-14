using System.Drawing;
using System.Windows.Forms;

namespace interfata
{
    partial class Form1
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
            //this.btnHideFile = new System.Windows.Forms.Button();
            //this.btnExtractFile = new System.Windows.Forms.Button();
            this.btnConvertToBmp = new System.Windows.Forms.Button();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.grpOperationMode = new System.Windows.Forms.GroupBox();
            this.rdbMessage = new System.Windows.Forms.RadioButton();
            this.rdbFile = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.lblCapacityInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModified)).BeginInit();
            this.grpOperationMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(171, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input image";
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(32, 124);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(443, 22);
            this.txtInputPath.TabIndex = 1;
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(493, 123);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseInput.TabIndex = 2;
            this.btnBrowseInput.Text = "browse";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 245);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Message";
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(135, 242);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(443, 22);
            this.txtMessage.TabIndex = 7;
            // 
            // btnHideMessage
            // 
            this.btnHideMessage.Location = new System.Drawing.Point(596, 238);
            this.btnHideMessage.Name = "btnHideMessage";
            this.btnHideMessage.Size = new System.Drawing.Size(129, 30);
            this.btnHideMessage.TabIndex = 8;
            this.btnHideMessage.Text = "hide message";
            this.btnHideMessage.UseVisualStyleBackColor = true;
            this.btnHideMessage.Click += new System.EventHandler(this.btn_hide);
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(135, 280);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(443, 22);
            this.txtOutput.TabIndex = 9;
            // 
            // btnRevealMessage
            // 
            this.btnRevealMessage.Location = new System.Drawing.Point(596, 277);
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
            this.label5.Location = new System.Drawing.Point(9, 283);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 16);
            this.label5.TabIndex = 12;
            this.label5.Text = "Hidden message";
            // 
            // txtActivity
            // 
            this.txtActivity.Location = new System.Drawing.Point(649, 41);
            this.txtActivity.Name = "txtActivity";
            this.txtActivity.Size = new System.Drawing.Size(268, 164);
            this.txtActivity.TabIndex = 14;
            this.txtActivity.Text = "";
            this.txtActivity.TextChanged += new System.EventHandler(this.txtActivity_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(744, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 16);
            this.label6.TabIndex = 15;
            this.label6.Text = "Activity log";
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(69)))), ((int)(((byte)(97)))));
            this.btnExit.Location = new System.Drawing.Point(830, 616);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(155, 37);
            this.btnExit.TabIndex = 16;
            this.btnExit.Text = "EXIT";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pictureBoxOriginal
            // 
            this.pictureBoxOriginal.Location = new System.Drawing.Point(12, 320);
            this.pictureBoxOriginal.Name = "pictureBoxOriginal";
            this.pictureBoxOriginal.Size = new System.Drawing.Size(395, 344);
            this.pictureBoxOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxOriginal.TabIndex = 17;
            this.pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxModified
            // 
            this.pictureBoxModified.Location = new System.Drawing.Point(420, 320);
            this.pictureBoxModified.Name = "pictureBoxModified";
            this.pictureBoxModified.Size = new System.Drawing.Size(395, 344);
            this.pictureBoxModified.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxModified.TabIndex = 18;
            this.pictureBoxModified.TabStop = false;
            // 
            // btnCompareImages
            // 
            this.btnCompareImages.Location = new System.Drawing.Point(825, 391);
            this.btnCompareImages.Name = "btnCompareImages";
            this.btnCompareImages.Size = new System.Drawing.Size(155, 30);
            this.btnCompareImages.TabIndex = 19;
            this.btnCompareImages.Text = "Compare images";
            this.btnCompareImages.Click += new System.EventHandler(this.btnCompareImages_Click);
            // 
            // btnHideFile
            // 
            //this.btnHideFile.Location = new System.Drawing.Point(747, 238);
            //this.btnHideFile.Name = "btnHideFile";
            //this.btnHideFile.Size = new System.Drawing.Size(150, 30);
            //this.btnHideFile.TabIndex = 20;
            //this.btnHideFile.Text = "Ascunde fișier";
            //this.btnHideFile.Click += new System.EventHandler(this.btnHideFile_Click);
            // 
            // btnExtractFile
            // 
            //this.btnExtractFile.Location = new System.Drawing.Point(747, 274);
            //this.btnExtractFile.Name = "btnExtractFile";
            //this.btnExtractFile.Size = new System.Drawing.Size(150, 30);
            //this.btnExtractFile.TabIndex = 21;
            //this.btnExtractFile.Text = "Extrage fișier";
            //this.btnExtractFile.Click += new System.EventHandler(this.btnExtractFile_Click);
            // 
            // btnConvertToBmp
            // 
            this.btnConvertToBmp.Location = new System.Drawing.Point(830, 473);
            this.btnConvertToBmp.Name = "btnConvertToBmp";
            this.btnConvertToBmp.Size = new System.Drawing.Size(150, 30);
            this.btnConvertToBmp.TabIndex = 22;
            this.btnConvertToBmp.Text = "Convert to BMP";
            this.btnConvertToBmp.UseVisualStyleBackColor = true;
            this.btnConvertToBmp.Click += new System.EventHandler(this.btnConvertToBmp_Click);
            // 
            // cmbMethod
            // 
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Items.AddRange(new object[] {
            "Standard LSB (Blue Channel)",
            "Multi-Channel LSB (R+G+B)"});
            this.cmbMethod.Location = new System.Drawing.Point(200, 57);
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
            this.grpOperationMode.Size = new System.Drawing.Size(150, 80);
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
            this.rdbMessage.Size = new System.Drawing.Size(117, 20);
            this.rdbMessage.TabIndex = 0;
            this.rdbMessage.TabStop = true;
            this.rdbMessage.Text = "Hide Message";
            this.rdbMessage.UseVisualStyleBackColor = true;
            this.rdbMessage.CheckedChanged += new System.EventHandler(this.rdbMessage_CheckedChanged);
            // 
            // rdbFile
            // 
            this.rdbFile.AutoSize = true;
            this.rdbFile.Location = new System.Drawing.Point(6, 51);
            this.rdbFile.Name = "rdbFile";
            this.rdbFile.Size = new System.Drawing.Size(82, 20);
            this.rdbFile.TabIndex = 1;
            this.rdbFile.Text = "Hide File";
            this.rdbFile.UseVisualStyleBackColor = true;
            this.rdbFile.CheckedChanged += new System.EventHandler(this.rdbFile_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(270, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 16);
            this.label4.TabIndex = 25;
            this.label4.Text = "Choose tehnique";
            // 
            // lblCapacityInfo
            // 
            this.lblCapacityInfo.AutoSize = true;
            this.lblCapacityInfo.Location = new System.Drawing.Point(70, 170); // Below input path
            this.lblCapacityInfo.Name = "lblCapacityInfo";
            this.lblCapacityInfo.Size = new System.Drawing.Size(0, 16);
            this.lblCapacityInfo.TabIndex = 25;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(992, 676);
            this.Controls.Add(this.label4);
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
            //this.Controls.Add(this.btnBrowseOutput);
            //this.Controls.Add(this.txtOutputPath);
            //this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxOriginal);
            this.Controls.Add(this.pictureBoxModified);
            this.Controls.Add(this.btnCompareImages);
            //this.Controls.Add(this.btnHideFile);
            //this.Controls.Add(this.btnExtractFile);
            this.Controls.Add(this.btnConvertToBmp);
            this.Controls.Add(this.cmbMethod);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxModified)).EndInit();
            this.grpOperationMode.ResumeLayout(false);
            this.grpOperationMode.PerformLayout();
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
        private System.Windows.Forms.Button btnConvertToBmp;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.GroupBox grpOperationMode;
        private System.Windows.Forms.RadioButton rdbMessage;
        private System.Windows.Forms.RadioButton rdbFile;
        private System.Windows.Forms.Label lblCapacityInfo;
        private Label label4;
    }
}

