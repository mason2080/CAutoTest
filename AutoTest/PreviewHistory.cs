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
    public partial class PreviewHistory : Form
    {
        public PreviewHistory()
        {
            InitializeComponent();
        }

        public PreviewHistory(string title,DataTable dt1)
        {
            InitializeComponent();

            textBox1.Text = title;

            dataGridView1.DataSource = dt1;


        }
    }
}
