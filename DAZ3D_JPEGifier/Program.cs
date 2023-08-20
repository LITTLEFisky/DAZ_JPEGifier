using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ3D_JPEGifier
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        public static int quality = 90;
        public static int libIndex = 0;
        public static Form2 selector;
        public static Form3 info;
        public static string filepath = "";
        public static int textureSize = 0;
        public static bool resize = false;
        public static bool ready = false;
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
