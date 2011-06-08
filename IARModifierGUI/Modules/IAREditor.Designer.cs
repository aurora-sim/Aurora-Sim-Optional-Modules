/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
            this.label2 = new System.Windows.Forms.Label ();
            this.button3 = new System.Windows.Forms.Button ();
            this.button5 = new System.Windows.Forms.Button ();
            this.panel2 = new System.Windows.Forms.Panel ();
            this.label1 = new System.Windows.Forms.Label ();
            this.textBox1 = new System.Windows.Forms.TextBox ();
            this.label3 = new System.Windows.Forms.Label ();
            this.textBox2 = new System.Windows.Forms.TextBox ();
            this.panel1.SuspendLayout ();
            this.panel2.SuspendLayout ();
            this.SuspendLayout ();
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point (102, 30);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size (342, 301);
            this.treeView1.TabIndex = 0;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler (this.treeView1_NodeMouseClick_1);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler (this.treeView1_NodeMouseDoubleClick);
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
            this.button1.Location = new System.Drawing.Point (7, 3);
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
            this.panel1.Controls.Add (this.button1);
            this.panel1.Location = new System.Drawing.Point (3, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size (93, 118);
            this.panel1.TabIndex = 3;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point (0, 90);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size (90, 23);
            this.button7.TabIndex = 6;
            this.button7.Text = "Cancel Move";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler (this.cancelMove_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point (0, 61);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size (90, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "Move Contents";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler (this.move_contents_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point (7, 32);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size (75, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "Move";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler (this.move_Click);
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
            // panel2
            // 
            this.panel2.Controls.Add (this.textBox2);
            this.panel2.Controls.Add (this.label3);
            this.panel2.Controls.Add (this.label1);
            this.panel2.Controls.Add (this.textBox1);
            this.panel2.Location = new System.Drawing.Point (450, 30);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size (166, 100);
            this.panel2.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point (60, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size (35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point (3, 20);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size (160, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler (this.textBox1_TextChanged);
            this.textBox1.Leave += new System.EventHandler (this.textBox1_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point (60, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size (31, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Type";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point (0, 59);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size (160, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.TextChanged += new System.EventHandler (this.textBox2_TextChanged);
            this.textBox2.Leave += new System.EventHandler (this.textBox2_Leave);
            // 
            // IAREditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size (628, 343);
            this.Controls.Add (this.panel2);
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
            this.panel2.ResumeLayout (false);
            this.panel2.PerformLayout ();
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
    }
}
