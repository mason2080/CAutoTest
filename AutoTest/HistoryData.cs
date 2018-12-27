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

namespace AutoTest
{
    public partial class HistoryData : Form
    {
        static string encryptKey = "BNET";    //定义密钥  

        DataTable dt1 = new DataTable();

        public HistoryData()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
        }

        private string GetDiskId()
        {
            string a = "0A1B2C3D4E";
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

        private string GenRandomString()
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

        public string Decrypt(string str)
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

        }

        private void ProjectSelect_Load(object sender, EventArgs e)
        {


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

        private void buttonSearchBarcode_Click(object sender, EventArgs e)
        {
            SQLiteClass sqlite = new SQLiteClass();
            try
            {
                DataTable dt1 = sqlite.SearchHistoryByBarCode(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", textBarCode.Text);
                dataGridView2.DataSource = dt1;

                dataGridView2.Columns[5].Visible = false;
                dataGridView2.Columns[6].Visible = false;
                dataGridView2.Columns[7].Visible = false;
                dataGridView2.Columns[8].Visible = false;
                dataGridView2.Columns[9].Visible = false;
                dataGridView2.Columns[10].Visible = false;
                dataGridView2.Columns[11].Visible = false;
                dataGridView2.Columns[12].Visible = false;
                dataGridView2.Columns[13].Visible = false;
                dataGridView2.Columns[14].Visible = false;
                dataGridView2.Columns[15].Visible = false;
            }
            catch { }
        }

        private void buttonSearchByTime_Click(object sender, EventArgs e)
        {
            SQLiteClass sqlite = new SQLiteClass();
            try
            {
                //if (textTime.Text == "")
                //{
                string histroyTableName = "H" + dtpStart.Value.Year.ToString() + dtpStart.Value.Month.ToString() + dtpStart.Value.Day.ToString();

                DataTable dt1 = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", histroyTableName);

                dataGridView2.DataSource = dt1;
                //}

                dataGridView2.Columns[5].Visible = false;
                dataGridView2.Columns[6].Visible = false;
                dataGridView2.Columns[7].Visible = false;
                dataGridView2.Columns[8].Visible = false;
                dataGridView2.Columns[9].Visible = false;
                dataGridView2.Columns[10].Visible = false;
                dataGridView2.Columns[11].Visible = false;
                dataGridView2.Columns[12].Visible = false;
                dataGridView2.Columns[13].Visible = false;
                dataGridView2.Columns[14].Visible = false;
                dataGridView2.Columns[15].Visible = false;
            }
            catch { }
        }

        private void buttonSaveToCsv_Click(object sender, EventArgs e)
        {

            try
            {
                if ((dataGridView1.SelectedRows.Count >= 0) && (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() != "") && (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != ""))
                {

                    FileStream fileStream;
                    StreamWriter streamWriter;

                    string tempTime = System.DateTime.Now.ToShortDateString().Replace("/", "");
                    saveFileDialog1.FileName = dataGridView2.SelectedRows[0].Cells[0].Value.ToString() + "_" + dataGridView2.SelectedRows[0].Cells[1].Value.ToString();//"History_"+ tempTime.Replace(@"\", "");

                    //saveFileDialog1.FileName = saveFileDialog1.FileName.Replace("-", "_");
                    saveFileDialog1.FileName = saveFileDialog1.FileName.Replace(":", "-");
                    saveFileDialog1.DefaultExt = ".csv";

                    SQLiteClass sqlite = new SQLiteClass();

                    DataTable dt1 = sqlite.SearchHistoryByBarCodeAndTime(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", dataGridView2.SelectedRows[0].Cells[0].Value.ToString(), dataGridView2.SelectedRows[0].Cells[1].Value.ToString());




                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog1.FileName;

                        fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                        streamWriter = new StreamWriter(fileStream, Encoding.Default);

                        string msg = "BarCode" + ",";
                        msg += "Time" + ",";
                        msg += "Result" + ",";
                        msg += "TestItem" + ",";
                        msg += "TestProduct";

                        streamWriter.WriteLine(msg);

                        msg = dt1.Rows[0].ItemArray[0].ToString() + "," + dt1.Rows[0].ItemArray[1].ToString() + "," + dt1.Rows[0].ItemArray[2].ToString() + "," + dt1.Rows[0].ItemArray[3].ToString() + "," + dt1.Rows[0].ItemArray[4].ToString() + "\n";

                        streamWriter.WriteLine(msg);

                        msg = "Name" + "," + "Min" + "," + "Max" + "," + "Meas" + "," + "Result";

                        streamWriter.WriteLine(msg);

                        string[] item = dt1.Rows[0].ItemArray[5].ToString().Split('&');

                        for (int i = 0; i <= item.Length - 1; i++)
                        {

                            if (item[i] != "")
                            {
                                streamWriter.WriteLine(item[i]);
                            }

                        }




                        streamWriter.Close();
                        fileStream.Close();

                        MessageBox.Show("导出成功");
                    }
                }
                else
                {
                    MessageBox.Show("请先选择需要导出的记录");
                }
            }
            catch { }

        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {


            folderBrowserDialog1.Description = "请选择保存历史数据的文件夹";
            folderBrowserDialog1.ShowNewFolderButton = true;
            SQLiteClass sqlite = new SQLiteClass();


            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {

                string filePath = folderBrowserDialog1.SelectedPath;

                for (int i = 0; i < dataGridView2.Rows.Count-1; i++)
                {
                    DataTable dt1 = sqlite.SearchHistoryByBarCodeAndTime(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", dataGridView2.Rows[i].Cells[0].Value.ToString(), dataGridView2.Rows[i].Cells[1].Value.ToString());

                    string tempPath = dataGridView2.Rows[i].Cells[0].Value.ToString() + "_" + dataGridView2.Rows[i].Cells[1].Value.ToString();//"History_"+ tempTime.Replace(@"\", "");

                    if (tempPath != "")
                    {
                        tempPath = tempPath.Replace(":", "-");
                        FileStream fileStream = new FileStream(filePath + "\\" + tempPath + ".csv", FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Default);

                        string msg = "BarCode" + ",";
                        msg += "Time" + ",";
                        msg += "Result" + ",";
                        msg += "TestItem" + ",";
                        msg += "TestProduct";

                        streamWriter.WriteLine(msg);

                        msg = dt1.Rows[0].ItemArray[0].ToString() + "," + dt1.Rows[0].ItemArray[1].ToString() + "," + dt1.Rows[0].ItemArray[2].ToString() + "," + dt1.Rows[0].ItemArray[3].ToString() + "," + dt1.Rows[0].ItemArray[4].ToString() + "\n";

                        streamWriter.WriteLine(msg);

                        msg = "Name" + "," + "Min" + "," + "Max" + "," + "Meas" + "," + "Result";

                        streamWriter.WriteLine(msg);

                        string[] item = dt1.Rows[0].ItemArray[5].ToString().Split('&');

                        for (int j = 0; j <= item.Length - 1; j++)
                        {

                            if (item[j] != "")
                            {
                                streamWriter.WriteLine(item[j]);
                            }

                        }

                        streamWriter.Close();
                        fileStream.Close();
                    }
                    MessageBox.Show("导出成功");
                }
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                SQLiteClass sqlite = new SQLiteClass();

                if ((dataGridView1.SelectedRows.Count >= 0) && (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() != "") && (dataGridView2.SelectedRows[0].Cells[1].Value.ToString() != ""))
                {
                    DataTable dt = new DataTable();     //创建一个空表
                    dt.Clear();


                    dt.TableName = "test";              //为这个表
                    dt.Columns.Add("Name");       //为这个表加一个名叫pageNumber的字段
                    dt.Columns.Add("Min");    //为这个表加一个名叫graphPageName的字段
                    dt.Columns.Add("Max");    //为这个表加一个名叫graphPageName的字段
                    dt.Columns.Add("Value");    //为这个表加一个名叫graphPageName的字段
                    dt.Columns.Add("Result");    //为这个表加一个名叫graphPageName的字段

                    DataTable dt1 = sqlite.SearchHistoryByBarCodeAndTime(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", dataGridView2.SelectedRows[0].Cells[0].Value.ToString(), dataGridView2.SelectedRows[0].Cells[1].Value.ToString());


                    //string msg = "BarCode" + ",";
                    //msg += "Time" + ",";
                    //msg += "Result" + ",";
                    //msg += "TestItem" + ",";
                    //msg += "TestProduct"+"\r";



                    string msg = dt1.Rows[0].ItemArray[0].ToString() + "," + dt1.Rows[0].ItemArray[1].ToString() + "," + dt1.Rows[0].ItemArray[2].ToString() + "," + dt1.Rows[0].ItemArray[3].ToString() + "," + dt1.Rows[0].ItemArray[4].ToString() + "\n";



                    //msg += "Name" + "," + "Min" + "," + "Max" + "," + "Meas" + "," + "Result"+"\r";



                    string[] item = dt1.Rows[0].ItemArray[5].ToString().Split('&');

                    for (int i = 0; i <= item.Length - 1; i++)
                    {

                        if (item[i] != "")
                        {
                            string[] row = item[i].Split(',');

                            DataRow dr = dt.NewRow();        //为这个表加一行记录   
                            dr["Name"] = row[0];//
                            dr["Min"] = row[1];    //
                            dr["Max"] = row[2];    //
                            dr["Value"] = row[3];    //
                            dr["Result"] = row[4];    //
                            dt.Rows.Add(dr);                //将这一行记录加这个表中
                        }

                    }

                    dataGridView4.DataSource = dt;
                    //PreviewHistory form = new PreviewHistory(msg, dt);
                    //form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("请先选择需要导出的记录");
                }
            }
            catch { }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            SQLiteClass sqlite = new SQLiteClass();


            DataTable dt = new DataTable();     //创建一个空表
            dt.Clear();


            dt.TableName = "test";              //为这个表
            dt.Columns.Add("Name");       //为这个表加一个名叫pageNumber的字段
            dt.Columns.Add("Min");    //为这个表加一个名叫graphPageName的字段
            dt.Columns.Add("Max");    //为这个表加一个名叫graphPageName的字段
            dt.Columns.Add("Value");    //为这个表加一个名叫graphPageName的字段
            dt.Columns.Add("Result");    //为这个表加一个名叫graphPageName的字段

            if ((dataGridView1.SelectedRows.Count >= 0)&&(dataGridView2.SelectedRows[0].Cells[0].Value.ToString()!="")&&(dataGridView2.SelectedRows[0].Cells[1].Value.ToString()!=""))
            {

                DataTable dt1 = sqlite.SearchHistoryByBarCodeAndTime(Directory.GetCurrentDirectory() + "\\AutoTestResult.db", dataGridView2.SelectedRows[0].Cells[0].Value.ToString(), dataGridView2.SelectedRows[0].Cells[1].Value.ToString());


                //string msg = "BarCode" + ",";
                //msg += "Time" + ",";
                //msg += "Result" + ",";
                //msg += "TestItem" + ",";
                //msg += "TestProduct"+"\r";



                string msg = dt1.Rows[0].ItemArray[0].ToString() + "," + dt1.Rows[0].ItemArray[1].ToString() + "," + dt1.Rows[0].ItemArray[2].ToString() + "," + dt1.Rows[0].ItemArray[3].ToString() + "," + dt1.Rows[0].ItemArray[4].ToString() + "\n";



                //msg += "Name" + "," + "Min" + "," + "Max" + "," + "Meas" + "," + "Result"+"\r";



                string[] item = dt1.Rows[0].ItemArray[5].ToString().Split('&');

                for (int i = 0; i <= item.Length - 1; i++)
                {

                    if (item[i] != "")
                    {
                        string[] row = item[i].Split(',');

                        DataRow dr = dt.NewRow();        //为这个表加一行记录   
                        dr["Name"] = row[0];//
                        dr["Min"] = row[1];    //
                        dr["Max"] = row[2];    //
                        dr["Value"] = row[3];    //
                        dr["Result"] = row[4];    //
                        dt.Rows.Add(dr);                //将这一行记录加这个表中
                    }

                }

                dataGridView4.DataSource = dt;
                //PreviewHistory form = new PreviewHistory(msg, dt);
                //form.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先选择需要导出的记录");
            }
        }


    }
}
