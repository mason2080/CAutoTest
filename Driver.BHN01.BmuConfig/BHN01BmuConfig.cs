using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoTestAttribute;
using System.Threading;
using System.Runtime.InteropServices;

namespace Driver.BHN01.BmuConfig
{
    [AutoTestClassAttribute("BHN01_BMU参数读写", "设备类型,波特率250/500,设备索引号,通道号")]
    public class BHN01BmuConfig
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


        /// <summary>
        /// BMU参数读取库
        /// </summary>
        /// <param name="Source_BMUNO"></param>
        /// <param name="CANType"></param>
        /// <param name="CanIndex"></param>
        /// <param name="CanChannel"></param>
        /// <param name="Path"></param>
        /// <param name="BMUIndex"></param>
        /// <param name="Target_BMUNO"></param>
        /// <returns></returns>
        [DllImport("BMUConfig.dll", EntryPoint = "BMU")]
        static extern UInt32 BMUConfig(byte Source_BMUNO, UInt32 CANType, UInt16 CanIndex, UInt16 CanChannel, char[] Path, byte BMUIndex, double Target_BMUNO);

        /// <summary>
        /// BMU参数读取库
        /// </summary>
        /// <param name="BMUNO"></param>
        /// <param name="CANType"></param>
        /// <param name="CanIndex"></param>
        /// <param name="CanChannel"></param>
        /// <param name="BMUIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [DllImport("BMUReadConfig.dll", EntryPoint = "BMUReadConfig")]
        static extern byte BMUReadConfig(byte BMUNO, UInt32 CANType, UInt16 CanIndex, UInt16 CanChannel, byte BMUIndex, char[] path);


         uint gdevicetype = 0;
         uint gbaudrate = 0;
         uint gdeviceIndex = 0;
         ushort gchannel = 0;

         uint result = 0;


        char[] path = new char[200];
        char[] configPath = new char[200];



       [DescriptionAttribute("开始写入", "路径,源板号,目标板号,结果.Out")]
        public uint Start_Write(string programPath,byte sourceBmuNo,ushort targetBmuNo,ref uint Result)
        {
            path = programPath.ToArray();
            try
            {
                Result = BMUConfig(sourceBmuNo, gdevicetype, (ushort)gdeviceIndex, gchannel, path, 0, targetBmuNo); //AutoBurn(gdevicetype, gdeviceIndex, gchannel, boardNo, path);
                return 1;
            }
            catch 
            {
                return 0;
            }
        }

       [DescriptionAttribute("开始读取", "路径,板号,结果.Out")]
       public uint Start_Read(string programPath, byte boardNo, ref uint Result)
       {
           path = programPath.ToArray();
           //result = 0;

           try
           {
               Result = BMUReadConfig(boardNo, gdevicetype, (ushort)gdeviceIndex, gchannel, 0, path);// AutoBurn(gdevicetype, gdeviceIndex, gchannel, boardNo, path);
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
