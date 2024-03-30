﻿namespace YtDownloader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            btnDownload = new Button();
            textBoxOutput = new TextBox();
            btnClear = new Button();
            progressBar1 = new ProgressBar();
            txtTarget = new TextBox();
            label1 = new Label();
            label2 = new Label();
            chkIsPlaylist = new CheckBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(42, 47);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(556, 23);
            textBox1.TabIndex = 0;
            textBox1.Text = "https://www.youtube.com/watch?v=wsrvmNtWU4E&pp=ygUJbWV0YWxsaWNh";
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(42, 76);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(108, 23);
            btnDownload.TabIndex = 2;
            btnDownload.Text = "DOWNLOAD";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // textBoxOutput
            // 
            textBoxOutput.AllowDrop = true;
            textBoxOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxOutput.Location = new Point(42, 203);
            textBoxOutput.Multiline = true;
            textBoxOutput.Name = "textBoxOutput";
            textBoxOutput.ScrollBars = ScrollBars.Both;
            textBoxOutput.Size = new Size(556, 266);
            textBoxOutput.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Location = new Point(523, 76);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 3;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(42, 486);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(556, 23);
            progressBar1.TabIndex = 3;
            progressBar1.Visible = false;
            // 
            // txtTarget
            // 
            txtTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTarget.Location = new Point(42, 154);
            txtTarget.Name = "txtTarget";
            txtTarget.Size = new Size(556, 23);
            txtTarget.TabIndex = 1;
            txtTarget.Text = "C:\\tools\\mp3";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 136);
            label1.Name = "label1";
            label1.Size = new Size(92, 15);
            label1.TabIndex = 4;
            label1.Text = "Target directory:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(42, 29);
            label2.Name = "label2";
            label2.Size = new Size(184, 15);
            label2.TabIndex = 4;
            label2.Text = "Video/Playlist URL/ID/mixed URL:";
            // 
            // chkIsPlaylist
            // 
            chkIsPlaylist.AutoSize = true;
            chkIsPlaylist.Location = new Point(156, 79);
            chkIsPlaylist.Name = "chkIsPlaylist";
            chkIsPlaylist.Size = new Size(63, 19);
            chkIsPlaylist.TabIndex = 5;
            chkIsPlaylist.Text = "Playlist";
            chkIsPlaylist.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AcceptButton = btnDownload;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 521);
            Controls.Add(chkIsPlaylist);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Controls.Add(textBoxOutput);
            Controls.Add(btnClear);
            Controls.Add(btnDownload);
            Controls.Add(txtTarget);
            Controls.Add(textBox1);
            Name = "Form1";
            Text = "YouTubino MP3";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button btnDownload;
        private TextBox textBoxOutput;
        private Button btnClear;
        private ProgressBar progressBar1;
        private TextBox txtTarget;
        private Label label1;
        private Label label2;
        private CheckBox chkIsPlaylist;
    }
}
