namespace AntsGame
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
            this.DrawedMap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.DrawedMap)).BeginInit();
            this.SuspendLayout();
            // 
            // DrawedMap
            // 
            this.DrawedMap.BackColor = System.Drawing.SystemColors.Window;
            this.DrawedMap.Location = new System.Drawing.Point(5, 5);
            this.DrawedMap.Name = "DrawedMap";
            this.DrawedMap.Size = new System.Drawing.Size(862, 462);
            this.DrawedMap.TabIndex = 0;
            this.DrawedMap.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 479);
            this.Controls.Add(this.DrawedMap);
            this.Name = "Form1";
            this.Text = "Ant Battle";
            ((System.ComponentModel.ISupportInitialize)(this.DrawedMap)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox DrawedMap;
    }
}

