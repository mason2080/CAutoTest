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

namespace AutoTest
{
    public partial class MainSelect : Form
    {
        static string encryptKey = "BNET";    //定义密钥  

        public MainSelect()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
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


            if (ProjectGlobal.UserName == "操作员")
            {
                btnProjectEdit.Enabled = false;
                btnStepEdit.Enabled = false;
                buttonSetDevice.Enabled = false;
            }
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

        private void btnSelectProject_Click(object sender, EventArgs e)
        {
            //this.Hide();

            //ProjectSelect form = new ProjectSelect();

            //if (form.ShowDialog() == DialogResult.OK)
            //{
            //    MainTest formMain = new MainTest();
            //    formMain.ShowDialog();

            //    this.Show();
            //}
            //else 
            //{
            //    this.Show();
            //}

            this.Hide();
            ProjectSelect form = new ProjectSelect();
            form.ShowDialog();
            this.Show();
        }

        private void btnProjectEdit_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectTestProject form = new SelectTestProject();
            form.ShowDialog();
            this.Show();
        }

        private void btnStepEdit_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectTestItem form = new SelectTestItem();
            form.ShowDialog();
            this.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.Hide();
            DllDebug form = new DllDebug();
            form.ShowDialog();
            this.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Hide();
            SelectDeviceConfig form = new SelectDeviceConfig();
            form.ShowDialog();
            this.Show();

            //this.Hide();
            //DeviceEdit form = new DeviceEdit();
            //form.ShowDialog();
            //this.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Hide();
            HistoryData form = new HistoryData();
            form.ShowDialog();
            this.Show();
        }


        //private void calc()
        //{
        //    byte[,] Err = new byte[2, 47];

        //    int sendErrCode ;//发送故障代码
        //    int sendErrLevel;//发送故障等级
        //    int startIndex=0;//搜索故障起始位置

        //    for (int i = 1; i < 48; i++)
        //    { 
        //        if((i>=1)&&(i<=8))
        //        {
        //            if (Err[1, i + startIndex] == 1)
        //            {
        //                sendErrCode = Err[0, i + startIndex];
        //                startIndex = i+1;

        //                if (startIndex >= 8)
        //                {
        //                    startIndex = 0;
        //                }
        //            }
        //            sendErrLevel = 1;
        //            break;
        //        }
        //        else if ((i >= 9) && (i <= 28))
        //        {
        //            if (Err[1, i + startIndex] == 2)
        //            {
        //                sendErrCode = Err[0, i + startIndex];
        //                startIndex = i + 1;

        //                if (startIndex >= 28)
        //                {
        //                    startIndex = 0;
        //                }
        //            }
        //            sendErrLevel = 2;
        //            break;
        //        }
        //        else if ((i >= 29) && (i <= 47))
        //        {
        //            if (Err[1, i + startIndex] == 3)
        //            {
        //                sendErrCode = Err[0, i + startIndex];
        //                startIndex = i + 1;

        //                if (startIndex >= 47)
        //                {
        //                    startIndex = 0;
        //                }
        //            }
        //            sendErrLevel = 3;
        //            break;
        //        }

        //       sendErrCode=0;//发送故障代码
        //       sendErrLevel=0;//发送故障等级

        //    }
        
        
        //}


    }
}
