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
    public partial class StepEdit : Form
    {
        private static Assembly assmeblyObject;
        private string testItemName;
        DataTable stepTable = new DataTable();
        DataTable dt;
        DataTable resultTable;
        List<string> allMethodParamaters = new List<string>();

        List<string> deviceNames= new List<string>();


        static string encryptKey = "BNET";    //定义密钥  

        DataRow tempResultTableRow;
        DataRow tempDtRow;

        string selectDevice = "";




      

        
     



        public StepEdit()
        {

     
            InitializeComponent();
            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
        }

        public StepEdit(string name)
        {
            testItemName = name;
            InitializeComponent();

            textBox4.Text = testItemName.Replace("_TestItem", "");

            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;

            checkBox1.Checked = false;
            //Sunisoft.IrisSkin.SkinEngine se = null;
            //se = new Sunisoft.IrisSkin.SkinEngine();
            //se.SkinAllForm = true;
        }




        private void Register_Load(object sender, EventArgs e)
        {

            this.Name = testItemName;
            string variableName = testItemName.Replace("_TestItem", "_Variable");
          

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
                dataGridView1.Columns[2].ReadOnly = true; ;

                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[4].ReadOnly = true;

                //dataGridView1.Columns[7].ReadOnly = true;

               // dataGridView1.Columns[9].ReadOnly = true;

               // dataGridView1.Columns[8].Visible = false;




                resultTable = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", variableName);
                dataGridView3.DataSource = resultTable;

                if (resultTable.Rows.Count == 0)
                {
                    dataGridView3.Enabled = false;
                }
                dataGridView3.ColumnHeadersVisible = true;
                dataGridView3.RowHeadersVisible = false;
                DataGridViewCellStyle columnHeaderStyle1 = new DataGridViewCellStyle();
                columnHeaderStyle1.BackColor = Color.Beige;
                columnHeaderStyle1.Font = new Font("Verdana", 10, FontStyle.Bold);
                dataGridView3.ColumnHeadersDefaultCellStyle = columnHeaderStyle1;

                dataGridView3.Columns[0].ReadOnly = false;
                dataGridView3.Columns[1].ReadOnly = false;
                dataGridView3.Columns[2].ReadOnly = false;
                dataGridView3.Columns[3].ReadOnly = true;
                dataGridView3.Columns[4].ReadOnly = false;


                dataGridView3.Columns[8].Visible = false;
                dataGridView3.Columns[9].Visible = false;


                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                for (int i = 0; i < dataGridView3.Columns.Count; i++)
                {
                    dataGridView3.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }


                for (int i = 0; i < resultTable.Rows.Count; i++)
                {
                    listViewVariable.Items.Add(resultTable.Rows[i][4].ToString());
                }


                DataTable table = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", "BNET_DeviceSettings");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    listViewDevice.Items.Add(table.Rows[i][1].ToString());

                    deviceNames.Add(table.Rows[i][1].ToString() + "&" + table.Rows[i][4].ToString());
                }


            }
            catch 
            {

            }

            SearchAllMethod();
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {


            DataRow dr =dt.NewRow();
            try
            {
                if (treeView1.SelectedNode.IsSelected == true)
                {
                    if (treeView1.SelectedNode.Level == 1)
                    {
                        string[] param = treeView1.SelectedNode.ImageKey.Split('&');
                        textFunc.Text = treeView1.SelectedNode.Text;
                        //textFunc.Text = treeView1.SelectedNode.ImageKey;
                        //MessageBox.Show(treeView1.SelectedNode.Index.ToString());
                       // DataGridViewRow dr = new DataGridViewRow();
                        dr[0]=true;
                        dr[1] = param[2];
                        dr[4] = testItemName.Replace("_TestItem","");
                        dr[5] = 100;
                        dr[6] = 0;
                        dr[9] = treeView1.SelectedNode.ImageKey;
                       // dt.Rows.Add(dr);



                        int index1 = 0;
                        if (dt.Rows.Count != 0)
                        {
                            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                            dt.Rows.InsertAt(dr, index1 + 1);
                            dataGridView1.Rows[index1 + 1].Selected = true;
                        }
                        else
                        {
                            dt.Rows.InsertAt(dr, 0);
                        }

                       //int  index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                       //dt.Rows.InsertAt(dr,index1 + 1);
                       //if (dt.Rows.Count != 1)
                       //{
                       //    dataGridView1.Rows[index1 + 1].Selected = true;
                       //}

                       DataTable dataTable = new DataTable();
                       if (param[3] != "")
                       {
                           string[] param1 = param[3].Split(',');
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
                                   dr[2] += ",";
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
            allMethodParamaters.Clear();
            string strDir = Directory.GetCurrentDirectory() + @"\driver";

            ParameterInfo[] paramsInfo=null;
            int length;
           string methodString;
            string methodParams;


            foreach (string file in Directory.EnumerateFiles(strDir, "*.dll"))//强制规定一个DLL中只能有一个类，要用多个类请新建DLL
            {
                try
                {

                    TreeNode node = new TreeNode();
                    assmeblyObject = Assembly.LoadFile(file);
                    Type[] typeClasses = assmeblyObject.GetTypes();



                    foreach (Type typeClass in typeClasses)
                    {


                       object[] objClasses = typeClass.GetCustomAttributes(typeof(AutoTestAttribute.AutoTestClassAttribute), true);

                       foreach (object objClass in objClasses)
                       {
                           AutoTestAttribute.AutoTestClassAttribute objClassProperty = (AutoTestAttribute.AutoTestClassAttribute)objClass;

                           if (objClassProperty.Description != "")//过滤其它DLL
                           {

                               node = treeView1.Nodes.Add(Path.GetFileName(file));
                               allMethodParamaters.Add(Path.GetFileName(file));
                           }



                               object classObject = assmeblyObject.CreateInstance(typeClass.FullName);
                               MethodInfo[] methods = typeClass.GetMethods();

                               foreach (MethodInfo method in methods)
                               {
                                   object[] testPropertyAttributes = method.GetCustomAttributes(typeof(AutoTestAttribute.DescriptionAttribute), true);
                                   foreach (object property in testPropertyAttributes)
                                   {
                                       AutoTestAttribute.DescriptionAttribute objProperty = (AutoTestAttribute.DescriptionAttribute)property;

                                       if (objProperty.Description != "")
                                       {
                                           paramsInfo = method.GetParameters();//得到指定方法的参数列表   
                                           length = paramsInfo.Length;
                                           methodParams = "";

                                           methodString = Path.GetFileName(file) + "&" + typeClass.FullName + "&" + method.Name + "&" + objProperty.Parameters;
                                           allMethodParamaters.Add(methodString);
                                           node.Nodes.Add(methodString, objProperty.Description, methodString);

                                           break;
                                       }
                                   }
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
                dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);


                string variableName = testItemName.Replace("_TestItem", "_Variable");

                SQLiteClass sqlite = new SQLiteClass();

                dt= dataGridView1.DataSource as DataTable;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i].ItemArray[3].ToString() == "")
                    {
                        MessageBox.Show("第" + (i + 1).ToString() + "行，设备调用名不能为空");
                        return;
                    
                    }
                }

                if (sqlite.UpdateTestItemTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItemName, dt) == 1)
                {
                    dt = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItemName);
                    dataGridView1.DataSource = dt;

                    resultTable = dataGridView3.DataSource as DataTable;

                    if (sqlite.UpdateVariableTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", variableName, resultTable) == 1)
                    {

                        resultTable = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", variableName);

                        dataGridView3.DataSource = resultTable;

                        listViewVariable.Items.Clear();

                        for (int i = 0; i < resultTable.Rows.Count; i++)
                        {
                            listViewVariable.Items.Add(resultTable.Rows[i][4].ToString());
                        }

                        MessageBox.Show("保存成功");
                    }
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
            if(textDevice.Text=="")
            {
                MessageBox.Show("设备调用名不能为空!!");
            }
            else
            {
                int index = dataGridView1.SelectedRows[0].Index;

                dt.Rows[index][2] = "";

                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    if (dataGridView2.Rows[i].Cells[0].Value!=null)
                    {
                        dt.Rows[index][2] += dataGridView2.Rows[i].Cells[1].Value.ToString() + ",";
                    }
                }
               
                dt.Rows[index][3] = textDevice.Text;
           }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = dataGridView1.SelectedRows[0].Index;

            textFunc.Text = dataGridView1.Rows[index].Cells[1].Value.ToString();
            textDevice.Text = dataGridView1.Rows[index].Cells[3].Value.ToString();

            DataTable dataTable = new DataTable();


            //搜索选中项目可用的设备名称
            string[] param3 = dataGridView1.Rows[index].Cells[9].Value.ToString().Split('&');
            listViewDevice.Clear();
            for (int i = 0; i < deviceNames.Count; i++)
            {
                if (deviceNames[i].IndexOf(param3[0]) > 0)
                {
                    string[] temp = deviceNames[i].ToString().Split('&');
                    listViewDevice.Items.Add(temp[0]);
                }
            }


            //参数设置
            if (dataGridView1.Rows[index].Cells[2].Value.ToString() != "")
            {
                string[] param = dataGridView1.Rows[index].Cells[9].Value.ToString().Split('&');
                string[] param2 = param[3].Split(',');

                string[] param1 = dataGridView1.Rows[index].Cells[2].Value.ToString().Split(',');



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

        private void listViewDevice_DoubleClick(object sender, EventArgs e)
        {
            textDevice.Text = listViewDevice.SelectedItems[0].Text;
        }

        private string GenRandomString()
        {
            Random rd = new Random();
            string randomString = "";
            for (byte i = 0; i < 5; i++)
            {
                randomString += rd.Next(10);
            }
            return randomString;
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DataRow dr = resultTable.NewRow();
            try
            {
                dr[0] = true;
                dr[1] = true;
                dr[2] = true;
                dr[3] = this.Name.Replace("_TestItem", "");
                dr[4] = "变量" + GenRandomString();
                dr[5] = "0";
                dr[6] = "0";
                dr[7] = "0";
                dr[8] = "";
                dr[9] = "";


                int index1 =0;
                if (resultTable.Rows.Count != 0)
                {
                    index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                    resultTable.Rows.InsertAt(dr, index1 + 1);
                }
                else 
                {
                    resultTable.Rows.InsertAt(dr, 0);
                }
                
               

                if (resultTable.Rows.Count != 1)
                {
                    dataGridView3.Rows[index1 + 1].Selected = true;
                }

                if (resultTable.Rows.Count != 0)
                {
                    dataGridView3.Enabled = true;
                }
            }
            catch { }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                treeView1.Enabled = false;
                btnModify.Enabled = false;

                删除ToolStripMenuItem.Enabled = false;
                置底ToolStripMenuItem.Enabled = false;
                下移ToolStripMenuItem.Enabled = false;
                上移ToolStripMenuItem.Enabled = false;
                置顶ToolStripMenuItem.Enabled = false;
            }
            else 
            {
                try
                {
                    DataView dv = new DataView(resultTable);

                    if (dv.Count != dv.ToTable(true, "名称").Rows.Count)
                    {
                        MessageBox.Show("变量名不唯一，请检查");
                        tabControl1.SelectedIndex = 1;
                        return;
                    }
                }
                catch { }


                treeView1.Enabled = true;
                btnModify.Enabled = true;

                删除ToolStripMenuItem.Enabled = true;
                置底ToolStripMenuItem.Enabled = true;
                下移ToolStripMenuItem.Enabled = true;
                上移ToolStripMenuItem.Enabled = true;
                置顶ToolStripMenuItem.Enabled = true;
            }
        }

        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;
            try
            {

                index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

                if (index1 >= 0)
                {

                    resultTable.Rows.RemoveAt(index1);
                }

                if (resultTable.Rows.Count == 0)
                {
                    dataGridView3.Enabled = false;
                }
            }
            catch { }
        }

        private void 上移ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 > 0)
            {
                DataRow dr = resultTable.NewRow();
                dr.ItemArray = resultTable.Rows[index1].ItemArray;
                resultTable.Rows[index1].ItemArray = resultTable.Rows[index1 - 1].ItemArray;
                resultTable.Rows[index1 - 1].ItemArray = dr.ItemArray;

                dataGridView3.Rows[index1 - 1].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return;
        }

        private void 下移ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 < resultTable.Rows.Count - 1)
            {
                DataRow dr = resultTable.NewRow();
                dr.ItemArray = resultTable.Rows[index1].ItemArray;
                resultTable.Rows[index1].ItemArray = resultTable.Rows[index1 + 1].ItemArray;
                resultTable.Rows[index1 + 1].ItemArray = dr.ItemArray;

                dataGridView3.Rows[index1 + 1].Selected = true;
            }
            return;
        }

        private void 置顶ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 > 0)
            {
                DataRow dr = resultTable.NewRow();
                dr.ItemArray = resultTable.Rows[index1].ItemArray;

                resultTable.Rows.RemoveAt(index1);

                resultTable.Rows.InsertAt(dr, 0);

                dataGridView3.Rows[0].Selected = true;
                //dataGridView1.Rows[index1].Selected = false;
            }
            return;
        }

        private void 置底ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            if (index1 < resultTable.Rows.Count - 1)
            {
                DataRow dr = resultTable.NewRow();
                dr.ItemArray = resultTable.Rows[index1].ItemArray;

                resultTable.Rows.Add(dr);
                dataGridView3.Rows[resultTable.Rows.Count - 1].Selected = true;
                resultTable.Rows.RemoveAt(index1);
            }
            return;
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            tempResultTableRow = resultTable.NewRow();

            tempResultTableRow.ItemArray = resultTable.Rows[index1].ItemArray;

            return;
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = resultTable.NewRow();
            object[] temp = new object[7];

            int index1;

            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;


            temp = tempResultTableRow.ItemArray;

            temp[4] = temp[4].ToString() + "("+GenRandomString()+")";

            dr.ItemArray = temp;

            resultTable.Rows.InsertAt(dr, index1+1);

            return;
        }

        private void dataGridView3_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 3)
                {
                    DataView dv = new DataView(resultTable);
                    int index1;


                    index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                    char[] charArray = dataGridView3.SelectedRows[0].Cells[3].Value.ToString().ToCharArray();

                    if (dataGridView3.SelectedRows[0].Cells[3].Value.ToString() == "")
                    {
                        MessageBox.Show("变量名不能为空");
                        dataGridView3.Rows[index1].Cells[3].Value = "变量" + GenRandomString();

                    }
                    else if ((charArray[0] >= 48) && (charArray[0] <= 57))
                    {
                        MessageBox.Show("不能以数字开头");
                        dataGridView3.Rows[index1].Cells[3].Value = "变量" + GenRandomString();

                    }
                    else if (dv.Count != dv.ToTable(true, "名称").Rows.Count)
                    {
                        MessageBox.Show("已存在此变量名");
                        dataGridView3.Rows[index1].Cells[3].Value = "变量" + GenRandomString();

                    }

                    //dataGridView3.Rows[index1].Selected=true;
                }
            }
            catch { }
        }

        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index1;

            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

            tempDtRow = dt.NewRow();

            tempDtRow.ItemArray = dt.Rows[index1].ItemArray;

            return;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                DataRow dr = dt.NewRow();
                int index1;
                index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

                dr.ItemArray = tempDtRow.ItemArray;
                dt.Rows.InsertAt(dr, index1 + 1);
            }

            return;
        }

        private void textDevice_Click(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = 0;
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            tabControl2.SelectedIndex = 1;
        }

        private void listViewVariable_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                int index;
                index = dataGridView2.SelectedRows[0].Index;

                dataGridView2.Rows[index].Cells[1].Value = listViewVariable.SelectedItems[0].Text;
            }
            catch { }
        }

        private void 返回ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // ProjectGlobal.ProjectName = listView1.SelectedItems[0].Text;

            if (ProjectGlobal.ProjectName != "")
            {

                if (MessageBox.Show("注意：未保存更改的将被丢弃！\r\n返回: " + ProjectGlobal.ProjectName + "  继续测试?", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        private void StepEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            ProjectGlobal.ProjectName = "";
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if ((selectDevice != dataGridView1.SelectedRows[0].Cells[3].Value.ToString())&&(checkBox1.Checked==true))
                {
                    selectDevice = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
                    string[] param = dataGridView1.SelectedRows[0].Cells[9].Value.ToString().Split('&');
                    for (int i = 0; i < treeView1.Nodes.Count; i++)
                    {
                        if (treeView1.Nodes[i].Text == param[0])
                        {
                            treeView1.CollapseAll();
                            treeView1.SelectedNode = treeView1.Nodes[i];
                            treeView1.SelectedNode.Expand();
                            return;
                        }
                    }
                }
            }
            catch { }  
        }

        private void treeView1_Click(object sender, EventArgs e)
        {
            //if (this.treeView1.SelectedNode.Index <= 0)//这这判断是否点了空白区，点了空白区进到if里   
            //    MessageBox.Show("请选中items！");
        }

        private void 另存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InputSN form = new InputSN("请输入步骤名称", textBox4.Text);
            if (form.ShowDialog() == DialogResult.OK)
            {

                SQLiteClass sqlite = new SQLiteClass();

                try
                {
                    DataTable dt1 = sqlite.ReadTestItem(Directory.GetCurrentDirectory() + "\\AutoTest.db", "");
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        if (form.msg == dt1.Rows[i].ItemArray[0].ToString().Replace("_TestItem", ""))
                        {
                            MessageBox.Show("已存在此测试步骤名称，请重新输入");
                            return;
                        }
                    }

                    char[] charArray = form.msg.ToCharArray();

                    if ((charArray[0] < 48) || (charArray[0] > 57))
                    {
                        try
                        {

                            sqlite.CreateNewTestItemTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_TestItem");
                            sqlite.CreateNewVariableTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg + "_Variable");

                            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);

                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dataGridView3.CommitEdit(DataGridViewDataErrorContexts.Commit);


                string variableName = form.msg+ "_Variable";

                dt= dataGridView1.DataSource as DataTable;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i].ItemArray[3].ToString() == "")
                    {
                        MessageBox.Show("第" + (i + 1).ToString() + "行，设备调用名不能为空");
                        return;
                    
                    }
                }

                if (sqlite.UpdateTestItemTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg+ "_TestItem", dt) == 1)
                {
                    dt = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", form.msg+ "_TestItem");
                    dataGridView1.DataSource = dt;

                    resultTable = dataGridView3.DataSource as DataTable;

                    if (sqlite.UpdateVariableTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", variableName, resultTable) == 1)
                    {

                        resultTable = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", variableName);

                        dataGridView3.DataSource = resultTable;

                        listViewVariable.Items.Clear();

                        for (int i = 0; i < resultTable.Rows.Count; i++)
                        {
                            listViewVariable.Items.Add(resultTable.Rows[i][4].ToString());
                        }

                        if (MessageBox.Show("保存成功,是否跳转到：" + form.msg + "  编辑界面", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            this.Hide();
                            StepEdit form1 = new StepEdit(form.msg + "_TestItem");
                            form1.ShowDialog();
                            this.Close();
                        }
                    }
                }
            }
            catch 
            {
            }

                    }
                    else
                    {
                        MessageBox.Show("项目名称不能以数字开头，请重输");
                    }
                }
                catch { }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            NewArray form = new NewArray();

            if (form.ShowDialog() == DialogResult.OK)
            {
                string arrayName = form.name;
                int arraySize = form.size;

                for (int i = 0; i < arraySize; i++)
                {
                    DataRow dr = resultTable.NewRow();
                    try
                    {
                        dr[0] = true;
                        dr[1] = true;
                        dr[2] = true;
                        dr[3] = this.Name.Replace("_TestItem", "");
                        dr[4] = "Array_" + arrayName+"_"+(i+1).ToString();
                        dr[5] = form.min.ToString();
                        dr[6] = form.max.ToString();
                        dr[7] = "0";
                        dr[8] = "";
                        dr[9] = "";


                        int index1 = 0;
                        if (resultTable.Rows.Count != 0)
                        {
                            index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;
                            resultTable.Rows.InsertAt(dr, index1+1);
                        }
                        else
                        {
                            resultTable.Rows.InsertAt(dr, 0);
                        }



                        if (resultTable.Rows.Count != 1)
                        {
                            dataGridView3.Rows[index1 + 1].Selected = true;
                        }

                        if (resultTable.Rows.Count != 0)
                        {
                            dataGridView3.Enabled = true;
                        }
                    }
                    catch { }
                }
            }

        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.V)
            {
                try
                {
                     string  pasteText = Clipboard.GetText();

                     if (string.IsNullOrEmpty(pasteText))
                         return;

                        Object[] data1=new object[10];

                        String rowStr;

                        string[] temp = pasteText.Split('\t');

                        for (int colIndex = 0; colIndex <10; colIndex++)
                        {
                            data1[colIndex] =temp[colIndex];// rowStr;
                        }
                        //截取下一行数据

                        if (tabControl1.SelectedIndex == 0)
                        {
                            DataRow dr = dt.NewRow();
                            int index1;
                            index1 = dataGridView1.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

                            dr.ItemArray = data1;
                            dt.Rows.InsertAt(dr, index1 + 1);
                        }

                       // Clipboard.Clear();
                   // return;
                }
                catch
                {
                  //  return ;
                }
            }
       }

        private void dataGridView3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.V)
            {
                try
                {
                    string pasteText = Clipboard.GetText();

                    if (string.IsNullOrEmpty(pasteText))
                        return;

                    Object[] data1 = new object[9];

                    String rowStr;

                    string[] temp = pasteText.Split('\t');

                    for (int colIndex = 0; colIndex < 8; colIndex++)
                    {
                        data1[colIndex] = temp[colIndex];// rowStr;
                    }

                    data1[8] = "";
                    //截取下一行数据

                    if (tabControl1.SelectedIndex == 1)
                    {
                        DataRow dr = resultTable.NewRow();
                        int index1;
                        index1 = dataGridView3.SelectedRows[0].Index;// dataGridView1.CurrentRow.Index;

                        dr.ItemArray = data1;
                        resultTable.Rows.InsertAt(dr, index1 + 1);
                    }

                    // Clipboard.Clear();
                    // return;
                }
                catch
                {
                    //  return ;
                }
            }
        }

    }
}
