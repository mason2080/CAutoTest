using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MonitorService1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //textBox1.Text = "";
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                //textBox1.Text += process.ProcessName.ToUpper() + "\r\n";

                if (process.ProcessName.ToUpper() == "BNETGPRSSERVER")
                {
                    //找到

                    return;
                }
                else
                {


                }
            }
            System.Diagnostics.Process.Start(@"D:\资料\Server Client - RemoteUpdate-44\GameServer\bin\Release\BNETGPRSServer.exe");
            //没找到
        }
    }
}
