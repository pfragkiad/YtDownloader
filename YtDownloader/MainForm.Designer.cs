namespace YtDownloader
{
    partial class MainForm
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
            label1 = new Label();
            label2 = new Label();
            chkIsPlaylist = new CheckBox();
            statusStrip1 = new StatusStrip();
            tstStatus = new ToolStripStatusLabel();
            cboTarget = new ComboBox();
            statusStrip1.SuspendLayout();
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
            textBoxOutput.Size = new Size(556, 242);
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
            progressBar1.Location = new Point(42, 465);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(556, 23);
            progressBar1.TabIndex = 3;
            progressBar1.Visible = false;
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
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { tstStatus });
            statusStrip1.Location = new Point(0, 499);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(630, 22);
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // tstStatus
            // 
            tstStatus.Name = "tstStatus";
            tstStatus.Size = new Size(23, 17);
            tstStatus.Text = "OK";
            // 
            // cboTarget
            // 
            cboTarget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboTarget.FormattingEnabled = true;
            cboTarget.Location = new Point(42, 165);
            cboTarget.Name = "cboTarget";
            cboTarget.Size = new Size(556, 23);
            cboTarget.TabIndex = 7;
            // 
            // MainForm
            // 
            AcceptButton = btnDownload;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 521);
            Controls.Add(cboTarget);
            Controls.Add(statusStrip1);
            Controls.Add(chkIsPlaylist);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Controls.Add(textBoxOutput);
            Controls.Add(btnClear);
            Controls.Add(btnDownload);
            Controls.Add(textBox1);
            Name = "MainForm";
            Text = "YouTubino MP3";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button btnDownload;
        private TextBox textBoxOutput;
        private Button btnClear;
        private ProgressBar progressBar1;
        private Label label1;
        private Label label2;
        private CheckBox chkIsPlaylist;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tstStatus;
        private ComboBox cboTarget;
    }
}
