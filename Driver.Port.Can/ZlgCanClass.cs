//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         调用ZLGCAN驱动CAN卡

//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using AutoTestAttribute;

public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] //MarshalAs属性指示如何在托管代码和非托管代码之间封送数据。
    public byte[] str_Serial_Num;                         //MarshalAs这个属性很难用，很容易用错，用好需要对C#、C++和COM数据的布局方式有一定的了解才能做
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved;
}
/////////////////////////////////////////////////////
//2.定义CAN信息帧的数据类型。
unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;
    public byte TimeFlag;
    public byte SendType;
    public byte RemoteFlag;//是否是远程帧
    public byte ExternFlag;//是否是扩展帧
    public byte DataLen;

    public fixed byte Data[8];

    public fixed byte Reserved[3];

}


//3.定义CAN控制器状态的数据类型。
public struct VCI_CAN_STATUS
{
    public byte ErrInterrupt;
    public byte regMode;
    public byte regStatus;
    public byte regALCapture;
    public byte regECCapture;
    public byte regEWLimit;
    public byte regRECounter;
    public byte regTECounter;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}

//4.定义错误信息的数据类型。
public struct VCI_ERR_INFO
{
    public UInt32 ErrCode;
    public byte Passive_ErrData1;
    public byte Passive_ErrData2;
    public byte Passive_ErrData3;
    public byte ArLost_ErrData;
}

//5.定义初始化CAN的数据类型
public struct VCI_INIT_CONFIG
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;
    public byte Timing0;
    public byte Timing1;
    public byte Mode;
}


public struct VCI_FILTER_RECORD
{
    public UInt32 ExtFrame;
    public UInt32 Start;
    public UInt32 End;
}

public enum VCI_DEVICE_TYPE
{
    VCI_PCI5121 = 1,
    VCI_PCI9810 = 2,
    VCI_USBCAN1 = 3,
    VCI_USBCAN2 = 4,
    VCI_USBCAN2A = 4,
    VCI_PCI9820 = 5,
    VCI_CAN232 = 6,
    VCI_PCI5110 = 7,
    VCI_CANLITE = 8,
    VCI_ISA9620 = 9,
    VCI_ISA5420 = 10,
    VCI_PC104CAN = 11,
    VCI_CANETUDP = 12,
    VCI_CANETE = 12,
    VCI_DNP9810 = 13,
    VCI_PCI9840 = 14,
    VCI_PC104CAN2 = 15,
    VCI_PCI9820I = 16,
    VCI_CANETTCP = 17,
    VCI_PEC9920 = 18,
    VCI_PCI5010U = 19,
    VCI_USBCAN_E_U = 20,
    VCI_USBCAN_2E_U = 21,
    VCI_PCI5020U = 22,
    VCI_EG20T_CAN = 23,
}

namespace Driver.Port.Can
{

    [AutoTestClassAttribute("周立功CAN操作", "设备类型,波特率250/500,设备索引号,通道号")]
    public class ZlgCanClass
    {
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        //static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        unsafe static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, byte* pData);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        public uint gdevicetype = 0;
        public uint gbaudrate = 0;
        public uint gdeviceIndex = 0;
        public uint gchannel = 0;

        uint result = 0;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="devicetype">0:USBCAN 1:USBCAN_2E_U</param>
        /// <param name="baudrate">0:250K 1:500K</param>
        /// <param name="deviceIndex">设备索引 0~20</param>
        /// <param name="channel">CAN通道 0~1</param>
        /// 
        public ZlgCanClass(uint devicetype, uint baudrate, uint deviceIndex, uint channel)
        {
            gdevicetype = devicetype;
            gbaudrate = baudrate;
            gdeviceIndex = deviceIndex;
            gchannel = channel;
        }


        /// <summary>
        /// 接口函数实现：设备初始化
        /// </summary>
        public void Connect_Device()
        {
            result= Can_Init();
        }


        /// <summary>
        /// 接口函数实现：设备关闭
        /// </summary>
        public void Disconnect_Device()
        {
            Can_Close();
        
        }

        /// <summary>
        /// 接口函数实现：设备复位并关闭
        /// </summary>
        public void Close_Device()
        {
            try
            {
                if (result == 1)
                {
                    Can_Close();
                }
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
                result = Can_Init();
            }
            catch { }
        }




