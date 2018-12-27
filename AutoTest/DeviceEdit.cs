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
using System.Reflection;
using Driver.File.Sql;

namespace AutoTest
{
    public partial class DeviceEdit : Form
    {
        private static Assembly assmeblyObject;
        private string testItemName;
        DataTable stepTable = new DataTable();
        DataTable dt;
        List<string> allMethodParamaters = new List<string>();
        static string encryptKey = "BNET";    //定义密钥  

        public DeviceEdit()
        {
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
        }

        public DeviceEdit(string name)
        {
            testItemName = name;
            InitializeComponent();

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;

            this.Text=name.Replace("_DeviceSetting","");
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

            this.Name = testItemName;

            SQLiteClass sqlite = new SQLiteClass();
            try
            {
                dt = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItemName);
                dataGridView1.DataSource = dt;
                dataGridView1.ColumnHeadersVisible = true;
                dataGridView1.RowHeadersVisible = false;
                DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
                columnHeaderStyle.BackColor = Color.Beige;
                columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
                dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[4].ReadOnly = true;

                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
            catch 
            {

            }



            SearchAllMethod();

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

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DataRow dr =dt.NewRow();
            try
            {
                if (treeView1.SelectedNode.IsSelected == true)
                {
                    if (treeView1.SelectedNode.Level == 0)
                    {
                        string[] param = treeView1.SelectedNode.ImageKey.Split('&');
                        textFunc.Text = treeView1.SelectedNode.Text;
                        //textFunc.Text = treeView1.SelectedNode.ImageKey;
                        //MessageBox.Show(treeView1.SelectedNode.Index.ToString());
                       // DataGridViewRow dr = new DataGridViewRow();
                        dr[0]=true;
                        dr[1] = "";
                        dr[2] = treeView1.SelectedNode.Text; 
                        dr[4] = treeView1.SelectedNode.ImageKey;
                       // dt.Rows.Add(dr);

                       int  index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                       dt.Rows.InsertAt(dr,index1 + 1);

                       if (dt.Rows.Count != 1)
                       {
                           dataGridView1.Rows[index1 + 1].Selected = true;
                       }

                       DataTable dataTable = new DataTable();
                       if (param[2] != "")
                       {
                           string[] param1 = param[2].Split(',');
                           if (param1.Count() > 0)
                           {

                               dataTable.Columns.Add("参数名");
                               dataTable.Columns.Add("参数");

                               for (int i = 0; i < param1.Count(); i++)
                               {
                                   DataRow row = dataTable.NewRow();
                                   string[] par = new string[2];
                                   par[0] = param1[i];
                                   par[1] = "";
                                   row.ItemArray = par;
                                   //row.ItemArray[1] = "aa";
                                   //row.ItemArray[0] = param1[i];
                                   dataTable.Rows.Add(row);
                                   dr[3] += ",";
                               }
                           }
                       }
                       else { }

                       dataGridView2.DataSource = dataTable;
                       dataGridView2.Columns[0].ReadOnly = true;

                    }
                }
            }
            catch { }
        }

        private void SearchAllMethod()
        {
            //allMethodParamaters.Clear();
            string strDir = Directory.GetCurrentDirectory() + @"\driver";

            ParameterInfo[] paramsInfo=null;
            int length;
           string methodString;
            string methodParams;


            foreach (string file in Directory.EnumerateFiles(strDir, "*.dll"))
            {
                try
                {
                    TreeNode node = new TreeNode();

                    allMethodParamaters.Add(Path.GetFileName(file));
                    assmeblyObject = Assembly.LoadFile(file);
                    Type[] typeClasses = assmeblyObject.GetTypes();
                    //List<string> MethodList = new List<string>();
                    foreach (Type typeClass in typeClasses)
                    {
                        object[] autoTestClassAttribute = typeClass.GetCustomAttributes(typeof(AutoTestAttribute.AutoTestClassAttribute), true);

                        foreach (object property in autoTestClassAttribute)
                            {
                                AutoTestAttribute.AutoTestClassAttribute objProperty = (AutoTestAttribute.AutoTestClassAttribute)property;
                                if (objProperty.Description != "")
                                {
                                    methodString = Path.GetFileName(file) + "&" + typeClass.FullName +  "&" + objProperty.Parameters;
                                    //allMethodParamaters.Add(methodString);
                                    node = treeView1.Nodes.Add(methodString, objProperty.Description, methodString);
                                    break;
                                }
                            }
                    }
                }
                catch { }
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
                dt.Rows[index1].ItemArray = dt.Rows[index1-1].ItemArray;
                dt.Rows[index1-1].ItemArray = dr.ItemArray;

                dataGridView1.Rows[index1 - 1].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return ;
        }

        private void 下移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 < dt.Rows.Count-1)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index1].ItemArray;
                dt.Rows[index1].ItemArray = dt.Rows[index1 +1].ItemArray;
                dt.Rows[index1 +1].ItemArray = dr.ItemArray;

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

