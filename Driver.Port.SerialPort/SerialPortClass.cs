//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于串口读写操作
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace Driver.Port.CSerialPort
{


    public class CSerialPort
    {
        SerialPort rs232 = new SerialPort();

        public void Open()
        {      
            rs232.PortName = "COM5";
            rs232.BaudRate = 9600;
            rs232.DataBits = 8;
            rs232.StopBits = StopBits.One;
            rs232.ReadTimeout = 100;
            rs232.WriteTimeout = 100;
            try
            {
                rs232.Open();
            }
            catch
            {

            }

        }

        public void SendStringData()
        {
            rs232.Write("hello");
        }

        public string ReadStringData()
        { 
            byte[] data =new byte[100];
            try
            {
                rs232.Read(data, 0, 100);
            }
            catch 
            {
            }
            string readdata =System.Text.Encoding.ASCII.GetString(data);
            return readdata;
        }
    }
}


  
//异常类名称 简单描述  
  
//MemberAccessException 访问错误：类型成员不能被访问  
  
//ArgumentException 参数错误：方法的参数无效  
  
//ArgumentNullException 参数为空：给方法传递一个不可接受的空参数  
  
//ArithmeticException 数学计算错误：由于数学运算导致的异常，覆盖面广。  
  
//ArrayTypeMismatchException 数组类型不匹配  
  
//DivideByZeroException 被零除  
  
//FormatException 参数的格式不正确  
  
//IndexOutOfRangeException 索引超出范围，小于0或比最后一个元素的索引还大  
  
//InvalidCastException 非法强制转换，在显式转换失败时引发  
  
//MulticastNotSupportedException 不支持的组播：组合两个非空委派失败时引发  
  
//NotSupportedException 调用的方法在类中没有实现  
  
//NullReferenceException 引用空引用对象时引发  
  
//OutOfMemoryException 无法为新语句分配内存时引发，内存不足  
  
//OverflowException 溢出  
  
//StackOverflowException 栈溢出  
  
//TypeInitializationException 错误的初始化类型：静态构造函数有问题时引发  
  
//NotFiniteNumberException 无限大的值：数字不合法  