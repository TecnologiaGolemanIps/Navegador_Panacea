using System;
using System.Windows.Forms;

namespace Panacea
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Iniciamos la aplicación con Form1
            Application.Run(new Form1());
        }
    }
}