using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoTest
{
    public partial class InputSN : Form
    {
        public string msg;
        public string temp;

        public InputSN()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        public InputSN(string info,string name)
        {
            InitializeComponent();

            label1.Text = info;

            textBox1.Text = name + "_复件";

            temp=name;

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        public InputSN(string info)
        {
            InitializeComponent();

            label1.Text = info;


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
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
