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
using System.Threading;
using Driver.File.Ini;

namespace AutoTest
{
    public partial class ProjectSelect : Form
    {
        static string encryptKey = "BNET";    //定义密钥  

        IniFileClass iniFileClass = new IniFileClass();

        public ProjectSelect()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;

            ProjectGlobal.Title = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "公司名称");

            label6.Text = ProjectGlobal.Title;
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

        private void btnStart_Click(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode.IsSelected == true)
            //{
            //    if (treeView1.SelectedNode.Level == 1)
            //    {
            //        ProjectGlobal.ProjectName = treeView1.SelectedNode.Text;

            //        this.DialogResult = DialogResult.OK;

            //        this.Close();
            //    }
            //}
           
        }

        private void ProjectSelect_Load(object sender, EventArgs e)
        {



            SQLiteClass sqlite = new SQLiteClass();

            try
            {

                DataTable dt1 = sqlite.ReadTestProject(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    listView1.Items.Add(dt1.Rows[i].ItemArray[0].ToString().Replace("_TestProject", ""));
                }
            }
            catch { }

            //dataGridView1.Columns[0].Width = 300;
            //dataGridView1.Columns[1].Width = 200;
            //dataGridView1.Columns[2].Width = 200;

 
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
           // MessageBox.Show(dataGridView1.SelectedRows[0].Index.ToString());

            ProjectGlobal.ProjectName = dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[0].ToString();
            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
           // MessageBox.Show(dataGridView1.SelectedRows[0].Index.ToString());

            ProjectGlobal.ProjectName = dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[0].Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ProjectGlobal.ProjectName = listView1.SelectedItems[0].Text;
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

        private void listView1_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count <= 0)//这这判断是否点了空白区，点了空白区进到if里   
                MessageBox.Show("请选中items");
        } 


    }
}
