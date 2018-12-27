using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.StubHelpers;

namespace MonitorServer
{
    public partial class Service1 : ServiceBase
    {
    

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
           // MessageBox.Show("");
            timer1.Enabled = true;
        }

        protected override void OnStop()
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Help.ShowHelp(this,@"C:\Program Files\GPRSClientSetUp\BNET_GPRS_Client.exe");
            
           // MessageBox.Show("");
           // System.Diagnostics.Process.Start(@"C:\Program Files\GPRSClientSetUp\BNET_GPRS_Client.exe");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = @"C:\Program Files\GPRSClientSetUp\BNET_GPRS_Client.exe";
            info.Arguments = "";
            info.WindowStyle = ProcessWindowStyle.Minimized;
            Process pro = Process.Start(info);
            pro.WaitForExit();
            
            try
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if (process.ProcessName.ToUpper() == "GAMESERVER")
                    {
                        //找到
                    }
                    else
                    {

                        System.Diagnostics.Process.Start(@"C:\Program Files\GPRSClientSetUp\BNET_GPRS_Client.exe");
                        //没找到
                    }
                }
            }
            catch { }
        }
    }
}
