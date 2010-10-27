using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Proxy
{
    static class Program
    {
        public static MainForm mForm;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mForm = new MainForm();
            Application.Run(mForm);
        }
    }
}
