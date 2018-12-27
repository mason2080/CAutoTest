using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Driver.SystemFunction
{
    public partial class ShowPicture : Form
    {
        string msg, filepath;
        int timeout;

        public ShowPicture()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        public ShowPicture(string msgShow,int timeOut, string filePath)
        {
            msg = msgShow;
            timeout = timeOut;
            filepath = filePath;

            if (timeout == 0)
            {
                timeout = 1;
            }

            InitializeComponent();
            label1.Text = msgShow;
            pictureBox1.Load(filePath);
            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            timeout--;

            label2.Text = timeout.ToString();

            if (timeout == 0)
            {
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