                dt.Rows.InsertAt(dr,0);

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


                dataGridView1.Rows[dt.Rows.Count-1].Selected = true;

                dt.Rows.RemoveAt(index1);

            }
            return;
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);

                SQLiteClass sqlite = new SQLiteClass();
                if (sqlite.UpdateDeviceSettingTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItemName, dt) == 1)
                {
                    MessageBox.Show("保存成功");
                }
            }
            catch 
            {
            }
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

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                e.Cancel = true;
            }
        }

        private void btnModify_Click(object sender, EventArgs e)
        {

            try
            {
                int index = dataGridView1.SelectedRows[0].Index;


                if ((textFunc.Text != "") && (textFunc.Text == dataGridView1.Rows[index].Cells[1].Value.ToString()))
                {

                }
                else
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[0].Value != null)
                        {
                            if (textFunc.Text == dataGridView1.Rows[i].Cells[1].Value.ToString())
                            {
                                MessageBox.Show("已经存在此调用名，请重新输入");
                                return;
                            }
                            else
                            {

                            }
                        }

                    }
                    dt.Rows[index][1] = textFunc.Text;
                }
                dt.Rows[index][3] = "";
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (dataGridView2.Rows[i].Cells[0].Value != null)
                    {
                        dt.Rows[index][3] += dataGridView2.Rows[i].Cells[1].Value.ToString() + ",";
                    }
                }
            }
            catch { }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = dataGridView1.SelectedRows[0].Index;

            DataTable dataTable = new DataTable();

            if (dataGridView1.Rows[index].Cells[1].Value.ToString() != "")
            {
                textFunc.Text = dataGridView1.Rows[index].Cells[1].Value.ToString();
            }
            else
            {
                textFunc.Text = dataGridView1.Rows[index].Cells[2].Value.ToString();
            }

            if (dataGridView1.Rows[index].Cells[3].Value.ToString() != "")
            {
                string[] param = dataGridView1.Rows[index].Cells[4].Value.ToString().Split('&');
                string[] param2 = param[2].Split(',');

                string[] param1 = dataGridView1.Rows[index].Cells[3].Value.ToString().Split(',');


                if (param1.Count() > 0)
                {

                    dataTable.Columns.Add("参数名");
                    dataTable.Columns.Add("参数");

                    for (int i = 0; i < param2.Count(); i++)
                    {
                        DataRow row = dataTable.NewRow();
                        string[] par = new string[2];
                        par[0] = param2[i];
                        par[1] = param1[i];
                        row.ItemArray = par;
                        //row.ItemArray[1] = "aa";
                        //row.ItemArray[0] = param1[i];
                        dataTable.Rows.Add(row);
                    }
                }
            }
            else { }

            dataGridView2.DataSource = dataTable;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                string param = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();

                for (int i = 0; i < treeView1.Nodes.Count; i++)
                {
                    if (treeView1.Nodes[i].Text == param)
                    {
                        treeView1.SelectedNode = treeView1.Nodes[i];

                        treeView1.SelectedNode.BackColor = Color.Aqua;
                        // treeView1.SelectedNode.Checked = true;

                    }
                    else
                    {
                        treeView1.Nodes[i].BackColor=Color.White;
                    }
                }
                //textFunc.Text = treeView1.SelectedNode.Text;
            }
            catch { }
        }

        private void 另存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputSN form = new InputSN("请输入设备配置名称",this.Text);
            if (form.ShowDialog() == DialogResult.OK)
            {

                    SQLiteClass sqlite = new SQLiteClass();

                    try
                    {

                        DataTable dt1 = sqlite.ReadTestProject(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            if (form.msg == dt1.Rows[i].ItemArray[0].ToString().Replace("_DeviceSetting", ""))
                            {
                                MessageBox.Show("已存在此设备配置名称，请重新输入");
                                return;
                            }
                        }

                        char[] charArray = form.msg.ToCharArray();

                        if ((charArray[0] < 48) || (charArray[0] > 57))
                        {
                            try
                            {

                                sqlite.CreateNewDeviceTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_DeviceSetting");
                                   
                                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);

                                if (sqlite.UpdateDeviceSettingTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_DeviceSetting", dt) == 1)
                                {
                                    if (MessageBox.Show("保存成功,是否跳转到：" + form.msg + "  编辑界面", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        this.Hide();
                                        DeviceEdit form1 = new DeviceEdit(form.msg + "_DeviceSetting");
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
