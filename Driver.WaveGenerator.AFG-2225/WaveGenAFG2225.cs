using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using AutoTestAttribute;
using System.Threading;

namespace Driver.WaveGenerator.AFG2225
{


    [AutoTestClassAttribute("GWINSTEK AFG-2225波形发生器", "端口号,波特率")]
    public class WaveGenAFG2225
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
           OutPut(1, 0);
           Port_Close();
       }

       /// <summary>
       /// 接口函数实现：设备连接并复位
       /// </summary>
       public void Init_Device()
       {
           try
           {
               Port_Open();
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

        [DescriptionAttribute("输出控制", "通道,状态:0x01开/0x00关")]
        public int OutPut(int channel, int status)
        {
            try
            {
                if (status == 0x01)
                {
                    if (channel == 0x01)
                    {
                        rs232.Write("OUTP1 ON;\r\n");
                    }
                    if (channel == 0x02)
                    {
                        rs232.Write("OUTP2 ON;\r\n");
                    }
                }
                else
                {
                    if (channel == 0x01)
                    {
                        rs232.Write("OUTP1 OFF;\r\n");
                    }
                    if (channel == 0x02)
                    {
                        rs232.Write("OUTP2 OFF;\r\n");
                    }
                }
                return 1;
            }
            catch 
            {
                return 0;
            }
        }
        [DescriptionAttribute("信号设置", "通道,类型1：SIN SQU PULS RAMP NOIS,Amplitude,Frequency,DC Offset")]
        public int  ConfigSignal(int chan ,string type, double amplitude,double frequency,double dcoffset)
        {
           // SOUR1:FUNC SIN;SOUR1:AMP 5.000000;SOUR1:FREQ 4000.000000;SOUR1:DCO 1.000000;
            try
            {
                rs232.Write("SOUR" + chan.ToString() + ":FUNC " + type +
                ";SOUR" + chan.ToString() + ":AMP " + amplitude.ToString() +
                ";SOUR" + chan.ToString() + ":FREQ " + frequency.ToString() +
                ";SOUR" + chan.ToString() + ":DCO " + dcoffset.ToString() + "\r\n");
            }
            catch   
            {
                return 0;
            }
            return 1;
        }

    }
}
