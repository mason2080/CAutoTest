using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Driver.File.Sql;
using System.IO;
using Driver.File.Ini;

namespace AutoTest
{
    public partial class SelectDeviceConfig: Form
    {
        int currentSelectedIndex = 0;
        IniFileClass iniFileClass = new IniFileClass();

        public SelectDeviceConfig()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;

            ProjectGlobal.Title = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "公司名称");

            label3.Text = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "当前设备配置");

            label4.Text = ProjectGlobal.Title;
        }

        private void selectTestItem_Load(object sender, EventArgs e)
        {
            SQLiteClass sqlite = new SQLiteClass();
            try
            {
                DataTable dt1 = sqlite.ReadDeviceSetting(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    listView1.Items.Add(dt1.Rows[i].ItemArray[0].ToString().Replace("_DeviceSetting",""));
                }
            }
            catch { }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
           textName.Text= listView1.SelectedItems[0].Text;

           //this.Hide();
           //StepEdit form = new StepEdit(listView1.SelectedItems[0].Text+"_TestItem");
           //form.ShowDialog();
           //this.Close();

           this.Hide();
           DeviceEdit form = new DeviceEdit(textName.Text+"_DeviceSetting");
           form.ShowDialog();
           this.Show();
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            if (textName.Text == "")
            {
                MessageBox.Show("请输入测试项目名称");
            }
            else
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[0].Text == textName.Text)
                    {
                        MessageBox.Show("已经存在此项目名称，请重新输入");
                        return;
                    }
                }
            }

                char[] charArray = textName.Text.ToCharArray();

                if ((charArray[0] < 48) || (charArray[0] > 57))
                {
                    try
                    {
                        SQLiteClass sqlite = new SQLiteClass();
                        sqlite.CreateNewDeviceTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", textName.Text + "_DeviceSetting");
                        this.Hide();
                        DeviceEdit form = new DeviceEdit(textName.Text + "_DeviceSetting");
                        form.ShowDialog();
                        this.Close();
                    }
                    catch { }
                }
                else 
                {
                    MessageBox.Show("项目名称不能以数字开头，请重输");
                }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("删除:" + listView1.Items[currentSelectedIndex].Text+"\r\n 引用到此步骤的测试项目需要手动删除", "删除后不可恢复！",MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                SQLiteClass sqlite = new SQLiteClass();
                sqlite.DeleteTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", listView1.Items[currentSelectedIndex].Text + "_DeviceSetting");

                try
                {
                    listView1.Items.Clear();

                    DataTable dt1 = sqlite.ReadDeviceSetting(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        listView1.Items.Add(dt1.Rows[i].ItemArray[0].ToString().Replace("_DeviceSetting", ""));
                    }
                }
                catch { }
            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count <= 0)//这这判断是否点了空白区，点了空白区进到if里   
            {
                MessageBox.Show("请选中items");
                return;
            }

            if ((listView1.SelectedItems[0].Index >= 0) && (listView1.SelectedItems[0].Index < listView1.Items.Count))
            {
                currentSelectedIndex = listView1.SelectedItems[0].Index;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            DeviceEdit form = new DeviceEdit(label3.Text + "_DeviceSetting");
            form.ShowDialog();
            this.Show();
        }

        private void 设为当前配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            iniFileClass.IniWriteValue(Application.StartupPath + "\\Config.ini", "配置", "当前设备配置", listView1.SelectedItems[0].Text);
            label3.Text = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "当前设备配置");
        }

        private void 导出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt2, dt1;
            SQLiteClass sqlite = new SQLiteClass();

            if (this.listView1.SelectedItems.Count <= 0)//这这判断是否点了空白区，点了空白区进到if里   
            {
                MessageBox.Show("请选中items");
                return;
            }
            dt1 = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", listView1.SelectedItems[0].Text + "_DeviceSetting");

            try
            {
                if (true)//(dataGridView1.SelectedRows.Count >= 0) && (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() != "") && (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != ""))
                {

                    FileStream fileStream;
                    StreamWriter streamWriter;

                    string tempTime = System.DateTime.Now.ToShortDateString().Replace("/", "");
                    saveFileDialog1.FileName = listView1.SelectedItems[0].Text + "_DeviceSetting";// dataGridView2.SelectedRows[0].Cells[0].Value.ToString() + "_" + dataGridView2.SelectedRows[0].Cells[1].Value.ToString();//"History_"+ tempTime.Replace(@"\", "");
                    saveFileDialog1.DefaultExt = ".dev";

                    saveFileDialog1.Filter = "All files (*.*)|*.*|Step files (*.dev)|*.dev";
                    saveFileDialog1.FilterIndex = 2;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog1.FileName;
                        fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                        streamWriter = new StreamWriter(fileStream, Encoding.Default);

                        string msg = "";
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            msg = dt1.Rows[i].ItemArray[0].ToString() + "^" + dt1.Rows[i].ItemArray[1].ToString() + "^" + dt1.Rows[i].ItemArray[2].ToString() + "^" + dt1.Rows[i].ItemArray[3].ToString() + "^" + dt1.Rows[i].ItemArray[4].ToString() + "\n";
                            streamWriter.WriteLine(msg);
                        }

                        streamWriter.Close();
                        fileStream.Close();

                        MessageBox.Show("导出成功");
                    }
                }
                else
                {
                    //  MessageBox.Show("请先选择需要导出的记录");
                }
            }
            catch { }
        }

        private void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SQLiteClass sqlite = new SQLiteClass();
                if (true)//(dataGridView1.SelectedRows.Count >= 0) && (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() != "") && (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != ""))
                {
                    openFileDialog1.Filter = "All files (*.*)|*.*|Step files (*.dev)|*.dev";
                    openFileDialog1.FilterIndex = 2;
                    string filename = "";
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        filename = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1);
                        filename = filename.Replace("_DeviceSetting.dev", "");
                        InputSN form = new InputSN("请输入测试步骤名称", filename);

                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                DataTable dt1 = sqlite.ReadDeviceSetting(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                                for (int i = 0; i < dt1.Rows.Count; i++)
                                {
                                    if (form.msg == dt1.Rows[i].ItemArray[0].ToString().Replace("_DeviceSetting", ""))
                                    {
                                        MessageBox.Show("已存在此测试步骤名称，请重新输入");
                                        return;
                                    }
                                }

                                char[] charArray = form.msg.ToCharArray();

                                if ((charArray[0] < 48) || (charArray[0] > 57))
                                {
                                    sqlite.CreateNewDeviceTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_DeviceSetting");
                                    DataTable dt2 = new DataTable();
                                    FileStream filest = new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.ReadWrite);
                                    StreamReader sr = new StreamReader(filest, Encoding.Default);
                                    string row = "";

                                    dt2.Columns.Add("Enable");
                                    dt2.Columns.Add("调用名");
                                    dt2.Columns.Add("Name");
                                    dt2.Columns.Add("配置");
                                    dt2.Columns.Add("路径");

                                    while ((row = sr.ReadLine()) != null)
                                    {
                                        if (row.Trim() != "")
                                        {
                                            DataRow dr = dt2.NewRow();
                                            string[] array = row.Split('^');

                                            dr["Enable"] = array[0];
                                            dr["调用名"] = array[1];
                                            dr["Name"] = array[2];
                                            dr["配置"] = array[3];
                                            dr["路径"] = array[4];
                                            dt2.Rows.Add(dr);
                                        }
                                    }
                                    filest.Close();
                                    sr.Close();

                                    if (sqlite.UpdateDeviceSettingTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_DeviceSetting", dt2) == 1)
                                    {
                                        MessageBox.Show("导入成功");
                                    }
                                }
                            }
                            catch { }
                        }

                    }
                }
                try
                {
                    listView1.Items.Clear();

                    DataTable dt1 = sqlite.ReadDeviceSetting(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        listView1.Items.Add(dt1.Rows[i].ItemArray[0].ToString().Replace("_DeviceSetting", ""));
                    }
                }
                catch { }
            }
            catch { }
        }

    }
}
