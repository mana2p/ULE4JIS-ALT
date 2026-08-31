using System;
using System.Windows.Forms;

namespace Ule4Jis.Net
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
    }
}