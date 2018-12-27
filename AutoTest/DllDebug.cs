using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using AutoTestAttribute;
using Driver.File.Sql;

namespace AutoTest
{
    public partial class DllDebug : Form
    {

      //private PropertyOperator propertyOperatorApp = new PropertyOperator();
        private static Assembly assmeblyObject;
        private static string selectPropertyName;
        private static string selectPropertyValue;
        //指定命名空间和类名
        //private static Type type;
        
        //public DllDebug(string assmblyPath, string selectName,string selectValue)
        //{
        //    string currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
        //    string assmblyFilePath = currentPath + assmblyPath;
        //    assmeblyObject = Assembly.LoadFile(assmblyFilePath);
        //    selectPropertyName = selectName;
        //    selectPropertyValue = selectValue;
        //}

        public DllDebug()
        {
            InitializeComponent();

        }

        private List<string> SearchMethodByName(string name)
        {

            //string currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            //string assmblyFilePath = currentPath + assmblyPath;
            string assmblyFilePath = @"F:\Bluewave New Energy\SVN_Work\BMS_MonitorC#\trunk -AutoTest\AutoTest\bin\Release\Driver.Port.Can.dll";
            assmeblyObject = Assembly.LoadFile(assmblyFilePath);

            Type[] typeClasses = assmeblyObject.GetTypes();
            List<string> MethodList = new List<string>();
            foreach (Type typeClass in typeClasses)
            {
                //Console.WriteLine("Class: " + typeClass.FullName);
                //Console.WriteLine("*****************************************************");

                textDebug.Text += "Class: " + typeClass.FullName + "\r\n";

                textDebug.Text += "*****************************************************"+"\r\n";

                object classObject = assmeblyObject.CreateInstance(typeClass.FullName);
                MethodInfo[] methods = typeClass.GetMethods();
                foreach (MethodInfo method in methods)
                {
                   // Console.WriteLine("Method: " + method);

                    //textDebug.Text += "Method: " + method.ToString()+ "\r\n";
                    object[] testPropertyAttributes = method.GetCustomAttributes(typeof(AutoTestAttribute.DescriptionAttribute), true);
                    foreach (object property in testPropertyAttributes)
                    {
                        AutoTestAttribute.DescriptionAttribute objProperty = (AutoTestAttribute.DescriptionAttribute)property;
                        //if (objProperty.Description.ToLower().Equals(name.ToLower()))
                        if (objProperty.Description!="")
                        {
                            MethodList.Add(typeClass.FullName +'.' + method.Name);
                            //Console.WriteLine(method.Name + " is Selected!");
                            textDebug.Text += method.Name + " is find!" + "\r\n";
                            break;
                        }
                    }
                    
                }
                Console.WriteLine(" "); 
            }
            Console.WriteLine("");
            return MethodList;
        }

        private void get_ClassName(string name,ref string nameSpace,ref string nameClass,ref string method)
        {
            string[] strSplitMethod = name.Split('.');

            int num = strSplitMethod.Length;

            method = strSplitMethod[num-1];
            nameClass = strSplitMethod[num - 2];

     
            for(int i=0;i<(num-2);i++)
            {
                if (i < (num - 3))
                {
                    nameSpace += strSplitMethod[i] + '.';
                }
                else
                {
                    nameSpace += strSplitMethod[i];
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nameSpaceName="" ;
            string className ="";
            string methodName="";

           
            //sqlite.CreateTable(@"F:\Bluewave New Energy\SVN_Work\BMS_MonitorC#\trunk -AutoTest\AutoTest\bin\Release\AutoTest.db","New");
          //  sqlite.InsertVariableData(@"F:\Bluewave New Energy\SVN_Work\BMS_MonitorC#\trunk -AutoTest\AutoTest\bin\Release\AutoTest.db", "Variable_Template");

            string name = "Can_SendOneFrame";
        
            List<string> selectedMethods = SearchMethodByName(name);
          
            get_ClassName(selectedMethods[0], ref nameSpaceName, ref className, ref methodName);

            Type typeClass = assmeblyObject.GetType(nameSpaceName + '.' + className);
            MethodInfo testMethod = typeClass.GetMethod(methodName);

            object classObject = assmeblyObject.CreateInstance(nameSpaceName + '.' + className);


            ParameterInfo[] paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
            int  length = paramsInfo.Length;

            string cmd = "21,500,0,0";
            string[] cmdParameter=cmd.Split(',');
            object[] obj = new object[length];  
 
            for (int i = 0; i < length; i++)   
            {   
                Type tType = paramsInfo[i].ParameterType;   
                 //如果它是值类型,或者String   
                if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))   
                {   
                 //改变参数类型   
                    obj[i] = Convert.ChangeType(cmdParameter[i], tType);   
                }   
                 else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                 {   
                         //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                 }   
            }   
                //执行方法   
            object returnValue = testMethod.Invoke(classObject, obj);


         testMethod = typeClass.GetMethod("Can_SendExtFrame");
            
            paramsInfo = testMethod.GetParameters();//得到指定方法的参数列表   
           length = paramsInfo.Length;

           cmd = "402690304,100,0,0,4,5,6,7,8";
          cmdParameter = cmd.Split(',');
          obj = new object[length];

            for (int i = 0; i < length; i++)
            {
                Type tType = paramsInfo[i].ParameterType;
                //如果它是值类型,或者String   
                if (tType.Equals(typeof(string)) || (!tType.IsInterface && !tType.IsClass))
                {
                    //改变参数类型   
                    obj[i] = Convert.ChangeType(cmdParameter[i], tType);
                }
                else if (tType.IsClass)//如果是类,将它的json字符串转换成对象   
                {
                    //obj[i] = Newtonsoft.Json.JsonConvert.DeserializeObject(Request.Form[i], tType);   
                }
            }
            //执行方法   
            testMethod.Invoke(classObject, obj);  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SQLiteClass sqlite = new SQLiteClass();

            sqlite.CreateNewVariableTable(@"F:\Bluewave New Energy\SVN_Work\BMS_MonitorC#\trunk -AutoTest\AutoTest\bin\Release\AutoTest.db", "设备初始化_Variable");
        }

    }
}
