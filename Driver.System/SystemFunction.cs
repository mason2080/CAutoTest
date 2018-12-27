using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using AutoTestAttribute;
using System.Threading;
using System.Windows.Forms;

namespace Driver.SystemFunction
{

    [AutoTestClassAttribute("系统函数", "")]
    public class SystemFunction
    {
        //SerialPort rs232 = new SerialPort();

       [DescriptionAttribute("提示窗口", "提示信息")]
        public int ShowMsg(string info)
        {
            MessageBox.Show(info);
            return 1;
        }

       [DescriptionAttribute("输入字符串窗口", "输入信息.Out")]
       public int InputString(ref string info)
       {
           //info=
           Input form = new Input();
           form.ShowDialog();
           info = form.msg;
           return 1;
       }

       [DescriptionAttribute("图片显示", "提示,自动退出时间,路径")]
       public int ShowPicture(string info,string timeout,string filePath)
       {
           ShowPicture form = new ShowPicture(info, Convert.ToInt32(timeout, 10), filePath);
           form.ShowDialog();
           return 1;
       }



       [DescriptionAttribute("输入字符串窗口", "输入数值.Out")]
       public int InputNumber(ref double data)
       {
           InputNum form = new InputNum();
           form.ShowDialog();
           data = form.data;
           return 1;
       }


       [DescriptionAttribute("带提示信息输入窗口", "提示信息,输入信息.Out")]
       public int InputStringUserDefineMsg(string msg,ref string info)
       {
           //info=
           InputUserInfo form = new InputUserInfo(msg);
           form.ShowDialog();
           info = form.msg;
           return 1;
       }

       [DescriptionAttribute("带提示信息确认窗口", "提示信息,选择结果.Out:Yes/No")]
       public int ChooseYesOrNo(string msg, ref string info)
       {
           //info=
           if (MessageBox.Show(msg, "请选择...", MessageBoxButtons.YesNo) == DialogResult.Yes)
           {
               info = "Yes";
           }
           else 
           {
               info = "No";
           }
           return 1;
       }


       [DescriptionAttribute("a+b=c", "a,b,c.Out")]
       public int Math_Add(double a,double b,ref double c)
       {
           c = a + b;
           return 1;
       }

       [DescriptionAttribute("a-b=c", "a,b,c.Out")]
       public int Math_Minus(double a, double b, ref double c)
       {
           c = a - b;
           return 1;
       }

       [DescriptionAttribute("a*b=c", "a,b,c.Out")]
       public int Math_Mul(double a, double b, ref double c)
       {
           c = a * b;
           return 1;
       }

       [DescriptionAttribute("a/b=c", "a,b,c.Out")]
       public int Math_Div(double a, double b, ref double c)
       {
           c = a / b;
           return 1;
       }

       /// <summary>
       /// 接口函数实现：设备初始化
       /// </summary>
       public void Connect_Device()
       { }


       /// <summary>
       /// 接口函数实现：设备关闭
       /// </summary>
       public void Disconnect_Device()
       { }

       /// <summary>
       /// 接口函数实现：设备复位并关闭
       /// </summary>
       public void Close_Device()
       { }

       /// <summary>
       /// 接口函数实现：设备连接并复位
       /// </summary>
       public void Init_Device()
       {
           try
           {

           }
           catch { }
       }


        public void Init_Class()//必须有此函数
        {

        }

    }
}
