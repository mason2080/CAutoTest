using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Security.Cryptography;
using Driver.File.Sql;
using Driver.File.Ini;

namespace AutoTest
{
    public partial class ProjectEdit : Form
    {
        static string encryptKey = "BNET";    //定义密钥  
        string TestProjectName;

        DataTable dt=new DataTable();

        IniFileClass iniFileClass = new IniFileClass();


        public ProjectEdit()
        {
            InitializeComponent();
            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;

            ProjectGlobal.Title = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "公司名称");

            label4.Text = ProjectGlobal.Title;

        }

        public ProjectEdit(string name)
        {
            TestProjectName = name;
            InitializeComponent();

            label4.Text = name.Replace("_TestProject", "");

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;

            //ProjectGlobal.Title = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "公司名称");

            //label4.Text = ProjectGlobal.Title;


        }

        private string  GetDiskId()
        {
            string a= "0A1B2C3D4E";
            string HDid = "";

            //获取硬盘ID   
            ManagementClass mc = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                // HDid = (string)mo.Properties["Model"].Value;
                HDid = (string)mo.Properties["SerialNumber"].Value;

                if (HDid != null)
                {
                    break;
                }
            }
            moc = null;
            mc = null;

            if (HDid != null)
            {
                HDid.Trim();
                char[] arr = HDid.ToCharArray();
                Array.Reverse(arr);

                HDid = new string(arr);

                if (HDid.Length >= 10)
                {
                    HDid = HDid.Substring(0, 10);
                }
                else
                {
                    HDid = HDid.Substring(0, HDid.Length) + a.Substring(0, 10 - HDid.Length);
                }
            }
            else 
            {
                HDid = a;
            }

