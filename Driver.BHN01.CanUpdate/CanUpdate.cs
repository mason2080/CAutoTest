using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoTestAttribute;
using System.Threading;
using System.Runtime.InteropServices;

namespace Driver.BHN01.CanUpdate
{
    [AutoTestClassAttribute("新源CAN升级", "设备类型,波特率250/500,设备索引号,通道号")]
    public class CanUpdateClass
    {

        /// <summary>
        /// 软件升级库
        /// </summary>
        /// <param name="CanType  14=PCI9840  22=USBCAN-2E"></param>
        /// <param name="CanIndex"></param>
        /// <param name="CANChannel"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        [DllImport("BmuUpdate.dll", EntryPoint = "AutoBurn")]
        static extern UInt32 AutoBurn(UInt32 CanType, UInt32 CanIndex, UInt32 CANChannel, UInt16 BMUNO, char[] Path);


         uint gdevicetype = 0;
         uint gbaudrate = 0;
         uint gdeviceIndex = 0;
         ushort gchannel = 0;

         uint result = 0;


        char[] path = new char[200];
        char[] configPath = new char[200];



       [DescriptionAttribute("开始升级", "路径,板号输入,板号输出.Out,结果.Out")]
        public uint Start_Update(string programPath, uint boardNo,ref uint BoardNoOut, ref uint Result)
        {
            path = programPath.ToArray();
            //result = 0;


            if (boardNo > 0)
            {

            }
            else 
            {
                InputNum form = new InputNum();
                form.ShowDialog();

                boardNo = ushort.Parse(form.slaveNo);
            }

            try
            {
                Result = AutoBurn(gdevicetype, gdeviceIndex, gchannel, (ushort)boardNo, path);
                BoardNoOut = boardNo;
                return 1;
            }
            catch 
            {
                return 0;
            }

           
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
       public void Init_Class(uint devicetype, uint baudrate, uint deviceIndex, ushort channel)//必须有此函数
       {
           gdevicetype = devicetype;
           gbaudrate = baudrate;
           gdeviceIndex = deviceIndex;
           gchannel = channel;
       }
    }
}
