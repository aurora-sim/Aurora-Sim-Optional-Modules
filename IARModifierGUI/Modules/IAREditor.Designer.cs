namespace IARModifierGUI
{
    partial class IAREditor
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
            this.treeView1 = new System.Windows.Forms.TreeView ();
            this.Title = new System.Windows.Forms.Label ();
            this.button1 = new System.Windows.Forms.Button ();
            this.panel1 = new System.Windows.Forms.Panel ();
            this.button7 = new System.Windows.Forms.Button ();
            this.button6 = new System.Windows.Forms.Button ();
            this.button4 = new System.Windows.Forms.Button ();
            this.button2 = new System.Windows.Forms.Button ();
            this.label2 = new System.Windows.Forms.Label ();
            this.button3 = new System.Windows.Forms.Button ();
            this.button5 = new System.Windows.Forms.Button ();
            this.panel1.SuspendLayout ();
            this.SuspendLayout ();
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point (102, 30);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size (342, 301);
            this.treeView1.TabIndex = 0;
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Location = new System.Drawing.Point (230, 9);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size (85, 13);
            this.Title.TabIndex = 1;
            this.Title.Text = "Inventory Nodes";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point (7, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size (75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Delete";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler (this.delete_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add (this.button7);
            this.panel1.Controls.Add (this.button6);
            this.panel1.Controls.Add (this.button4);
            this.panel1.Controls.Add (this.button2);
            this.panel1.Controls.Add (this.button1);
            this.panel1.Location = new System.Drawing.Point (3, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size (93, 152);
            this.panel1.TabIndex = 3;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point (0, 119);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size (90, 23);
            this.button7.TabIndex = 6;
            this.button7.Text = "Cancel Move";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler (this.cancelMove_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point (0, 90);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size (90, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "Move Contents";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler (this.move_contents_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point (7, 61);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size (75, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "Move";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler (this.move_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point (7, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size (75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Rename";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler (this.rename_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point (16, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size (59, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Commands";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point (10, 308);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size (75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Save";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler (this.save_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point (10, 279);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size (75, 23);
            this.button5.TabIndex = 6;
            this.button5.Text = "Merge";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler (this.merge_Click);
            // 
            // IAREditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size (456, 343);
            this.Controls.Add (this.button5);
            this.Controls.Add (this.button3);
            this.Controls.Add (this.label2);
            this.Controls.Add (this.panel1);
            this.Controls.Add (this.Title);
            this.Controls.Add (this.treeView1);
            this.Name = "IAREditor";
            this.Text = "IAREditor";
            this.Load += new System.EventHandler (this.IAREditor_Load);
            this.panel1.ResumeLayout (false);
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
    }
}