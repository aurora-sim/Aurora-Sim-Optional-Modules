﻿namespace IARModifierGUI
{
    partial class AddAsset
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose ();
            }
            base.Dispose (disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent ()
        {
            this.textBox1 = new System.Windows.Forms.TextBox ();
            this.label1 = new System.Windows.Forms.Label ();
            this.button1 = new System.Windows.Forms.Button ();
            this.SuspendLayout ();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point (81, 11);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size (100, 20);
            this.textBox1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point (12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (63, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Asset UUID";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point (56, 37);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size (123, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Upload and add Asset";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler (this.button1_Click);
            // 
            // AddAsset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size (191, 65);
            this.Controls.Add (this.button1);
            this.Controls.Add (this.label1);
            this.Controls.Add (this.textBox1);
            this.Name = "AddAsset";
            this.Text = "AddAsset";
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}