using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using AutoTestAttribute;
using System.Threading;

namespace Driver.DCPower.IT6723G
{
    [AutoTestClassAttribute("IT6723直流源600V5A", "端口号,波特率")]
    public class DCPowerIT6723GClass
    {
        SerialPort rs232 = new SerialPort();

       [DescriptionAttribute("打开端口", "")]
        public int Port_Open()
        {      
            try
            {
                rs232.Open();
            }
            catch
            {
                return 0;
            }

            return 1;
        }

       [DescriptionAttribute("关闭端口", "")]
       public int Port_Close()
       {
           try
           {
               rs232.Close();
           }
           catch
           {
               return 0;
           }

           return 1;
       }

       /// <summary>
       /// 接口函数实现：设备初始化
       /// </summary>
       public void Connect_Device()
       {
           Port_Open();
       }


       /// <summary>
       /// 接口函数实现：设备关闭
       /// </summary>
       public void Disconnect_Device()
       {
           Port_Close();
       }

       /// <summary>
       /// 接口函数实现：设备复位并关闭
       /// </summary>
       public void Close_Device()
       {
           try
           {
               OutPut(0);
               Port_Close();
           }
           catch { }
       }


       /// <summary>
       /// 接口函数实现：设备连接并复位
       /// </summary>
       public void Init_Device()
       {
           try
           {
               Port_Open();
               OutPut(0);
           }
           catch { }
       }


       public void Init_Class(string portName, int baudrate)//必须有此函数
        {
            rs232.PortName = portName;
            rs232.BaudRate = baudrate;

            rs232.DataBits = 8;
            rs232.StopBits = StopBits.One;
            rs232.ReadTimeout = 100;
            rs232.WriteTimeout = 100;
        }


        [DescriptionAttribute("输出控制", "状态:0x01开/0x00关")]
        public int OutPut (int status)
        {
            try
            {
                if (status == 1)
                {
                    rs232.Write("OUTP ON\r\n");
                }
                else
                {
                    rs232.Write("VOLT 1" + "\r\n");
                    rs232.Write("OUTP OFF\r\n");
                }
                return 1;
            }
            catch 
            {
                return 0;
            }
        }

        [DescriptionAttribute("设置电压", "电压值")]
        public int SetVolt(string volt)
        {
            try
            {
                    rs232.Write("VOLT "+volt+"\r\n");

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [DescriptionAttribute("设置电流", "电压值")]
        public int SetCurrent(string curr)
        {
            try
            {
                rs232.Write("Curr " + curr + "\r\n");

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [DescriptionAttribute("控制模式", "模式:0x01远程/0x00本地")]
        public int ControlMode(int status)
        {
            try
            {
                if (status == 1)
                {
                    rs232.Write("SYST:REM\r\n");
                }
                else
                {
                    rs232.Write("SYST:LOC\r\n");
                }
                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
}
