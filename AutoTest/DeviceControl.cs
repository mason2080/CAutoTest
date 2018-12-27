using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Driver.File.Sql;
using System.Reflection;
using System.IO;

namespace AutoTest
{
    public partial class DeviceControl : Form
    {

        object[] classObject = new object[100];
        Type[] typeClass = new Type[100];
        List<string> typeClassName = new List<string>();
        DataTable deviceTable;

        public DeviceControl(string deviceSetting,Object [] objectArray)
        {
            InitializeComponent();

            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;

            classObject = objectArray;
            SQLiteClass sqlite = new SQLiteClass();
            deviceTable = sqlite.ReadWholeTable(Directory.GetCurrentDirectory() + "\\AutoTest.db", deviceSetting);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            this.ControlBox = false;

            btnDisconnect.Enabled = false;
            btnConnect.Enabled = true;

            string[] param = new string[20];
            try
            {
                for (int i = 0; i < deviceTable.Rows.Count; i++)//deviceTable.Rows.Count
                {
                    if ((deviceTable.Rows[i][4] != null) && (deviceTable.Rows[i][4] != ""))
                    {
                        param = deviceTable.Rows[i][4].ToString().Split('&');
                        string assmblyFilePath = Directory.GetCurrentDirectory() + "\\Driver\\" + param[0];
                        Assembly assmeblyObject = Assembly.LoadFile(assmblyFilePath);
                        typeClass[i] = assmeblyObject.GetType(param[1]);
                        typeClassName.Add(deviceTable.Rows[i][1].ToString());

                        //classObject[i] = assmeblyObject.CreateInstance(param[1]);

                        MethodInfo testMethod = typeClass[i].GetMethod("Disconnect_Device");
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
            catch { }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.ControlBox = true;

            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;


            string[] param = new string[20];
            try
            {
                for (int i = 0; i < deviceTable.Rows.Count; i++)//deviceTable.Rows.Count
                {
                    if ((deviceTable.Rows[i][4] != null) && (deviceTable.Rows[i][4] != ""))
                    {
                        param = deviceTable.Rows[i][4].ToString().Split('&');
                        string assmblyFilePath = Directory.GetCurrentDirectory() + "\\Driver\\" + param[0];
                        Assembly assmeblyObject = Assembly.LoadFile(assmblyFilePath);
                        typeClass[i] = assmeblyObject.GetType(param[1]);
                        typeClassName.Add(deviceTable.Rows[i][1].ToString());

                        //classObject[i] = assmeblyObject.CreateInstance(param[1]);

                        MethodInfo testMethod = typeClass[i].GetMethod("Connect_Device");
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
            catch { }
        }


    }
}