        /// <summary>
        /// 初始化，用于设备相关参数的输入：如端口号，波特率，设备类型等，参数个数需要与类的属性中相对应
        /// </summary>
        /// <param name="devicetype"></param>
        /// <param name="baudrate"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="channel"></param>
        public void Init_Class(uint devicetype, uint baudrate, uint deviceIndex, uint channel)//必须有此函数
        {
            gdevicetype = devicetype;
            gbaudrate = baudrate;
            gdeviceIndex = deviceIndex;
            gchannel = channel;
        }

        public ZlgCanClass()
        {
 
        }

        unsafe public uint Can_InitChanWithNoFilter(int baudrate)
        {
            VCI_INIT_CONFIG vci_init_config = new VCI_INIT_CONFIG();
            vci_init_config.Mode = 0;//正常模式
            vci_init_config.Filter = 0;
            vci_init_config.AccCode = 0x00000000;
            vci_init_config.AccMask = 0xFFFFFFFF;
            //VCI_CloseDevice(gdevicetype, gdeviceIndex);

            if ((gdevicetype == 2) || (gdevicetype == 4) || (gdevicetype == 14)) //USBCAN2
            {
                if (baudrate == 250) //250K
                {
                    vci_init_config.Timing0 = 0x01;
                    vci_init_config.Timing1 = 0x1c;
                }
                else if (baudrate == 500) //500K
                {
                    vci_init_config.Timing0 = 0x00;
                    vci_init_config.Timing1 = 0x1c;
                }

                VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
            }
            else //USBCAN2E_U
            {
                UInt32 baud;
                if (baudrate == 250)
                {
                    baud = 0x1c0008;
                }
                else if (baudrate == 500)
                {
                    baud = 0x60007;
                }
                VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 0, (byte*)&baud);
                VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
            }
            return 0;
        }
        /// <summary>
        /// 初始化CAN设备
        /// </summary>
        /// 
        unsafe public uint Can_InitChanWithFilter(int baudrate)
        {
            VCI_INIT_CONFIG vci_init_config = new VCI_INIT_CONFIG();
            vci_init_config.Mode = 0;//正常模式
            vci_init_config.Filter = 1;
            vci_init_config.AccCode = 0xC0000000;
            vci_init_config.AccMask = 0x3FFFFFFF;

            //VCI_CloseDevice(gdevicetype, gdeviceIndex);
            try
            {
                if ((gdevicetype == 2) || (gdevicetype == 4) || (gdevicetype == 14)) //USBCAN2
                {
                    if (baudrate == 250) //250K
                    {
                        vci_init_config.Timing0 = 0x01;
                        vci_init_config.Timing1 = 0x1c;
                    }
                    else if (baudrate == 500) //500K
                    {
                        vci_init_config.Timing0 = 0x00;
                        vci_init_config.Timing1 = 0x1c;
                    }

                    VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                }
                else //USBCAN2E_U
                {
                    UInt32 baud;
                    if (baudrate == 250)
                    {
                        baud = 0x1c0008;
                    }
                    else if (baudrate == 500)
                    {
                        baud = 0x60007;
                    }

                    VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 0, (byte*)&baud);
                    //Thread.Sleep(100);
                    VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                    //Thread.Sleep(100);
                    VCI_FILTER_RECORD filterRecord = new VCI_FILTER_RECORD();
                    filterRecord.ExtFrame = 1;
                    filterRecord.Start = 0x18000000;
                    filterRecord.End = 0x18ffffff;
                    //填充滤波表格
                    VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 1, (byte*)&filterRecord);
                    //Thread.Sleep(100);                   //使滤波表格生效
                    byte tm = 0;
                    VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 2, &tm);
                }
            }
            catch { }

            return 0;
        }


        /// <summary>
        /// 初始化CAN设备
        /// </summary>
        /// 
        unsafe public uint  Can_InitChan1(int baudrate)
        {
            VCI_INIT_CONFIG vci_init_config = new VCI_INIT_CONFIG();
            vci_init_config.Mode = 0;//=0 正常模式   =1 表示只听模式（只接收，不影响总线）
            vci_init_config.Filter = 0;//=1 表示单滤波， =0 表示双滤波
            vci_init_config.AccCode = 0x00000000;
            vci_init_config.AccMask = 0xFFFFFFFF;


            if ((gdevicetype == 2) || (gdevicetype == 4) || (gdevicetype == 14)) //USBCAN2
                {
                    if (baudrate == 250) //250K
                    {
                        vci_init_config.Timing0 = 0x01;
                        vci_init_config.Timing1 = 0x1c;
                    }
                    else if (baudrate == 500) //500K
                    {
                        vci_init_config.Timing0 = 0x00;
                        vci_init_config.Timing1 = 0x1c;
                    }

                    VCI_InitCAN(gdevicetype, gdeviceIndex, 1, ref  vci_init_config);
                }
                else //USBCAN2E_U
                {
                    UInt32 baud;
                    if (baudrate == 250)
                    {
                        baud = 0x1c0008;
                    }
                    else if (baudrate == 500)
                    {
                        baud = 0x60007;
                    }
                    if (VCI_SetReference(gdevicetype, gdeviceIndex, 1, 0, (byte*)&baud) != 1)
                    {
                        //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(gdevicetype, gdeviceIndex);
                        return 0;
                    }
                    else
                    {
                        VCI_InitCAN(gdevicetype, gdeviceIndex, 1, ref  vci_init_config);
                    }

                    return (VCI_StartCAN(gdevicetype, gdeviceIndex, 1));
                }

            return 0;
        }

        [DescriptionAttribute("相加", "a,b,sum.Out")]
        public uint Add(int a, int b, ref int sum)
        {
            sum = a + b;
            return 1;
        }

        [DescriptionAttribute("CAN初始化", "")]
        unsafe public uint  Can_Init()
        {
           
          VCI_INIT_CONFIG vci_init_config=new VCI_INIT_CONFIG();
          vci_init_config.Mode = 0;//正常模式
          vci_init_config.Filter = 1;
          vci_init_config.AccCode = 0x00000000;
          vci_init_config.AccMask = 0xFFFFFFFF;

          try
          {
              if (VCI_OpenDevice(gdevicetype, gdeviceIndex, 0) == 1) //打开设备成功
              {

                  if ((gdevicetype == 2) || (gdevicetype == 4) || (gdevicetype == 14)) //USBCAN2
                  {
                      if (gbaudrate == 250) //250K
                      {
                          vci_init_config.Timing0 = 0x01;
                          vci_init_config.Timing1 = 0x1c;
                      }
                      else if (gbaudrate == 500) //500K
                      {
                          vci_init_config.Timing0 = 0x00;
                          vci_init_config.Timing1 = 0x1c;
                      }

                       VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                  }
                  else //USBCAN2E_U
                  {
                      UInt32 baud;
                      if (gbaudrate == 250)
                      {
                          baud = 0x1c0008;
                      }
                      else if (gbaudrate == 500)
                      {
                          baud = 0x60007;
                      }
                      if (VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 0, (byte*)&baud) != 1)
                      {
                          //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                          VCI_CloseDevice(gdevicetype, gdeviceIndex);
                          return 0;
                      }
                      else
                      {
                          VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                      }
                  }
                  return (VCI_StartCAN(gdevicetype, gdeviceIndex, gchannel));
              }
          }
          catch { }
          return 0;
        }


        //[DescriptionAttribute("CAN初始化","设备类型,波特率250/500,设备索引号,通道号")]
        unsafe public uint Can_Open(uint deviceType, uint baudRate, uint deviceInd, uint canChannel)
        {
            VCI_INIT_CONFIG vci_init_config = new VCI_INIT_CONFIG();
            vci_init_config.Mode = 0;//正常模式
            vci_init_config.Filter = 1;
            vci_init_config.AccCode = 0x00000000;
            vci_init_config.AccMask = 0xFFFFFFFF;

            gdevicetype = deviceType;
            gbaudrate = baudRate;
            gdeviceIndex = deviceInd;
            gchannel = canChannel;



            if (VCI_OpenDevice(deviceType, deviceInd, 0) == 1) //打开设备成功
            {

                if ((deviceType == 2) || (deviceType == 4) || (deviceType == 14)) //USBCAN2
                {
                    if (baudRate == 250) //250K
                    {
                        vci_init_config.Timing0 = 0x01;
                        vci_init_config.Timing1 = 0x1c;
                    }
                    else if (baudRate == 500) //500K
                    {
                        vci_init_config.Timing0 = 0x00;
                        vci_init_config.Timing1 = 0x1c;
                    }

                    VCI_InitCAN(deviceType, deviceInd, canChannel, ref  vci_init_config);
                }
                else //USBCAN2E_U
                {
                    UInt32 baud;
                    if (baudRate == 250)
                    {
                        baud = 0x1c0008;
                    }
                    else if (baudRate == 500)
                    {
                        baud = 0x60007;
                    }
                    if (VCI_SetReference(deviceType, deviceInd, canChannel, 0, (byte*)&baud) != 1)
                    {
                        //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(gdevicetype, gdeviceIndex);
                        return 0;
                    }
                    else
                    {
                        VCI_InitCAN(deviceType, deviceInd, canChannel, ref  vci_init_config);
                    }
                }
                return (VCI_StartCAN(deviceType, deviceInd, canChannel));
            }
            return 0;
        }



        unsafe public uint Can_Init1()
        {
            VCI_INIT_CONFIG vci_init_config = new VCI_INIT_CONFIG();
            vci_init_config.Mode = 0;//正常模式
            vci_init_config.Filter = 1;
            vci_init_config.AccCode = 0xC0000000;
            vci_init_config.AccMask = 0x3FFFFFFF;
            try
            {
                if (VCI_OpenDevice(gdevicetype, gdeviceIndex, 0) == 1) //打开设备成功
                {

                    if ((gdevicetype == 2) || (gdevicetype == 4) || (gdevicetype == 14)) //USBCAN2
                    {
                        if (gbaudrate == 250) //250K
                        {
                            vci_init_config.Timing0 = 0x01;
                            vci_init_config.Timing1 = 0x1c;
                        }
                        else if (gbaudrate == 500) //500K
                        {
                            vci_init_config.Timing0 = 0x00;
                            vci_init_config.Timing1 = 0x1c;
                        }

                        VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                    }
                    else //USBCAN2E_U
                    {
                        UInt32 baud;
                        if (gbaudrate == 250)
                        {
                            baud = 0x1c0008;
                        }
                        else if (gbaudrate == 500)
                        {
                            baud = 0x60007;
                        }
                        if (VCI_SetReference(gdevicetype, gdeviceIndex, gchannel, 0, (byte*)&baud) != 1)
                        {
                            //MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            VCI_CloseDevice(gdevicetype, gdeviceIndex);
                            return 0;
                        }
                        else
                        {
                            VCI_InitCAN(gdevicetype, gdeviceIndex, gchannel, ref  vci_init_config);
                        }
                    }
                    return (VCI_StartCAN(gdevicetype, gdeviceIndex, gchannel));
                }
            }
            catch { }
            return 0;
        }


        public void GetReceiveNum()
        {
            VCI_GetReceiveNum(gdevicetype, gdeviceIndex, gchannel);
        }

        /// <summary>
        /// 关闭CAN设备
        /// </summary>
        /// <returns></returns>
       [DescriptionAttribute("CAN关闭","")]
        public uint  Can_Close()
        {
            try
            {
                if (VCI_CloseDevice(gdevicetype, gdeviceIndex) == 1)
                {
                    return 1;
                }
            }
            catch { }

            return 0;
        }

        /// <summary>
        /// 清设备缓存数据
        /// </summary>
        /// <returns></returns>
        [DescriptionAttribute("CAN清除缓存","")]
        public bool CanClearBuffer()
        {
            if(VCI_ClearBuffer(gdevicetype,gdeviceIndex,gchannel) ==1)
            {
                return true;
            }
           
            return false;
        }

       /// <summary>
       /// CAN接收
       /// </summary>
       /// <returns></returns>
        [DescriptionAttribute("CAN接收","num")]
        unsafe public VCI_CAN_OBJ[] Can_Receive(ref uint num)
        {
            VCI_CAN_OBJ[] objArray = new VCI_CAN_OBJ[5]; 
            try
            {

                UInt32 res = new UInt32();
                UInt32 con_maxlen = 5;
                //res = VCI_GetReceiveNum(gdevicetype, gdeviceIndex, gchannel);

                //if (res > 5)
                //{
                //    if (res > 5)
                //    {
                //        res = 5;
                //    }

                    IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)con_maxlen);
                   // IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)res);

                    num = VCI_Receive(gdevicetype, gdeviceIndex, gchannel, pt, con_maxlen, 20);

                    if (num != 0)
                    {
                        for (UInt32 i = 0; i < num; i++)
                        {
                            VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                            objArray[i] = obj;
                        }
                    }
                //}
            }
            catch 
            { 
            }
            return objArray;
        }

        unsafe public VCI_CAN_OBJ[] Can_ReceiveCh1(ref uint num)
        {
            VCI_CAN_OBJ[] objArray = new VCI_CAN_OBJ[5];
            try
            {
                UInt32 res = new UInt32();
                UInt32 con_maxlen = 5;
                IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)con_maxlen);
                num = VCI_Receive(gdevicetype, gdeviceIndex, 1, pt, 5, 20);
                if (num != 0)
                {
                    for (UInt32 i = 0; i < num; i++)
                    {
                        VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                        objArray[i] = obj;
                    }
                }
            }
            catch
            {
            }
            return objArray;
        }


        /// <summary>
        /// 发送一帧CAN信息
        /// </summary>
        /// <param name="canId">Can Id</param>
        /// <param name="sendData">需要发送的字节数组</param>
        /// <param name="externFlag">00：标准帧 01：扩展帧</param>
         
        unsafe public void Can_SendOneFrame(uint canId, byte[] sendData,byte externFlag)
        {
            byte  i = 0;
            VCI_CAN_OBJ vci_can_obj =new VCI_CAN_OBJ();
            vci_can_obj.ID = canId;
            vci_can_obj.ExternFlag = externFlag;
            vci_can_obj.RemoteFlag =0;
            vci_can_obj.SendType = 0;
            vci_can_obj.DataLen = 8;
            for (i = 0; i < 8; i++)
            {
                vci_can_obj.Data[i] = sendData[i];
            }
            VCI_Transmit(gdevicetype, gdeviceIndex, gchannel, ref vci_can_obj, 1);
        }

        [DescriptionAttribute("Can发送扩展帧","id,data0,data1,data2,data3,data4,data5,data6,data7")]
        unsafe public int Can_SendExtFrame(uint canId, byte data0, byte data1, byte data2, byte data3, byte data4, byte data5, byte data6, byte data7)
         {
             byte i = 0;
             VCI_CAN_OBJ vci_can_obj = new VCI_CAN_OBJ();
             vci_can_obj.ID = canId;
             vci_can_obj.ExternFlag = 1;
             vci_can_obj.RemoteFlag = 0;
             vci_can_obj.SendType = 0;
             vci_can_obj.DataLen = 8;

             vci_can_obj.Data[0] = data0;
             vci_can_obj.Data[1] = data1;
             vci_can_obj.Data[2] = data2;
             vci_can_obj.Data[3] = data3;
             vci_can_obj.Data[4] = data4;
             vci_can_obj.Data[5] = data5;
             vci_can_obj.Data[6] = data6;
             vci_can_obj.Data[7] = data7;

             if (VCI_Transmit(gdevicetype, gdeviceIndex, gchannel, ref vci_can_obj, 1) == 1)
             {
                 return 1;
             }
             else 
             {
                 return 0;
             }
         }



        unsafe public void Can_SeltTestFrame(uint canId, byte[] sendData, byte externFlag)
        {
            byte i = 0;
            VCI_CAN_OBJ vci_can_obj = new VCI_CAN_OBJ();
            vci_can_obj.ID = canId;
            vci_can_obj.ExternFlag = externFlag;
            vci_can_obj.RemoteFlag = 0;
            vci_can_obj.SendType = 2;
            vci_can_obj.DataLen = 8;
            for (i = 0; i < 8; i++)
            {
                vci_can_obj.Data[i] = sendData[i];
            }

            VCI_Transmit(gdevicetype, gdeviceIndex, gchannel, ref vci_can_obj, 1);
        }


        unsafe public void Can_SendOneFrameCh1(uint canId, byte[] sendData, byte externFlag)
        {
            byte i = 0;
            VCI_CAN_OBJ vci_can_obj = new VCI_CAN_OBJ();
            vci_can_obj.ID = canId;
            vci_can_obj.ExternFlag = externFlag;
            vci_can_obj.RemoteFlag = 0;
            vci_can_obj.SendType = 0;
            vci_can_obj.DataLen = 8;
            for (i = 0; i < 8; i++)
            {
                vci_can_obj.Data[i] = sendData[i];
            }

            VCI_Transmit(gdevicetype, gdeviceIndex, 1, ref vci_can_obj, 1);
        }

    }
}
