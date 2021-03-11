using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.GDI;
using System.Windows.Forms;

namespace GraphicsTester.GDI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GraphicsPlatform.RegisterGlobalService(new GDIGraphicsService());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
