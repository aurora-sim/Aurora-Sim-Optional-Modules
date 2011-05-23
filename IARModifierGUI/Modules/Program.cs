using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;
using Microsoft.Win32;

using OpenSim.Framework;
using OpenSim.Services.Interfaces;

using log4net;
using Nini.Config;
using OpenMetaverse;

namespace IARModifierGUI
{
    public class IARModifierGUI
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main (string[] cmd)
        {
            //Register the extention
            string ext = ".iar";
            RegistryKey key = Registry.ClassesRoot.CreateSubKey (ext);
            key.SetValue ("", "iar file");
            key.Close ();

            key = Registry.ClassesRoot.CreateSubKey (ext + "\\Shell\\Open\\command");

            key.SetValue ("", "\"" + Application.ExecutablePath + "\" \"%L\"");
            key.Close ();

            Application.EnableVisualStyles ();
            Application.SetCompatibleTextRenderingDefault (false);
            if (cmd.Length == 0)
                Application.Run (new IAREditor ());
            else
                Application.Run (new IAREditor (cmd[0]));
        }
    }
}