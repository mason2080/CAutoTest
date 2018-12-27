using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using AutoTestAttribute;
using System.Threading;
using System.Windows;

using Driver.File.Log;

namespace Driver.Relay.I16O16
{
    [AutoTestClassAttribute("智嵌16IN16OUT", "端口号,波特率")]
    public class RelayI16O16Class
    {
        SerialPort rs232 = new SerialPort();

        LogFileClass logFile = new LogFileClass();


       [DescriptionAttribute("打开端口", "")]
        public int Port_Open()
        {      
            try
            {
                rs232.Open();
            }
            catch
            {
                logFile.LogHistory("Open RS232 " + rs232.PortName + " NG");
                return 0;
            }

            logFile.LogHistory("Open RS232 " + rs232.PortName + " OK");
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
               logFile.LogHistory("Close RS232 " + rs232.PortName + " NG");
               return 0;
           }

           logFile.LogHistory("Close RS232 " + rs232.PortName + " OK");
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
               RelayControl(85, 0);
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
               RelayControl(85, 0);
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

        [DescriptionAttribute("继电器控制", "RelayNo:170全开/85全关,状态:0x01开/0x00关")]
        public int  RelayControl(int RelayNo, int status)
        {
            int p = 0;
            byte[] cmd = new byte[50];
            cmd[p++] = 0x48;
            cmd[p++] = 0x3A;
            cmd[p++] = 0x00;
            cmd[p++] = 0x57;


            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;
            cmd[p++] = 0xFF;

            switch(RelayNo)
            {
                case 1:
                    cmd[4] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 2:
                    cmd[4] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 3:
                    cmd[5] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 4:
                    cmd[5] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;


                case 5:
                    cmd[6] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 6:
                    cmd[6] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 7:
                    cmd[7] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 8:
                    cmd[7] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 9:
                    cmd[8] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 10:
                    cmd[8] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 11:
                    cmd[9] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 12:
                    cmd[9] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 13:
                    cmd[10] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 14:
                    cmd[10] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;

                case 15:
                    cmd[11] = (status == 0x01) ? (byte)0xF1 : (byte)0xF0;
                    break;
                case 16:
                    cmd[11] = (status == 0x01) ? (byte)0x1F : (byte)0x0F;
                    break;
            }

            if (RelayNo == 0xAA)
            {
                cmd[4] = 0x11;
                cmd[5] = 0x11;
                cmd[6] = 0x11;
                cmd[7] = 0x11;
                cmd[8] = 0x11;
                cmd[9] = 0x11;
                cmd[10] = 0x11;
                cmd[11] = 0x11;
            }
            else if (RelayNo == 0x55)
            {
                cmd[4] = 0x00;
                cmd[5] = 0x00;
                cmd[6] = 0x00;
                cmd[7] = 0x00;
                cmd[8] = 0x00;
                cmd[9] = 0x00;
                cmd[10] = 0x00;
                cmd[11] = 0x00;
            }

            cmd[12] = Crc8(cmd, 12);

            cmd[13] = 0x45;
            cmd[14] = 0x44;

            try
            {
                rs232.Write(cmd, 0, 15);
            }
            catch 
            {
                logFile.LogHistory("RS232 " + rs232.PortName + " TX:" + logFile.byteToHexStr(cmd) + " NG");
                return 0;
            }

            logFile.LogHistory("RS232 " + rs232.PortName + " TX:" +logFile.byteToHexStr(cmd) + " OK");
            return 1;
        }

        [DescriptionAttribute("DI检测", "DI端口,Status.Out")]
        public int ReadDiStatus(int portNo, ref int status)
        {
            int p = 0;
            byte[] cmd = new byte[50];
            byte[] data = new byte[100];

            status = 0;

            cmd[p++] = 0x48;
            cmd[p++] = 0x3A;
            cmd[p++] = 0x00;
            cmd[p++] = 0x52;
            cmd[4] = 0x00;
            cmd[5] = 0x00;
            cmd[6] = 0x00;
            cmd[7] = 0x00;
            cmd[8] = 0x00;
            cmd[9] = 0x00;
            cmd[10] = 0x00;
            cmd[11] = 0x00;
            cmd[12] = Crc8(cmd, 12);
            cmd[13] = 0x45;
            cmd[14] = 0x44;

            try
            {
                rs232.Write(cmd, 0, 15);

                logFile.LogHistory("RS232 " + rs232.PortName + " TX:" + cmd.ToString());

                Thread.Sleep(500);

                rs232.Read(data, 0, 15);

                if ((data[0] == 0x48) && (data[1] == 0x3A) && (data[3] == 0x41))
                {
                    if (data[12] == Crc8(data, 12))
                    {
                        switch (portNo)
                        {
                            case 1:
                                status = data[4] & 0x0F;
                                break;
                            case 2:
                                status =( data[4] & 0xF0)>>4;
                                break;

                            case 3:
                                status = data[5] & 0x0F;
                                break;
                            case 4:
                                status = (data[5] & 0xF0) >> 4;
                                break;

                            case 5:
                                status = data[6] & 0x0F;
                                break;
                            case 6:
                                status = (data[6] & 0xF0) >> 4;
                                break;

                            case 7:
                                status = data[7] & 0x0F;
                                break;
                            case 8:
                                status = (data[7] & 0xF0) >> 4;
                                break;

                            case 9:
                                status = data[8] & 0x0F;
                                break;
                            case 10:
                                status = (data[8] & 0xF0) >> 4;
                                break;

                            case 11:
                                status = data[9] & 0x0F;
                                break;
                            case 12:
                                status = (data[9] & 0xF0) >> 4;
                                break;

                            case 13:
                                status = data[10] & 0x0F;
                                break;
                            case 14:
                                status = (data[10] & 0xF0) >> 4;
                                break;

                            case 15:
                                status = data[11] & 0x0F;
                                break;
                            case 16:
                                status = (data[11] & 0xF0) >> 4;
                                break;
                        }

                        logFile.LogHistory("RS232 " + rs232.PortName + " RX:" + logFile.byteToHexStr(data) + " OK");
                        return 1;
                    }
                }
                logFile.LogHistory("RS232 " + rs232.PortName + " RX:" + logFile.byteToHexStr(data) + " Error");
                return 0;
            }
            catch 
            {
            }
            

            return 1;
        }

        Byte Crc8(byte[] input,int len)
        {
            byte crc = 0;

            for (int i = 0; i < len; i++)
            {
                crc += input[i];
            }

            return crc;
        }

        //public static string byteToHexStr(byte[] bytes)
        //{
        //    string returnStr = "";
        //    if (bytes != null)
        //    {
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            returnStr += bytes[i].ToString("X2");

        //        }
        //    }

        //    return returnStr;
        //}
    }
}
