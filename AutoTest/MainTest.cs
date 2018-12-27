using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Driver.Port.Can;
using System.IO;
using Driver.File.Ini;
using System.Threading;
using System.Runtime.InteropServices;
using Driver.File.Sql;
using System.Reflection;

namespace AutoTest
{
    public partial class MainTest : Form
    {
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        IniFileClass iniFileClass = new IniFileClass();
        Thread MainTestThread;
        Thread calTimeThread;
        DataTable dt1 = new DataTable();
        DataTable dt2 = new DataTable();
        bool testingFlag = false;
        enum Relay 
        {
            Pos=1,
            Neg=2,
            Pre=3,
            Heat=4,
            Chg=5,
            Fan=6,
            Do1=7,
            Do2=8
        }
        Object lockVoltObject = new Object();
        DateTime dateTimeStart = new DateTime();
        DataTable tempDataTable = new DataTable();
        DataTable deviceTable=new DataTable();

        string histroyTableName;

        object[] classObject = new object[100];
        Type[] typeClass = new Type[100];
        List<string> typeClassName = new List<string>();

        string barCode;
        string DeviceSetting;

        int testTimes = 1000;
        int testTimesNow = 0;

        public MainTest()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            textBox1.Text = ProjectGlobal.ProjectName;
            textBox1.ReadOnly = true;
            DeviceSetting = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "当前设备配置")+"_DeviceSetting";
            this.skinEngine1.SkinFile = Application.StartupPath + ProjectGlobal.SkinPath;
            ProjectGlobal.Title = iniFileClass.IniReadValue(Application.StartupPath + "\\Config.ini", "配置", "公司名称");
            toolStripStatusLabel1.Text = ProjectGlobal.Title;
            checkBox1.Checked = true;
        }

        private void ReadTestConfig()
        {
            DataTable dt = new DataTable() ;
            SQLiteClass sqlite = new SQLiteClass();
            string testItem;
            try
            {
                //dataGridTestItem.Rows.Clear();
                dt = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", textBox1.Text + "_TestProject");
                dataGridTestItem.DataSource = dt;
                dataGridTestItem.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridTestItem.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            catch { }

            dt2.Clear();
            dt1.Clear();

            for (int i = 0; i < dataGridTestItem.RowCount; i++)
            {
                testItem = dataGridTestItem.Rows[i].Cells[0].Value.ToString();
                if ((testItem != "") && (dataGridTestItem.Rows[i].Cells[1].Value.ToString().ToLower() == "true"))
                {
                    try
                    {
                       
                        dt2 = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItem + "_TestItem");
                        dt1.Merge(dt2);
                    }
                    catch { }
                }
            }

            dataGridTestStep.DataSource = dt1;

            for (int i = 0; i < dataGridTestStep.ColumnCount; i++)
            {
                dataGridTestStep.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            tempDataTable.Clear();
            dt2.Clear();
            for (int i = 0; i < dataGridTestItem.RowCount; i++)
            {
                testItem = dataGridTestItem.Rows[i].Cells[0].Value.ToString();
                if ((testItem != "") && (dataGridTestItem.Rows[i].Cells[1].Value.ToString().ToLower() == "true"))
                {
                    try
                    {
                        sqlite.ClearVariableTableValueColumn(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItem + "_Variable");
                        //dt2 = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItem + "_Variable");
                        dt2 = sqlite.ReadVariableTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", testItem + "_Variable");
                        tempDataTable.Merge(dt2);
                    }
                    catch { }
                }
            }
            dataGridViewVariable.ColumnHeadersVisible = true;
            dataGridViewVariable.RowHeadersVisible = false;
            dataGridViewVariable.DataSource = tempDataTable;
            dataGridViewVariable.Columns[0].Visible = false;
            dataGridViewVariable.Columns[2].Visible = false;
            dataGridViewVariable.Columns[7].Visible = false;
            dataGridViewVariable.RowHeadersVisible = true;
            for (int i = 0; i < dataGridViewVariable.ColumnCount; i++)
            {
                dataGridViewVariable.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridViewVariable.Columns[i].ReadOnly = true;
            }
            string tempTime = System.DateTime.Now.ToShortDateString().Replace("/", "");
            tempTime.Replace(@"\", "");
            histroyTableName = "H" + tempTime;
            if (sqlite.CheckTableExistStatus((Directory.GetCurrentDirectory() + "\\AutoTestResult.db"), histroyTableName) == 0)
            {
                sqlite.CreateNewHistoryTable((Directory.GetCurrentDirectory() + "\\AutoTestResult.db"), histroyTableName);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textPass.Text = "0";
            textFail.Text = "0";
            textPassRate.Text = "";
            textSum.Text = "0";
            this.Text = "测试项目:" + ProjectGlobal.ProjectName;
            tabControl2.SelectedIndex = 2;
            MenuItemStart.Enabled = true;
            MenuItemStop.Enabled = false;
            toolStripResume.Enabled = false;
            MenuItemPause.Enabled = false;
            InitClass();
            ReadTestConfig();
        }

        private void CloseDevice()
        {
            try
            {
                for (int i = 0; i < deviceTable.Rows.Count; i++)//deviceTable.Rows.Count
                {
                    if ((deviceTable.Rows[i][4] != null) && (deviceTable.Rows[i][4] != ""))
                    {
                        MethodInfo testMethod = typeClass[i].GetMethod("Close_Device");
                        ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                        int length = paramsInfo.Length;
                        string[] cmdParameter = deviceTable.Rows[i][3].ToString().Split(',');
                        object[] obj = new object[length];
                        for (int j = 0; j < length; j++)
                        {
                            Type tType = paramsInfo[j].ParameterType;
                            //如果它是值类型,或者String   
                            if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                            {
                                //改变参数类型   
                                obj[j] = Convert.ChangeType(cmdParameter[j], tType);
                            }
                            else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                            {
                                //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                            }
                        }
                        ////执行方法   
                        object returnValue = testMethod.Invoke(classObject[i], obj);
                    }
                }
            }
            catch 
            {

            }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
 
        }

        private void CalTimeElapsed()
        {
            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    TimeSpan ts= DateTime.Now - dateTimeStart;

                    if (textTimeElapse.InvokeRequired)
                    {
                        Action actonDelegate = delegate()
                        {
                            this.textTimeElapse.Text = ts.TotalSeconds.ToString("0");
                        };

                        this.textTimeElapse.Invoke(actonDelegate);
                    }
                    else 
                    {
                        textTimeElapse.Text = ts.TotalSeconds.ToString("0");
                    }
                }
                catch
                {
                    //MessageBox.Show("26");
                }
            }
        }

        private void InitClass()
        {
            SQLiteClass sqlite = new SQLiteClass();
            string[] param = new string[20];
            try
            {
                deviceTable = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", DeviceSetting);
                for (int i = 0; i < deviceTable.Rows.Count; i++)//deviceTable.Rows.Count
                {  
                    if ((deviceTable.Rows[i][4] != null) && (deviceTable.Rows[i][4] != ""))
                    {
                        param =deviceTable.Rows[i][4].ToString().Split('&');
                        string assmblyFilePath = Directory.GetCurrentDirectory() + "\\Driver\\" + param[0];
                        Assembly assmeblyObject = Assembly.LoadFile(assmblyFilePath);
                        typeClass[i] = assmeblyObject.GetType(param[1]);
                        typeClassName.Add(deviceTable.Rows[i][1].ToString().Trim());
                        classObject[i] = assmeblyObject.CreateInstance(param[1]);
                        MethodInfo testMethod = typeClass[i].GetMethod("Init_Class");
                        ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                        int length = paramsInfo.Length;
                        string[] cmdParameter =deviceTable.Rows[i][3].ToString().Split(',');
                        object[] obj = new object[length];
                        for (int j = 0; j < length; j++)
                        {
                            Type tType = paramsInfo[j].ParameterType;
                            //如果它是值类型,或者String   
                            if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                            {
                                //改变参数类型   
                                obj[j] = Convert.ChangeType(cmdParameter[j], tType);
                            }
                            else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                            {
                                //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                            }
                        }
                        ////执行方法   
                        object returnValue = testMethod.Invoke(classObject[i], obj);

                        /**********************************初始化设备连接********************************/
                        testMethod = typeClass[i].GetMethod("Init_Device");
                        paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                        length = paramsInfo.Length;

                        cmdParameter = deviceTable.Rows[i][3].ToString().Split(',');
                        obj = new object[length];
                        for (int j = 0; j < length; j++)
                        {
                            Type tType = paramsInfo[j].ParameterType;
                            //如果它是值类型,或者String   
                            if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                            {
                                //改变参数类型   
                                obj[j] = Convert.ChangeType(cmdParameter[j], tType);
                            }
                            else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                            {
                                //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                            }
                        }
                        ////执行方法   
                       returnValue = testMethod.Invoke(classObject[i], obj);
                    }
                }
            }
            catch { }
        }

        /***************************************************************************/
        private void MainTestFunction()
        {
                int count = dataGridTestStep.Rows.Count;
                int index = 0;
                int delayMs = 0;
                bool finalResult = true;
                int retryTimes = 0;
                string[] param = new string[20];
                SQLiteClass sqlite = new SQLiteClass();
                testingFlag = true;
                try
                {
                    if (textTimeElapse.InvokeRequired)
                    {
                        Action actonDelegate = delegate()
                        {
                            this.textTimeElapse.Text ="0";
                        };

                        this.textTimeElapse.Invoke(actonDelegate);
                    }
                    else
                    {
                        textTimeElapse.Text = "0";
                    }
                }
                catch
                {
                    //MessageBox.Show("-0");
                }
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        timerElapse.Enabled = true;
                        progressBar1.Value = i;
                        if (i > 0)
                        {
                            dataGridTestStep.Rows[i - 1].Selected = false;
                            dataGridTestStep.Rows[i].Selected = true;
                        }
                    }
                    catch
                    {
                        //MessageBox.Show("1");
                    }


                    if (i > 10)//实现自动滚动显示
                    {
                        try
                        {
                            dataGridTestStep.FirstDisplayedScrollingRowIndex = i - 10;
                        }
                        catch
                        {
                            //MessageBox.Show("2");
                        }
                    }
                    if ((bool)(dataGridTestStep.Rows[i].Cells[0].Value) == true)//Enable Status
                    {
                        retryTimes = int.Parse(dataGridTestStep.Rows[i].Cells[6].Value.ToString());
                        try
                        {
                            if (textStatus.InvokeRequired)
                            {
                                Action actonDelegate = delegate()
                                {
                                    this.textStatus.Text = dataGridTestStep.Rows[i].Cells[4].Value.ToString();
                                };

                                this.textStatus.Invoke(actonDelegate);
                            }
                            else
                            {
                                textStatus.Text = dataGridTestStep.Rows[i].Cells[4].Value.ToString();
                            }
                        }
                        catch {
                            //MessageBox.Show("3");
                        }

                        for (int t = 0; t < retryTimes + 1; t++)//Fail Retry times
                        {
                            param = dataGridTestStep.Rows[i].Cells[9].Value.ToString().Split('&');
                            string[] param2 = param[3].Split(',');
                            index = typeClassName.IndexOf(dataGridTestStep.Rows[i].Cells[3].Value.ToString().Trim());
                            MethodInfo testMethod = typeClass[index].GetMethod(param[2]);
                            ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                            int length = paramsInfo.Length;
                            string[] cmdParameter = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                            string[] cmdParameter1 = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                            int valueExist;
                            object[] obj = new object[length];
                            object returnValue = new object();

                            try
                            {
                                for (int j = 0; j < length; j++)//获取变量对应数据
                                {
                                    valueExist = sqlite.CheckVariableValueExist(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                                    if (valueExist > 0)
                                    {
                                        cmdParameter1[j] = sqlite.ReadVariableValue(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                                    }
                                    else 
                                    {
                                        if (cmdParameter[j].IndexOf("全局.") >= 0)
                                        {
                                            for (int nn = 0; nn < dataGridViewVariable.Rows.Count; nn++)
                                            {
                                                if (dataGridViewVariable.Rows[nn].Cells[4].Value.ToString() == cmdParameter[j].Replace("全局.", ""))
                                                {
                                                    cmdParameter1[j] = dataGridViewVariable.Rows[nn].Cells[8].Value.ToString();
                                                    break;
                                                }

                                            }
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                //MessageBox.Show("4");
                            }

                                for (int j = 0; j < length; j++)
                                {
                                    Type tType = paramsInfo[j].ParameterType;
                                    //如果它是值类型,或者String   
                                    if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                                    {
                                        //改变参数类型   
                                        obj[j] = Convert.ChangeType(cmdParameter1[j], tType);
                                    }
                                    else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                                    {
                                        //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                                    }
                                }
                                ////执行方法   
                            returnValue = testMethod.Invoke(classObject[index], obj);
                            double min, max;
                            int startIndex = 0;
                            if (param[2].IndexOf("_ARRAY") >= 0)//数组输出特殊处理
                            {
                                for (int l = 0; l < double.Parse(cmdParameter1[0]); l++)
                                {
                                    string[] temp = cmdParameter[1].Split('_');
                                    string variableName = "Array_" + temp[1] + "_" + (l + 1).ToString();
                                    string[] arrayValue = obj[1].ToString().Split('&');
                                    //数组暂时不更新数据库
                                    //sqlite.UpdateVariableTableValueArray(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", variableName, arrayValue[l]);

                                    for (int k = startIndex; k < dataGridViewVariable.Rows.Count - 1; k++)
                                    {

                                        if ((dataGridViewVariable.Rows[k].Cells[4].Value != null) && (dataGridViewVariable.Rows[k].Cells[4].Value != ""))
                                        {
                                            if (dataGridViewVariable.Rows[k].Cells[4].Value.ToString() == variableName)
                                            {
                                                startIndex = k;
                                                dataGridViewVariable.Rows[k].Cells[8].Value = arrayValue[l];
                                                if (k > 10)
                                                {
                                                    try
                                                    {
                                                        dataGridViewVariable.FirstDisplayedScrollingRowIndex = k - 10;
                                                    }
                                                    catch
                                                    {
                                                        //MessageBox.Show("5");
                                                    }
                                                }

                                                if ((bool)dataGridViewVariable.Rows[k].Cells[1].Value == true)//需要判定
                                                {
                                                    if ((bool)dataGridViewVariable.Rows[k].Cells[2].Value == true)//是否是数字类型
                                                    {
                                                        try
                                                        {
                                                            min = double.Parse(dataGridViewVariable.Rows[k].Cells[5].Value.ToString());
                                                            max = double.Parse(dataGridViewVariable.Rows[k].Cells[6].Value.ToString());
                                                            if ((double.Parse(arrayValue[l]) >= min) && (double.Parse(arrayValue[l]) <= max))
                                                            {
                                                                dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                                dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.Red;
                                                                dataGridViewVariable.Rows[k].Cells[9].Value = "FAIL";
                                                                finalResult = false;
                                                                break;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            //MessageBox.Show("6");
                                                        }
                                                    }
                                                    else //不是数值就当字符串判断
                                                    {
                                                        try
                                                        {

                                                            if ((dataGridViewVariable.Rows[k].Cells[5].Value.ToString() == arrayValue[l]) || (dataGridViewVariable.Rows[k].Cells[6].Value.ToString() == arrayValue[l]))
                                                            {
                                                                dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                                dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.Red;
                                                                dataGridViewVariable.Rows[k].Cells[9].Value = "FAIL";
                                                                finalResult = false;
                                                                break;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            //MessageBox.Show("7");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                        dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                    }
                                                    catch
                                                    {
                                                        //MessageBox.Show("8");
                                                    }

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                //  break;
                            }//End of 数组输出处理
                            else
                            {
                                for (int j = 0; j < length; j++) //更新结果至变量
                                {
                                    if (param2[j].IndexOf(".Out") >= 0)//单个输出情况处理
                                    {
                                        sqlite.UpdateVariableTableValueColumn(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j], obj[j].ToString());

                                        for (int k = 0; k < dataGridViewVariable.Rows.Count - 1; k++)
                                        {
                                            if (dataGridViewVariable.Rows[k].Cells[4].Value != null)
                                            {
                                                if (dataGridViewVariable.Rows[k].Cells[4].Value.ToString() == cmdParameter[j])
                                                {
                                                    dataGridViewVariable.Rows[k].Cells[8].Value = obj[j].ToString();

                                                    if (k > 10)
                                                    {
                                                        try
                                                        {
                                                            dataGridViewVariable.FirstDisplayedScrollingRowIndex = k - 10;
                                                        }
                                                        catch
                                                        {
                                                            //MessageBox.Show("9");
                                                        }
                                                    }

                                                    if ((bool)dataGridViewVariable.Rows[k].Cells[1].Value == true)//需要判定
                                                    {
                                                        if ((bool)dataGridViewVariable.Rows[k].Cells[2].Value == true)//是否是数字类型
                                                        {
                                                            try
                                                            {
                                                                min = double.Parse(dataGridViewVariable.Rows[k].Cells[5].Value.ToString());
                                                                max = double.Parse(dataGridViewVariable.Rows[k].Cells[6].Value.ToString());
                                                                if ((double.Parse(obj[j].ToString()) >= min) && (double.Parse(obj[j].ToString()) <= max))
                                                                {
                                                                    dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                                    dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                                }
                                                                else
                                                                {
                                                                    dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.Red;
                                                                    dataGridViewVariable.Rows[k].Cells[9].Value = "FAIL";
                                                                    finalResult = false;
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                //MessageBox.Show("10");
                                                            }
                                                        }
                                                        else //不是数值就当字符串判断
                                                        {
                                                            try
                                                            {
                                                                if ((dataGridViewVariable.Rows[k].Cells[5].Value.ToString() == obj[j].ToString()) || (dataGridViewVariable.Rows[k].Cells[6].Value.ToString() == obj[j].ToString()))
                                                                {
                                                                    dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                                    dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                                }
                                                                else
                                                                {
                                                                    dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.Red;
                                                                    dataGridViewVariable.Rows[k].Cells[9].Value = "FAIL";
                                                                    finalResult = false;
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                //MessageBox.Show("11");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            dataGridViewVariable.Rows[k].DefaultCellStyle.BackColor = Color.PaleGreen;
                                                            dataGridViewVariable.Rows[k].Cells[9].Value = "PASS";
                                                        }
                                                        catch
                                                        {
                                                            //MessageBox.Show("12");
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }//End off 单个处理
                                }
                            }
                            try
                            {

                                if (dataGridTestStep.Rows[i].Cells[5].Value != null)
                                {
                                    if (dataGridTestStep.Rows[i].Cells[5].Value.ToString() != "")
                                    {
                                        delayMs = int.Parse(dataGridTestStep.Rows[i].Cells[5].Value.ToString());
                                    }
                                    else
                                    {
                                        delayMs = 1;
                                    }
                                }
                            }
                            catch
                            {
                                //MessageBox.Show("13");
                            }


                            if (returnValue == null)
                            {
                                try
                                {
                                    dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                }
                                catch
                                {
                                    //MessageBox.Show("14");
                                }

                                if (delayMs < 100000)
                                {
                                    Thread.Sleep(delayMs);
                                }
                                else
                                {
                                    Thread.Sleep(2000);
                                }

                                break;
                            }
                            else
                            {
                                if (returnValue.ToString() == "1")
                                {
                                    try
                                    {
                                        dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.PaleGreen;
                                    }
                                    catch
                                    {
                                        //MessageBox.Show("15");
                                    }

                                    if (delayMs < 100000)
                                    {
                                        Thread.Sleep(delayMs);
                                    }
                                    else
                                    {
                                        Thread.Sleep(10);
                                    }

                                    break;
                                }
                                else
                                {
                                    try
                                    {
                                        dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                                    }
                                    catch
                                    {
                                        //MessageBox.Show("16");
                                    }

                                    Thread.Sleep(delayMs);
                                }
                            }

                        }
                    }
                }

               int sum=0;
               string testTime="";

                try
                {
                    Thread.Sleep(10);
                    MenuItemStart.Enabled = true;
                    MenuItemStop.Enabled = false;
                    toolStripResume.Enabled = false;
                    MenuItemPause.Enabled = false;
                    try
                    {
                        sum = int.Parse(textSum.Text) + 1;
                        textSum.Text = sum.ToString();
                    }
                    catch
                    {
                        sum = 1;
                        textPass.Text = "0";
                        textFail.Text = "0";
                        textSum.Text = sum.ToString();                                            
                    }
                    testTime = System.DateTime.Now.ToShortDateString().Replace("/", "-");
                    testTime.Replace(@"\", "-");
                    testTime = testTime + " " + System.DateTime.Now.Hour.ToString() + ":" + System.DateTime.Now.Minute.ToString() + ":" + System.DateTime.Now.Second.ToString();
                }
                catch
                {
                    //MessageBox.Show("18");
                }

                if (finalResult == true)
                {
                    try
                    {
                        try
                        {
                            int pass = int.Parse(textPass.Text) + 1;
                            textPass.Text = pass.ToString();

                            calTimeThread.Abort();
                            textPassRate.Text = ((double)pass / (double)sum * 100).ToString();
                        }
                        catch
                        {
                            //MessageBox.Show("19");
                        }

                        if (textStatus.InvokeRequired)
                        {
                            Action actonDelegate = delegate()
                            {
                                this.textStatus.Text = "测试通过";
                                this.textStatus.BackColor = Color.PaleGreen;
                            };
                            this.textStatus.Invoke(actonDelegate);
                            this.textSN.Text += barCode + ":PASS\r\n";
                        }
                        else
                        {
                            textStatus.Text = "测试通过";
                            textStatus.BackColor = Color.PaleGreen;
                            textSN.Text += barCode + ":PASS\r\n";
                        }
                        dataGridViewVariable.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        sqlite.SaveOneHistoryData((Directory.GetCurrentDirectory() + "\\AutoTestResult.db"), histroyTableName, barCode, testTime, "通过", ProjectGlobal.ProjectName, "BMS", tempDataTable);
                    }
                    catch
                    {
                        //MessageBox.Show("20");
                    }
                }
                else
                {
                    try
                    {
                        try
                        {
                            int fail = int.Parse(textFail.Text) + 1;
                            textFail.Text = fail.ToString();
                            int pass = int.Parse(textPass.Text);
                            textPassRate.Text = ((double)pass / (double)sum * 100).ToString();
                        }
                        catch
                        {
                            //MessageBox.Show("21");
                        }

                        calTimeThread.Abort();

                        if (textStatus.InvokeRequired)
                        {
                            Action actonDelegate = delegate()
                            {
                                this.textStatus.Text = "测试失败";
                                this.textStatus.BackColor = Color.Red;
                                this.textSN.Text += barCode + ":FAIL\r\n";
                            };

                            this.textStatus.Invoke(actonDelegate);
                        }
                        else
                        {
                            textStatus.Text = "测试失败";
                            textStatus.BackColor = Color.Red;

                            textSN.Text += barCode + ":FAIL\r\n";
                        }
                        dataGridViewVariable.CommitEdit(DataGridViewDataErrorContexts.Commit);
                        sqlite.SaveOneHistoryData((Directory.GetCurrentDirectory() + "\\AutoTestResult.db"), histroyTableName, barCode, testTime, "失败", ProjectGlobal.ProjectName, "BMS", tempDataTable);
                    }
                    catch
                    {
                        //MessageBox.Show("22");
                    }
                }


                try
                {

                    if (textBarCode.InvokeRequired)
                    {
                        Action actonDelegate = delegate()
                        {
                            this.textBarCode.ReadOnly = false;
                            this.textBarCode.Text = "";
                        };

                        this.textBarCode.Invoke(actonDelegate);
                    }
                    else
                    {
                        textBarCode.ReadOnly = false;
                        textBarCode.Text = "";
                    }
                }
                catch
                {
                    //MessageBox.Show("23");
                }
                testTimesNow++;
                testingFlag = false;
                dataGridViewVariable.Enabled = true;
                if (testTimesNow < testTimes)
                {
                    MenuItemStart.PerformClick();
                }
                else
                {
                    testTimesNow = 0;
                }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            MenuItemStart.PerformClick();
        }
    
        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter) && (textBarCode.Text != "") && (textBarCode.ReadOnly == false))
            {
                barCode = textBarCode.Text;
                textBarCode.ReadOnly = true;
                textStatus.Text = "开始测试";
                textStatus.BackColor = Color.Aquamarine;
                progressBar1.Maximum = dataGridTestStep.Rows.Count - 1;
                for (int i = 0; i < dataGridTestStep.Rows.Count; i++)
                {
                    dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }
                for (int i = 0; i < dataGridViewVariable.Rows.Count; i++)
                {
                    dataGridViewVariable.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    dataGridViewVariable.Rows[i].Cells[7].Value = "";
                    dataGridViewVariable.Rows[i].Cells[8].Value = "";
                }
                MainTestThread = new Thread(MainTestFunction);
                MainTestThread.Start();
                MenuItemStart.Enabled = false;
                MenuItemStop.Enabled = true;
                toolStripResume.Enabled = false;
                MenuItemPause.Enabled = true;
            }
        }

        private void dataGridTestStep_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = 0;
            string[] param = new string[20];
            SQLiteClass sqlite = new SQLiteClass();
            int i = dataGridTestStep.CurrentRow.Index;
            if (Control.ModifierKeys == Keys.Alt)
            {
                for (int j = 0; j < dataGridTestStep.Rows.Count; j++)
                {
                    dataGridTestStep.Rows[j].DefaultCellStyle.BackColor = Color.White;
                }
            }

            if ((Control.ModifierKeys == Keys.Control) && (i >= 0))
            {//do something}
                if ((ProjectGlobal.UserName == "管理员") && ((bool)(dataGridTestStep.Rows[i].Cells[0].Value) == true))
                {
                    if ((bool)(dataGridTestStep.Rows[i].Cells[0].Value) == true)//Enable Status
                    {
                        dataGridTestStep.CurrentRow.DefaultCellStyle.BackColor = Color.LightGreen;
                        param = dataGridTestStep.Rows[i].Cells[9].Value.ToString().Split('&');
                        string[] param2 = param[3].Split(',');

                        index = typeClassName.IndexOf(dataGridTestStep.Rows[i].Cells[3].Value.ToString());
                        MethodInfo testMethod = typeClass[index].GetMethod(param[2]);
                        ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                        int length = paramsInfo.Length;

                        string[] cmdParameter = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                        string[] cmdParameter1 = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                        int valueExist;

                        for (int j = 0; j < length; j++)//获取变量对应数据
                        {
                            valueExist = sqlite.CheckVariableValueExist(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                            if (valueExist > 0)
                            {
                                cmdParameter1[j] = sqlite.ReadVariableValue(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                            }
                            else
                            {
                                if (cmdParameter[j].IndexOf("全局.") >= 0)
                                {
                                    for (int nn = 0; nn < dataGridViewVariable.Rows.Count; nn++)
                                    {
                                        if (dataGridViewVariable.Rows[nn].Cells[4].Value.ToString() == cmdParameter[j].Replace("全局.", ""))
                                        {
                                            cmdParameter1[j] = dataGridViewVariable.Rows[nn].Cells[8].Value.ToString();
                                            break;
                                        }

                                    }
                                }
                            }
                        }

                        object[] obj = new object[length];

                        for (int j = 0; j < length; j++)
                        {
                            Type tType = paramsInfo[j].ParameterType;
                            //如果它是值类型,或者String   
                            if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                            {
                                //改变参数类型   
                                obj[j] = Convert.ChangeType(cmdParameter1[j], tType);
                            }
                            else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                            {
                                //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                            }
                        }
                        ////执行方法   
                        object returnValue = testMethod.Invoke(classObject[index], obj);
                    }
                }
            }
        }
        /****************************************菜单处理********************************/
        private void toolStripMenuConfig_Click(object sender, EventArgs e)
        {
            LoopTest form = new LoopTest(testTimes);
            form.ShowDialog();
            testTimes = form.testTimes;
        }

        private void 设备连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceControl form = new DeviceControl(DeviceSetting, classObject);
            form.ShowDialog();
        }

        private void 进入测试项目编辑ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProjectGlobal.UserName == "操作员")
            {
                MessageBox.Show("没有此权限，请用管理员登录");
                return;
            }
            if (testingFlag ==false)
            {
                try
                {
                    if (MessageBox.Show("确定进入:" + dataGridTestStep.SelectedRows[0].Cells[4].Value.ToString() + " 测试项目编辑", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        this.Hide();
                        StepEdit form = new StepEdit(dataGridTestStep.SelectedRows[0].Cells[4].Value.ToString() + "_TestItem");
                        form.ShowDialog();
                        ReadTestConfig();
                        this.Show();
                    }
                }
                catch { }
            }


        }

        private void MenuItemStop_Click(object sender, EventArgs e)
        {
            testingFlag = false;
            //timerElapse.Enabled = false;
            testTimesNow = 0;
            try
            {
                if ((MainTestThread.ThreadState != ThreadState.Unstarted))
                {
                    if ((MainTestThread.ThreadState == ThreadState.Suspended))
                    {
                        MainTestThread.Resume();
                    }
                    MainTestThread.Abort();
                    calTimeThread.Abort();
                    MenuItemStart.Enabled = true;
                    MenuItemStop.Enabled = false;
                    toolStripResume.Enabled = false;
                    MenuItemPause.Enabled = false;
                    textStatus.Text = "测试中止";
                    textBarCode.ReadOnly = false;
                    textBarCode.Text = "";
                    textStatus.BackColor = Color.Coral;
                    dataGridViewVariable.Enabled = true;

                }
            }
            catch { }
        }

        private void MenuItemPause_Click(object sender, EventArgs e)
        {
            //timerElapse.Enabled = false;
            try
            {
                if ((MainTestThread.ThreadState != ThreadState.Unstarted))
                {
                    MainTestThread.Suspend();
                    toolStripResume.Enabled = true;
                    MenuItemPause.Enabled = false;
                    textStatus.Text = "暂停测试";
                    textStatus.BackColor = Color.Yellow;
                    dataGridViewVariable.Enabled = true ;
                }

            }
            catch { }
        }

        private void toolStripResume_Click(object sender, EventArgs e)
        {
            timerElapse.Enabled = true;

            dataGridViewVariable.Enabled = false;
            try
            {
                if ((MainTestThread.ThreadState == ThreadState.Suspended))
                {
                    MainTestThread.Resume();
                    toolStripResume.Enabled = false;
                    MenuItemPause.Enabled = true;
                    textStatus.Text = "继续测试";
                    textStatus.BackColor = Color.Aquamarine;
                }
            }
            catch 
            {
            
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (ProjectGlobal.UserName == "操作员")
            {
                MessageBox.Show("没有此权限，请用管理员登录");
                return;
            }

            if (testingFlag == false)
            {
                if (MessageBox.Show("确定进入测试程序编辑", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                   // CloseDevice();
                    this.Hide();
                    ProjectEdit form = new ProjectEdit(textBox1.Text + "_TestProject");
                    form.ShowDialog();
                    ReadTestConfig();
                    this.Show();
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (ProjectGlobal.UserName == "操作员")
            {
                MessageBox.Show("没有此权限，请用管理员登录");
                return;
            }

            try
            {
                if (testingFlag == false)
                {
                    if (tabControl2.SelectedIndex == 0)
                    {
                        if (dataGridTestItem.SelectedRows[0].Cells[0].Value.ToString() != "")
                        {
                            if (MessageBox.Show("确定进入:" + dataGridTestItem.SelectedRows[0].Cells[0].Value.ToString() + " 测试项目编辑", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                // CloseDevice();
                                this.Hide();
                                StepEdit form = new StepEdit(dataGridTestItem.SelectedRows[0].Cells[0].Value.ToString() + "_TestItem");
                                form.ShowDialog();
                                ReadTestConfig();
                                this.Show();
                            }
                        }
                    }
                    else if (tabControl2.SelectedIndex == 2)
                    {
                        if (dataGridViewVariable.SelectedRows[0].Cells[3].Value.ToString() != "")
                        {
                            if (MessageBox.Show("确定进入:" + dataGridViewVariable.SelectedRows[0].Cells[3].Value.ToString() + " 测试项目编辑", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                //  CloseDevice();
                                this.Hide();
                                StepEdit form = new StepEdit(dataGridViewVariable.SelectedRows[0].Cells[3].Value.ToString() + "_TestItem");
                                form.ShowDialog();
                                ReadTestConfig();
                                this.Show();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void contextMenuStrip4_Opening(object sender, CancelEventArgs e)
        {

        }

        private void MenuItemStart_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                InputSN form = new InputSN("请输入产品条码：");

                if (form.ShowDialog() == DialogResult.OK)
                {
                    textBarCode.Text = form.msg;
                }
                else
                {
                    return;
                }
            }

            try
            {
                if (textBarCode.Text == "")
                {
                    textBarCode.Text = System.DateTime.Now.ToShortDateString().Replace("/", "") + System.DateTime.Now.ToLongTimeString().Replace(":", "");
                    textBarCode.Text = textBarCode.Text.Replace(@"\", "");
                }
                barCode = textBarCode.Text;
                textStatus.Text = "开始测试";
                dataGridViewVariable.FirstDisplayedScrollingRowIndex = 0;
                dataGridViewVariable.Enabled = false;
            }
            catch
            {
                //MessageBox.Show("27");
            }

            try
            {
                dateTimeStart = DateTime.Now;
                textStatus.BackColor = Color.Aquamarine;
                progressBar1.Maximum = dataGridTestStep.Rows.Count - 1;
                for (int i = 0; i < dataGridTestStep.Rows.Count; i++)
                {
                    dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.White;
                }
                for (int i = 0; i < dataGridViewVariable.Rows.Count; i++)
                {
                    dataGridViewVariable.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    dataGridViewVariable.Rows[i].Cells[7].Value = "";
                    dataGridViewVariable.Rows[i].Cells[8].Value = "";
                    dataGridViewVariable.Rows[i].Cells[9].Value = "";
                }
                btnStart.Focus();
            }
            catch
            {
                //MessageBox.Show("28");
            }

            MainTestThread = new Thread(MainTestFunction);
            MainTestThread.Start();
            calTimeThread = new Thread(CalTimeElapsed);
            calTimeThread.Start();
            MenuItemStart.Enabled = false;
            MenuItemStop.Enabled = true;
            toolStripResume.Enabled = false;
            MenuItemPause.Enabled = true;
        }

        private void 运行选中项目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = 0;
            string[] param = new string[20];
            SQLiteClass sqlite = new SQLiteClass();
            int i;
            if (ProjectGlobal.UserName == "操作员")
            {
                MessageBox.Show("没有此权限，请用管理员登录");
                return;
            }

            for (i = 0; i < dataGridTestStep.Rows.Count; i++)
            {
                dataGridTestStep.Rows[i].DefaultCellStyle.BackColor = Color.White;
            }

            for(int n=0;n<dataGridTestStep.SelectedRows.Count;n++)
            {//do something}
                i = dataGridTestStep.SelectedRows[n].Index;

                if ((ProjectGlobal.UserName == "管理员") && ((bool)(dataGridTestStep.Rows[i].Cells[0].Value) == true))
                {
                    if ((bool)(dataGridTestStep.Rows[i].Cells[0].Value) == true)//Enable Status
                    {
                        dataGridTestStep.SelectedRows[n].DefaultCellStyle.BackColor = Color.LightGreen;

                        param = dataGridTestStep.Rows[i].Cells[9].Value.ToString().Split('&');
                        string[] param2 = param[3].Split(',');

                        index = typeClassName.IndexOf(dataGridTestStep.Rows[i].Cells[3].Value.ToString());
                        MethodInfo testMethod = typeClass[index].GetMethod(param[2]);
                        ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
                        int length = paramsInfo.Length;

                        string[] cmdParameter = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                        string[] cmdParameter1 = dataGridTestStep.Rows[i].Cells[2].Value.ToString().Split(',');
                        int valueExist=0;

                        for (int j = 0; j < length; j++)//获取变量对应数据
                        {
                            valueExist = sqlite.CheckVariableValueExist(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                            if (valueExist > 0)
                            {
                                cmdParameter1[j] = sqlite.ReadVariableValue(Directory.GetCurrentDirectory() + "\\AutoTest.db", dataGridTestStep.Rows[i].Cells[4].Value.ToString() + "_Variable", cmdParameter[j]);
                            }
                            else
                            {
                                if (cmdParameter[j].IndexOf("全局.") >= 0)
                                {
                                    for (int nn = 0; nn < dataGridViewVariable.Rows.Count; nn++)
                                    {
                                        if (dataGridViewVariable.Rows[nn].Cells[4].Value.ToString() == cmdParameter[j].Replace("全局.",""))
                                        {
                                            cmdParameter1[j] = dataGridViewVariable.Rows[nn].Cells[8].Value.ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                        }


                        object[] obj = new object[length];

                        for (int j = 0; j < length; j++)
                        {
                            Type tType = paramsInfo[j].ParameterType;
                            //如果它是值类型,或者String   
                            if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                            {
                                //改变参数类型   
                                obj[j] = Convert.ChangeType(cmdParameter1[j], tType);
                            }
                            else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                            {
                                //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                            }
                        }

                        ////执行方法   
                        object returnValue = testMethod.Invoke(classObject[index], obj);
                    }
                }
            }
            MessageBox.Show("执行完成");
        }

        private void MainTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (MessageBox.Show("确定要退出测试程序", "注意", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CloseDevice();
                    if ((MainTestThread.ThreadState != ThreadState.Unstarted))
                    {
                        if ((MainTestThread.ThreadState == ThreadState.Suspended))
                        {
                            MainTestThread.Resume();
                        }
                        MainTestThread.Abort();
                        calTimeThread.Abort();
                        MenuItemStart.Enabled = true;
                        MenuItemStop.Enabled = false;
                        toolStripResume.Enabled = false;
                        MenuItemPause.Enabled = false;
                        textStatus.Text = "测试中止";
                        textBarCode.ReadOnly = false;
                        textBarCode.Text = "";
                        textStatus.BackColor = Color.Coral;
                    }
                }
                else 
                {
                    e.Cancel = true;
                }
            }
            catch { }
        }


    }
}