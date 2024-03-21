namespace Secure
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
            this.userInput = new System.Windows.Forms.TextBox();
            this.commandHistory = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // userInput
            // 
            this.userInput.BackColor = System.Drawing.SystemColors.InfoText;
            this.userInput.ForeColor = System.Drawing.SystemColors.Window;
            this.userInput.Location = new System.Drawing.Point(1, 429);
            this.userInput.Name = "userInput";
            this.userInput.Size = new System.Drawing.Size(799, 20);
            this.userInput.TabIndex = 0;
            // 
            // commandHistory
            // 
            this.commandHistory.BackColor = System.Drawing.SystemColors.InfoText;
            this.commandHistory.ForeColor = System.Drawing.SystemColors.Window;
            this.commandHistory.Location = new System.Drawing.Point(1, -1);
            this.commandHistory.Name = "commandHistory";
            this.commandHistory.Size = new System.Drawing.Size(799, 424);
            this.commandHistory.TabIndex = 1;
            this.commandHistory.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.commandHistory);
            this.Controls.Add(this.userInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Secure";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox userInput;
        private System.Windows.Forms.RichTextBox commandHistory;
    }
}

