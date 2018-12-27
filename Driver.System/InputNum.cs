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
    public partial class InputNum : Form
    {
        public double data;

        public InputNum()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + "\\SKins\\DiamondBlue.ssk";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                try
                {
                    data = double.Parse(textBox1.Text);
                    this.Close();
                }
                catch 
                {
                    MessageBox.Show("输入有误,请重新输入数值");
                }
                
            }
            catch
            {
                data = 0;
            }
            
        }


    }
}
