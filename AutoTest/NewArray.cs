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
    public partial class NewArray : Form
    {
        public string name;
        public int size=2;
        public double min = 0;
        public double max = 0;

        public NewArray()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                name = textArrayName.Text;
                size = int.Parse(textArraySize.Text);
                if (name.Trim() != "")
                {
                    if (size >= 2)
                    {
                        try
                        {
                            min = double.Parse(textMin.Text);
                            max = double.Parse(textMax.Text);

                            this.DialogResult = DialogResult.OK;
                        }
                        catch 
                        {
                            MessageBox.Show("请输入正确的判断范围");
                        }
                        
                    }
                    else 
                    {
                        MessageBox.Show("数组大小必须大于等于2");
                    }
                }
                else 
                {
                    MessageBox.Show("请输入数组名称");
                }
            }
            catch { }
        }

        private void btnCanCel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
