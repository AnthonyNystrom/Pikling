namespace Plinking_Server
{
    partial class PlikingServerMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlikingServerMain));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblOcrTo = new System.Windows.Forms.TextBox();
            this.lblOcrFrom = new System.Windows.Forms.TextBox();
            this.butLoadImage = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.butClearLog = new System.Windows.Forms.Button();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.Translator = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radGoogle = new System.Windows.Forms.RadioButton();
            this.radTrans = new System.Windows.Forms.RadioButton();
            this.txtSrc = new System.Windows.Forms.RichTextBox();
            this.txtDest = new System.Windows.Forms.RichTextBox();
            this.butGetTextTranslated = new System.Windows.Forms.Button();
            this.txLantDest = new System.Windows.Forms.TextBox();
            this.txtLanSrc = new System.Windows.Forms.TextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.Translator.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.Translator);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(826, 576);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.pictureBox5);
            this.tabPage2.Controls.Add(this.pictureBox4);
            this.tabPage2.Controls.Add(this.pictureBox3);
            this.tabPage2.Controls.Add(this.pictureBox2);
            this.tabPage2.Controls.Add(this.butClearLog);
            this.tabPage2.Controls.Add(this.lstLog);
            this.tabPage2.Controls.Add(this.pictureBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(818, 550);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Last Process";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblOcrTo);
            this.groupBox2.Controls.Add(this.lblOcrFrom);
            this.groupBox2.Controls.Add(this.butLoadImage);
            this.groupBox2.Location = new System.Drawing.Point(10, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(93, 98);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Test";
            // 
            // lblOcrTo
            // 
            this.lblOcrTo.Location = new System.Drawing.Point(49, 68);
            this.lblOcrTo.Name = "lblOcrTo";
            this.lblOcrTo.Size = new System.Drawing.Size(33, 20);
            this.lblOcrTo.TabIndex = 16;
            this.lblOcrTo.Text = "IT";
            // 
            // lblOcrFrom
            // 
            this.lblOcrFrom.Location = new System.Drawing.Point(10, 68);
            this.lblOcrFrom.Name = "lblOcrFrom";
            this.lblOcrFrom.Size = new System.Drawing.Size(33, 20);
            this.lblOcrFrom.TabIndex = 15;
            this.lblOcrFrom.Text = "EN";
            // 
            // butLoadImage
            // 
            this.butLoadImage.Location = new System.Drawing.Point(10, 21);
            this.butLoadImage.Name = "butLoadImage";
            this.butLoadImage.Size = new System.Drawing.Size(72, 30);
            this.butLoadImage.TabIndex = 14;
            this.butLoadImage.Text = "Load Img...";
            this.butLoadImage.UseVisualStyleBackColor = true;
            this.butLoadImage.Click += new System.EventHandler(this.butLoadImage_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox5.Location = new System.Drawing.Point(565, 6);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(108, 100);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox5.TabIndex = 11;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Click += new System.EventHandler(this.pictureBox5_Click);
            // 
            // pictureBox4
            // 
            this.pictureBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox4.Location = new System.Drawing.Point(451, 6);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(108, 100);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 10;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Click += new System.EventHandler(this.pictureBox4_Click);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox3.Location = new System.Drawing.Point(337, 6);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(108, 100);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 9;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(223, 6);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(108, 100);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // butClearLog
            // 
            this.butClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butClearLog.Location = new System.Drawing.Point(715, 76);
            this.butClearLog.Name = "butClearLog";
            this.butClearLog.Size = new System.Drawing.Size(97, 30);
            this.butClearLog.TabIndex = 7;
            this.butClearLog.Text = "Clear Log";
            this.butClearLog.UseVisualStyleBackColor = true;
            this.butClearLog.Click += new System.EventHandler(this.butClearLog_Click);
            // 
            // lstLog
            // 
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 16;
            this.lstLog.Location = new System.Drawing.Point(0, 114);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(812, 436);
            this.lstLog.TabIndex = 6;
            this.lstLog.SelectedIndexChanged += new System.EventHandler(this.lstLog_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(109, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(108, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // Translator
            // 
            this.Translator.Controls.Add(this.groupBox1);
            this.Translator.Location = new System.Drawing.Point(4, 22);
            this.Translator.Name = "Translator";
            this.Translator.Size = new System.Drawing.Size(818, 550);
            this.Translator.TabIndex = 3;
            this.Translator.Text = "Translator";
            this.Translator.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radGoogle);
            this.groupBox1.Controls.Add(this.radTrans);
            this.groupBox1.Controls.Add(this.txtSrc);
            this.groupBox1.Controls.Add(this.txtDest);
            this.groupBox1.Controls.Add(this.butGetTextTranslated);
            this.groupBox1.Controls.Add(this.txLantDest);
            this.groupBox1.Controls.Add(this.txtLanSrc);
            this.groupBox1.Location = new System.Drawing.Point(13, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(694, 281);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Test Translator";
            // 
            // radGoogle
            // 
            this.radGoogle.AutoSize = true;
            this.radGoogle.Location = new System.Drawing.Point(140, 10);
            this.radGoogle.Name = "radGoogle";
            this.radGoogle.Size = new System.Drawing.Size(59, 17);
            this.radGoogle.TabIndex = 20;
            this.radGoogle.Text = "Google";
            this.radGoogle.UseVisualStyleBackColor = true;
            // 
            // radTrans
            // 
            this.radTrans.AutoSize = true;
            this.radTrans.Checked = true;
            this.radTrans.Location = new System.Drawing.Point(140, 33);
            this.radTrans.Name = "radTrans";
            this.radTrans.Size = new System.Drawing.Size(75, 17);
            this.radTrans.TabIndex = 19;
            this.radTrans.TabStop = true;
            this.radTrans.Text = "Translated";
            this.radTrans.UseVisualStyleBackColor = true;
            // 
            // txtSrc
            // 
            this.txtSrc.Location = new System.Drawing.Point(6, 56);
            this.txtSrc.Name = "txtSrc";
            this.txtSrc.Size = new System.Drawing.Size(330, 161);
            this.txtSrc.TabIndex = 18;
            this.txtSrc.Text = "";
            // 
            // txtDest
            // 
            this.txtDest.Location = new System.Drawing.Point(342, 56);
            this.txtDest.Name = "txtDest";
            this.txtDest.Size = new System.Drawing.Size(330, 161);
            this.txtDest.TabIndex = 17;
            this.txtDest.Text = "";
            // 
            // butGetTextTranslated
            // 
            this.butGetTextTranslated.Location = new System.Drawing.Point(599, 240);
            this.butGetTextTranslated.Name = "butGetTextTranslated";
            this.butGetTextTranslated.Size = new System.Drawing.Size(73, 26);
            this.butGetTextTranslated.TabIndex = 16;
            this.butGetTextTranslated.Text = "Translate";
            this.butGetTextTranslated.UseVisualStyleBackColor = true;
            this.butGetTextTranslated.Click += new System.EventHandler(this.butGetTextTranslated_Click_1);
            // 
            // txLantDest
            // 
            this.txLantDest.Location = new System.Drawing.Point(45, 20);
            this.txLantDest.Name = "txLantDest";
            this.txLantDest.Size = new System.Drawing.Size(33, 20);
            this.txLantDest.TabIndex = 12;
            this.txLantDest.Text = "en";
            // 
            // txtLanSrc
            // 
            this.txtLanSrc.Location = new System.Drawing.Point(6, 20);
            this.txtLanSrc.Name = "txtLanSrc";
            this.txtLanSrc.Size = new System.Drawing.Size(33, 20);
            this.txtLanSrc.TabIndex = 11;
            this.txtLanSrc.Text = "it";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "home-16x16.png");
            this.imageList1.Images.SetKeyName(1, "add-folder-16x16.png");
            this.imageList1.Images.SetKeyName(2, "mail-sent-16x16.png");
            this.imageList1.Images.SetKeyName(3, "mail-16x16.png");
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // PlikingServerMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 600);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PlikingServerMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pikling Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.Translator.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button butClearLog;
        private System.Windows.Forms.TabPage Translator;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox txtSrc;
        private System.Windows.Forms.RichTextBox txtDest;
        private System.Windows.Forms.Button butGetTextTranslated;
        private System.Windows.Forms.TextBox txLantDest;
        private System.Windows.Forms.TextBox txtLanSrc;
        private System.Windows.Forms.RadioButton radGoogle;
        private System.Windows.Forms.RadioButton radTrans;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox lblOcrTo;
        private System.Windows.Forms.TextBox lblOcrFrom;
        private System.Windows.Forms.Button butLoadImage;
    }
}

