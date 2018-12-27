//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于调用GPIB接口，需要NI-488.2支持
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NationalInstruments.NI4882;
using AutoTestAttribute;

namespace Driver.AgilentCurrPower.A6680
{
    [AutoTestClassAttribute("Agilent6680A电流源", "板号,地址")]
    public class A6680Class
    {
        Device gpib = null;//new Device(0,1);
        int boardNumber = 0;
        byte  address = 0;


        [DescriptionAttribute("打开端口", "")]
        public int Port_Open()
        {
            try
            {
                gpib = new Device(boardNumber, address);
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
                gpib.Clear();
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

        public void Init_Class(int boardNumber_in, byte address_in)//必须有此函数
        {
            boardNumber = boardNumber_in;
            address = address_in;
        }

        [DescriptionAttribute("输出控制", "状态:0x01开/0x00关")]
        public int OutPut(int status)
        {
            try
            {
                if (status == 1)
                {
                    gpib.Write("OUTP ON\r\n");
                }
                else
                {
                    gpib.Write("Curr 1" + "\r\n");
                    gpib.Write("OUTP OFF\r\n");
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
                gpib.Write("VOLT " + volt + "\r\n");

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        [DescriptionAttribute("设置电流", "电流值")]
        public int SetCurrent(string curr)
        {
            try
            {
                gpib.Write("Curr " + curr + "\r\n");

                return 1;
            }
            catch
            {
                return 0;
            }
        }


        public void write(string cmd)
        {
            gpib.Write(cmd);
        }



        public string read()
        {
            return gpib.ReadString();
        }
    }
}
