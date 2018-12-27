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
    public partial class LoopTest : Form
    {
        public int testTimes;

        public LoopTest(int times)
        {
            InitializeComponent();

            testTimes = times;
            textBox1.Text = testTimes.ToString();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                testTimes = int.Parse(textBox1.Text);
            }
            catch
            {
                testTimes = 0;
            }
            this.Close();
        }
    }
}
