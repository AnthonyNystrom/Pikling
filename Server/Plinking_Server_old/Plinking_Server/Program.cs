using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Plinking_Server
{
    static class Program
    {
        static public PlikingServerMain MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm=new PlikingServerMain();
            Application.Run(MainForm);
        }
    }
}
