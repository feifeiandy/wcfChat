namespace NikeSoftChat
{
    partial class PleaseWaitDialog
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlConnecting = new System.Windows.Forms.Panel();
            this.pnlError = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.pnlConnecting.SuspendLayout();
            this.pnlError.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(30, 41);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(198, 20);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connecting to chat server... Please wait.";
            // 
            // pnlConnecting
            // 
            this.pnlConnecting.Controls.Add(this.label1);
            this.pnlConnecting.Controls.Add(this.progressBar1);
            this.pnlConnecting.Location = new System.Drawing.Point(0, 0);
            this.pnlConnecting.Name = "pnlConnecting";
            this.pnlConnecting.Size = new System.Drawing.Size(269, 94);
            this.pnlConnecting.TabIndex = 3;
            // 
            // pnlError
            // 
            this.pnlError.Controls.Add(this.pnlConnecting);
            this.pnlError.Controls.Add(this.btnOK);
            this.pnlError.Controls.Add(this.lblErrorMessage);
            this.pnlError.Location = new System.Drawing.Point(11, 11);
            this.pnlError.Name = "pnlError";
            this.pnlError.Size = new System.Drawing.Size(269, 94);
            this.pnlError.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(178, 58);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.AutoSize = true;
            this.lblErrorMessage.Location = new System.Drawing.Point(27, 14);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(150, 13);
            this.lblErrorMessage.TabIndex = 0;
            this.lblErrorMessage.Text = "Error: Cannot connect to chat!";
            // 
            // PleaseWaitDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 107);
            this.ControlBox = false;
            this.Controls.Add(this.pnlError);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PleaseWaitDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connecting...";
            this.pnlConnecting.ResumeLayout(false);
            this.pnlConnecting.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.pnlError.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlConnecting;
        private System.Windows.Forms.Panel pnlError;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblErrorMessage;
    }
}