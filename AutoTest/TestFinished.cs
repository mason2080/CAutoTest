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
    public partial class TestFinished : Form
    {
        public string msg;
        public string temp;

        public TestFinished()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        public TestFinished(string info, string name)
        {
            InitializeComponent();

            label1.Text = info;

            textBox1.Text = name + "_复件";

            temp=name;

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
    }
}
