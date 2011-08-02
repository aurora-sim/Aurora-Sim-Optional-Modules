using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;    
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;

namespace IARModifierGUI
{
    public partial class AddAsset : Form
    {
        private IAREditor m_editor = null;
        public AddAsset (IAREditor editor)
        {
            m_editor = editor;
            InitializeComponent ();
            textBox1.Text = UUID.Random ().ToString ();
        }

        private void button1_Click (object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog ();
            o.AddExtension = true;
            DialogResult r = o.ShowDialog ();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                Stream fileStream = o.OpenFile ();
                try
                {
                    Bitmap b = (Bitmap)Bitmap.FromStream (fileStream);
                    byte[] data = OpenMetaverse.Imaging.OpenJPEG.EncodeFromImage (b, false);
                    m_editor.AddAsset (textBox1.Text, data);
                }
                catch
                {
                    byte[] readData = new byte[(int)fileStream.Length];
                    fileStream.Read(readData, 0, (int)fileStream.Length);
                    OpenMetaverse.Imaging.ManagedImage mi;
                    if(OpenMetaverse.Imaging.OpenJPEG.DecodeToImage(readData, out mi))
                        m_editor.AddAsset (textBox1.Text, readData);
                }
            }
        }
    }
}
