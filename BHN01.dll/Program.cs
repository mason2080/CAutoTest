using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace BHN01.Dll
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

            Application.Run(new BHN01Config());
            //Register registerForm = new Register();
            //registerForm.ShowDialog();
            //if (registerForm.DialogResult == DialogResult.OK)
            //{
            //    try
            //    {
            //        Application.Run(new Main());
            //    }
            //    catch { }
            //}
            //else
            //{
            //    return;
            //}
        }
    }
}