            return HDid;     
        }

        private string  GenRandomString()
        {
            Random rd = new Random();
            string randomString = "";
            for (byte i = 0; i < 20; i++)
            {
                randomString += rd.Next(10);
            }
            return randomString;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = GetDiskId() + GenRandomString();

            string strDir = Directory.GetCurrentDirectory() + @"\temp.key";
            FileStream fs = new FileStream(strDir, FileMode.Create, FileAccess.ReadWrite);

            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(textBox1.Text);
            sw.Close();

            fs.Close();

            try
            {
                strDir = Directory.GetCurrentDirectory() + @"\License.key";
                File.Delete(strDir);
            }
            catch { }
            //string en=Encrypt(textBox1.Text);
            //MessageBox.Show(en);

            //string dn = Decrypt(en);
            //MessageBox.Show(dn);
        }

        private void Register_Load(object sender, EventArgs e)
        {
            textBox3.Focus();
            comboBox1.SelectedIndex = 0;


            SQLiteClass sqlite = new SQLiteClass();
            try
            {
                dt = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", TestProjectName);
                dataGridView1.DataSource = dt;
                dataGridView1.ColumnHeadersVisible = true;
                dataGridView1.RowHeadersVisible = false;
                DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
                columnHeaderStyle.BackColor = Color.Beige;
                columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
                dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
                dataGridView1.Columns[0].ReadOnly = true;

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
            catch
            {
            }

            try
            {

                DataTable dt1 = sqlite.ReadTestItem(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    listView1.Items.Add(dt1.Rows[i].ItemArray[0].ToString().Replace("_TestItem", ""));
                }
            }
            catch { }


            //string strDir = Directory.GetCurrentDirectory() + @"\License.key";
            //if (File.Exists(strDir))//Create HistoryData Directory
            //{
            //    FileStream fs = new FileStream(strDir, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //    StreamReader sr = new StreamReader(fs);
            //    string key = sr.ReadLine();
            //    string license = sr.ReadLine();
            //    if ((key != null) && (license != null) && (key != ""))
            //    {
            //        if ((key == Decrypt(license))&&(GetDiskId() == key.Substring(0, 10)))
            //        {
            //            this.DialogResult = DialogResult.OK;
            //            this.Close();
            //        }
            //        else 
            //        {
            //            textBox1.Text = "注册文件破坏,请重新注册";
            //        }
            //    }
            //    fs.Close();
            //}
            //else 
            //{
            //    strDir = Directory.GetCurrentDirectory() + @"\temp.key";
                
            //    if (!File.Exists(strDir))//Create HistoryData Directory
            //    {
            //        FileStream fs = new FileStream(strDir, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //        textBox1.Text = GetDiskId() + GenRandomString();
            //        StreamWriter sw = new StreamWriter(fs);
            //        sw.WriteLine(textBox1.Text);
            //        sw.Close();
            //        fs.Close();
            //    }
            //    else
            //    {
            //        FileStream fs = new FileStream(strDir, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //        StreamReader sr = new StreamReader(fs);
            //        textBox1.Text = sr.ReadLine();
            //        fs.Close();
            //    }
                
            //}
        }

        public string Encrypt(string str)
        {
            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();   //实例化加/解密类对象   

            byte[] key = Encoding.Unicode.GetBytes(encryptKey); //定义字节数组，用来存储密钥    

            byte[] data = Encoding.Unicode.GetBytes(str);//定义字节数组，用来存储要加密的字符串  

            MemoryStream MStream = new MemoryStream(); //实例化内存流对象      

            //使用内存流实例化加密流对象   
            CryptoStream CStream = new CryptoStream(MStream, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);

            CStream.Write(data, 0, data.Length);  //向加密流中写入数据      

            CStream.FlushFinalBlock();              //释放加密流      

            return Convert.ToBase64String(MStream.ToArray());//返回加密后的字符串  
        }

        public  string Decrypt(string str)
        {
            try
            {
                DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();   //实例化加/解密类对象    

                byte[] key = Encoding.Unicode.GetBytes(encryptKey); //定义字节数组，用来存储密钥    

                byte[] data = Convert.FromBase64String(str);//定义字节数组，用来存储要解密的字符串  

                MemoryStream MStream = new MemoryStream(); //实例化内存流对象      

                //使用内存流实例化解密流对象       
                CryptoStream CStream = new CryptoStream(MStream, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);

                CStream.Write(data, 0, data.Length);      //向解密流中写入数据     

                CStream.FlushFinalBlock();               //释放解密流      

                return Encoding.Unicode.GetString(MStream.ToArray());       //返回解密后的字符串  
            }
            catch 
            { }

            return "error";
            
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string dn = Decrypt(textBox2.Text);

            if (dn == textBox1.Text)
            {

                string strDir = Directory.GetCurrentDirectory() + @"\License.key";
                FileStream fs = new FileStream(strDir, FileMode.Create, FileAccess.ReadWrite);

                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(textBox1.Text);
                sw.WriteLine(textBox2.Text);
                sw.Close();
                fs.Close();
                //MessageBox.Show("注册成功");
                this.DialogResult = DialogResult.OK;

                this.Close();
            }
            else 
            {
                //MessageBox.Show("无效注册码");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "123456789")
            {
                //MessageBox.Show("注册成功");
                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }

        private void 上移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 > 0)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index1].ItemArray;
                dt.Rows[index1].ItemArray = dt.Rows[index1 - 1].ItemArray;
                dt.Rows[index1 - 1].ItemArray = dr.ItemArray;

                dataGridView1.Rows[index1 - 1].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return;
        }

        private void 下移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 < dt.Rows.Count - 1)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index1].ItemArray;
                dt.Rows[index1].ItemArray = dt.Rows[index1 + 1].ItemArray;
                dt.Rows[index1 + 1].ItemArray = dr.ItemArray;

                dataGridView1.Rows[index1 + 1].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return;
        }

        private void 置顶ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 > 0)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index1].ItemArray;

                dt.Rows.RemoveAt(index1);

                dt.Rows.InsertAt(dr, 0);

                dataGridView1.Rows[0].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return;
        }

        private void 置底ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 < dt.Rows.Count - 1)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index1].ItemArray;

                dt.Rows.Add(dr);


                dataGridView1.Rows[dt.Rows.Count - 1].Selected = true;

                dt.Rows.RemoveAt(index1);

            }
            return;
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;
            try
            {

                index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

                if (index1 >= 0)
                {

                    dt.Rows.RemoveAt(index1);
                }
            }
            catch { }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);

                SQLiteClass sqlite = new SQLiteClass();
                if (sqlite.UpdateTestProjectTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", TestProjectName, dt) == 1)
                {
                    MessageBox.Show("保存成功");
                }
            }
            catch
            {
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        //private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        //{
        //    int index1;
        //    try
        //    {

        //        index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

        //        dt.Rows[index1][0] = dataGridView1.SelectedRows[0].Cells[0];
        //    }
        //    catch { }
        //}

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //int index1;
            //try
            //{

            //    index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            //    dt.Rows[index1][1] = dataGridView1.SelectedRows[0].Cells[1];
            //}
            //catch { }
        }



        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
             
                if ((listView1.SelectedItems[0].Index >= 0) && (listView1.SelectedItems[0].Index < listView1.Items.Count))
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == listView1.SelectedItems[0].Text)
                        {
                            MessageBox.Show("已存在此测试步骤");
                            return;
                        }
                    }
                    object[] par = new object[2];
                    par[0] = listView1.SelectedItems[0].Text;
                    par[1] = true;
                    dr.ItemArray = par;
                    int index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                    dt.Rows.InsertAt(dr, index1 + 1);

                    dataGridView1.Rows[index1 + 1].Selected = true;
                }
            }
            catch { }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count <= 0)//这这判断是否点了空白区，点了空白区进到if里   
                MessageBox.Show("请选中items");
        }

        private void 返回ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProjectGlobal.ProjectName != "")
            {

                if (MessageBox.Show("注意：未保存的更改将被丢弃！\r\n返回: " + ProjectGlobal.ProjectName + "  继续测试?", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.Hide();

                    MainTest form = new MainTest();
                    try
                    {
                        form.ShowDialog();
                    }
                    catch
                    {

                    }

                    this.Close();
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
        
            //dataGridView1.Update();

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void 另存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputSN form = new InputSN("请输入项目名称", label4.Text);
            if (form.ShowDialog() == DialogResult.OK)
            {

                    SQLiteClass sqlite = new SQLiteClass();

                    try
                    {

                        DataTable dt1 = sqlite.ReadTestProject(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            if (form.msg == dt1.Rows[i].ItemArray[0].ToString().Replace("_TestProject", ""))
                            {
                                MessageBox.Show("已存在此测试项目名称，请重新输入");
                                return;
                            }
                        }

                        char[] charArray = form.msg.ToCharArray();

                        if ((charArray[0] < 48) || (charArray[0] > 57))
                        {
                            try
                            {

                                sqlite.CreateNewTestProjectTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_TestProject");
                                   
                                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);

                                if (sqlite.UpdateTestProjectTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_TestProject", dt) == 1)
                                {
                                    if (MessageBox.Show("保存成功,是否跳转到：" + form.msg + "  编辑界面", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        this.Hide();
                                        ProjectEdit form1 = new ProjectEdit(form.msg + "_TestProject");
                                        form1.ShowDialog();
                                        this.Close();
                                    }
                                }



                            }
                            catch { }
                        }
                        else
                        {
                            MessageBox.Show("项目名称不能以数字开头，请重输");
                        }
                    }
                    catch { }
            }
        } 


    }
}
