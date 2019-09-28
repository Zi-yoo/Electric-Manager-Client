using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Init log = new Init();

            log.ShowDialog();

            if (log.DialogResult == DialogResult.OK)
            {
                Application.Run(new Main());
            }
        }
    }
}
