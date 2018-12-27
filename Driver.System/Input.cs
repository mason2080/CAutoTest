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
    public partial class Input : Form
    {
        public string msg;

        public Input()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                msg = textBox1.Text;
            }
            catch
            {
                msg = "";
            }
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
