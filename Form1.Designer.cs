namespace SteganoGraphy
{
    partial class Form1
    {
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbpHide = new System.Windows.Forms.TabPage();
            this.btnDecode = new System.Windows.Forms.Button();
            this.btnHide = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtKeyText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtImageFile = new System.Windows.Forms.TextBox();
            this.btnImageFile = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.picImage = new System.Windows.Forms.PictureBox();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.menuStrip1.SuspendLayout();
            this.tbpHide.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(492, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideToolStripMenuItem,
            this.extractToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.hideToolStripMenuItem.Text = "Hide";
            this.hideToolStripMenuItem.Click += new System.EventHandler(this.hideToolStripMenuItem_Click);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.extractToolStripMenuItem.Text = "Extract";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.extractToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // tbpHide
            // 
            this.tbpHide.Controls.Add(this.btnDecode);
            this.tbpHide.Controls.Add(this.btnHide);
            this.tbpHide.Controls.Add(this.groupBox5);
            this.tbpHide.Controls.Add(this.groupBox3);
            this.tbpHide.Location = new System.Drawing.Point(4, 22);
            this.tbpHide.Name = "tbpHide";
            this.tbpHide.Padding = new System.Windows.Forms.Padding(3);
            this.tbpHide.Size = new System.Drawing.Size(484, 547);
            this.tbpHide.TabIndex = 0;
            this.tbpHide.Text = "Image DCT";
            this.tbpHide.UseVisualStyleBackColor = true;
            // 
            // btnDecode
            // 
            this.btnDecode.Location = new System.Drawing.Point(237, 496);
            this.btnDecode.Name = "btnDecode";
            this.btnDecode.Size = new System.Drawing.Size(79, 32);
            this.btnDecode.TabIndex = 20;
            this.btnDecode.Text = "Decode";
            this.btnDecode.UseVisualStyleBackColor = true;
            this.btnDecode.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnHide
            // 
            this.btnHide.Enabled = false;
            this.btnHide.Location = new System.Drawing.Point(139, 496);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(78, 30);
            this.btnHide.TabIndex = 2;
            this.btnHide.Text = "Encode";
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.textBox1);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.txtKeyText);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.txtImageFile);
            this.groupBox5.Controls.Add(this.btnImageFile);
            this.groupBox5.Location = new System.Drawing.Point(24, 378);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(440, 112);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "File info";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Message";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(92, 76);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(241, 20);
            this.textBox1.TabIndex = 18;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Password";
            // 
            // txtKeyText
            // 
            this.txtKeyText.Location = new System.Drawing.Point(92, 50);
            this.txtKeyText.Name = "txtKeyText";
            this.txtKeyText.Size = new System.Drawing.Size(241, 20);
            this.txtKeyText.TabIndex = 4;
            this.txtKeyText.TabStop = false;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(28, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "File";
            // 
            // txtImageFile
            // 
            this.txtImageFile.Location = new System.Drawing.Point(92, 23);
            this.txtImageFile.Name = "txtImageFile";
            this.txtImageFile.Size = new System.Drawing.Size(241, 20);
            this.txtImageFile.TabIndex = 0;
            this.txtImageFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtImageFile_KeyDown);
            // 
            // btnImageFile
            // 
            this.btnImageFile.Location = new System.Drawing.Point(342, 23);
            this.btnImageFile.Name = "btnImageFile";
            this.btnImageFile.Size = new System.Drawing.Size(66, 47);
            this.btnImageFile.TabIndex = 1;
            this.btnImageFile.Text = "Browse...";
            this.btnImageFile.Click += new System.EventHandler(this.btnImageFile_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.picImage);
            this.groupBox3.Location = new System.Drawing.Point(64, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(355, 361);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Image";
            // 
            // picImage
            // 
            this.picImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picImage.Location = new System.Drawing.Point(15, 19);
            this.picImage.Name = "picImage";
            this.picImage.Size = new System.Drawing.Size(325, 327);
            this.picImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picImage.TabIndex = 0;
            this.picImage.TabStop = false;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tbpHide);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 24);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(492, 573);
            this.tabControl2.TabIndex = 3;
            this.tabControl2.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl2_Selecting);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(492, 597);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Multimedia - SteganoGraphy";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tbpHide.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picImage)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region atribute
        private static bool hiex = true;
        private string fileName = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        #endregion

        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TabPage tbpHide;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtKeyText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtImageFile;
        private System.Windows.Forms.Button btnImageFile;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.PictureBox picImage;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.Button btnDecode;
    }
}

