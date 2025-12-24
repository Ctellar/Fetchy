namespace Fetchy
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblLog = new System.Windows.Forms.Label();
            this.picBox = new System.Windows.Forms.PictureBox();
            this.pProgress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).BeginInit();
            this.SuspendLayout();
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.Location = new System.Drawing.Point(79, 26);
            this.lblLog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(118, 14);
            this.lblLog.TabIndex = 1;
            this.lblLog.Text = "Checking for update...";
            // 
            // picBox
            // 
            this.picBox.Image = global::Fetchy.Properties.Resources.progress;
            this.picBox.Location = new System.Drawing.Point(14, 13);
            this.picBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.picBox.Name = "picBox";
            this.picBox.Size = new System.Drawing.Size(58, 54);
            this.picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picBox.TabIndex = 2;
            this.picBox.TabStop = false;
            // 
            // pProgress
            // 
            this.pProgress.Location = new System.Drawing.Point(82, 43);
            this.pProgress.MarqueeAnimationSpeed = 30;
            this.pProgress.Name = "pProgress";
            this.pProgress.Size = new System.Drawing.Size(309, 10);
            this.pProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pProgress.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 84);
            this.Controls.Add(this.pProgress);
            this.Controls.Add(this.picBox);
            this.Controls.Add(this.lblLog);
            this.Font = new System.Drawing.Font("Cambria", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "~";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.picBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.PictureBox picBox;
        private System.Windows.Forms.ProgressBar pProgress;
    }
}

