using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace _02379_SERTECFARMASL_IOSfera
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.Run(new Form1());
                Console.ReadLine();
   
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }

}
