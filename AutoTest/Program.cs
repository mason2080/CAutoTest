using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;



namespace AutoTest
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
            Login registerForm = new Login();

            //DllDebug registerForm = new DllDebug();

            registerForm.ShowDialog();
            if (registerForm.DialogResult == DialogResult.OK)
            {
                try
                {
                    Application.Run(new MainSelect());

                    //MainSelect form = new MainSelect();
                    //form.ShowDialog();
                    //if (form.DialogResult == DialogResult.OK)
                    //{
                    //    Application.Run(new Main());
                    //}
                }
                catch { }
            }
            else
            {
                return;
            }
        }
    }
}
