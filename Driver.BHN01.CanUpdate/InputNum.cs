using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Driver.BHN01.CanUpdate
{
    public partial class InputNum : Form
    {
       public string slaveNo = "1";

        public InputNum()
        {
            InitializeComponent();
            textBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.Parse(textBox1.Text) >= 1)
                {
                    slaveNo = textBox1.Text;
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch
            {
                slaveNo = "1";
            }

        }
    }
}
