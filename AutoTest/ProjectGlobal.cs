using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Driver.Port.Can;

namespace AutoTest
{
    class ProjectGlobal
    {

        
        public  struct BMS_INFO
        {
            public static int[] boardCnt = { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };

            public static byte slaveNum=3;
            public static byte MainLife;
            public static double VoltSupply;
            public static double inSumVolt;
            public static double extSumVolt;
            public static double insCurrent;
            public static double avgCurrent;

            public static double maxVolt;
            public static int maxVoltLoc;
            public static double minVolt;
            public static int minVoltLoc;
            public static double SOC;
            public static double SOH;


            public static int MaxTemp;             //最高温度
            public static int MaxTempLoc;          //最高温度位置
            public static int MinTemp;             //最低温度
            public static int MinTempLoc;          //最低温度位置


            public static double maxChgCur;         //最大充电电流
            public static double maxDischgCur;      //最大放电电流
            public static double ActCap;            //实际容量

            public static double totalInAh;
            public static double totalOutAh;

            public static byte intCanState;
            public static byte carCanState;
            public static Byte chgCanState;
            public static string bmsClock;         //Bms时钟

            public static byte relayPos;
            public static byte relayNeg;
            public static byte relayPre;
            public static byte relayChg;
            public static byte relayFan;
            public static byte relayHeat;
            public static byte DO1;
            public static byte DO2;
            public static double  cp;
            public static byte DI1=0;
            public static byte DI2 = 0;
            public static byte DI3 = 0;
            public static byte DI4 = 0;
            public static double CC;
            public static double AI1;
            public static double AI2;
            public static double CC2;


            public static int[] batErrorCode = new int[8];
            public static int[] batSysErrorCode = new int[8];
            public static int[] otherErrorCode = new int[8];
            public static int[] hardwareErrorCode = new int[8];

            //*****************************BMU INFO
            public static double[,] slaveVolt = new double[30, 60];
            public static byte[,] balanceFlag = new byte[30, 60];
            public static byte[] balanceState = new byte[30];
            public static int[] cellNumber = new int[31];
            public static int[,] slaveTemp = new int[30, 10];
            public static int[] tempNumber = new int[31];

            public static double[] sumVolt_Bmu=new double[31];
            public static double[] maxVolt_Bmu = new double[31];
            public static int[] maxVoltLoc_Bmu = new int[31];
            public static double[] minVolt_Bmu = new double[31];
            public static int[] minVoltLoc_Bmu = new int[31];
            public static int[] maxTemp_Bmu = new int[31];
            public static int[] maxTempLoc_Bmu = new int[31];
            public static int[] minTemp_Bmu = new int[31];
            public static int[] minTempLoc_Bmu = new int[31];

            public static byte[] relayFan_Bmu = new byte[31];
            public static byte[] relayHeat_Bmu = new byte[31];
            public static byte[] Di1_Bmu = new byte[31];
            public static byte[] Do1_Bmu = new byte[31];
            public static double[] Ai1_Bmu = new double[31];

            public static byte[,] hardwareErrorCode_Bmu = new byte[31,8];

            ///////////////////ChgInfo

            public static byte chg_pmt_ctl_state;
            public static byte chg_sgnl_state;
            public static byte AC_chg_adjon_cap;
            public static byte chg_adjon_state;
            public static byte chg_state;
            public static byte chg_step;
            public static byte charge_state;

            public static double Total_chg_time;
            public static double Rest_chg_time;
            public static double Max_Pmt_chg_SumV;
            public static double Max_Pmt_chg_cellV;
            public static double Max_Pmt_chg_cur;
            public static double Min_Pmt_chg_cur;
            public static double Chg_cur_step;
            public static int Max_Pmt_chg_t;

            public static double Chg_output_sumv;
            public static double Chg_output_cur;
            public static double Chg_total_energy;

            public static byte bms_chg_stop1;
            public static byte bms_chg_stop2;
            public static byte bms_chg_stop3;

            public static byte chg_stop;

            public static byte chg_fault1;
            public static byte chg_fault2;
        }

        public static ZlgCanClass zlgCan;

        public static string ProjectName;

        public static string UserName;

        public static string SkinPath = "\\SKins\\DiamondBlue.ssk";

        public static string Title;

        //public  struct AiDiDo
        //{
        //    public static byte relayPos;
        //    public static byte relayNeg;
        //    public static byte relayPre;
        //    public static byte relayChg;
        //    public static byte relayAux_A;
        //    public static byte relayAux_B;
        //    public static byte DO1;
        //    public static byte DO2;
        //    public static byte cp;
        //    public static byte DI1;
        //    public static byte DI2;
        //    public static byte DI3;
        //    public static byte DI4;
        //    public static double CC;
        //    public static double AI1;
        //    public static double AI2;
        //    public static double CC2;
        //}


        //public   struct BMU_INFO
        //{
        //    public static double[,] slaveVolt=new double[30,60];
        //    public static uint[] cellNumber=new uint[31];
        //    public static int[,] slaveTemp = new int[30, 10];
        //    public static int[] tempNumber=new int[31];

        //    public static double sumVolt;
        //    public static double maxVolt;
        //    public static int maxVoltLoc;
        //    public static double minVolt;
        //    public static int minVoltLoc;
        //    public static int maxTemp;
        //    public static int maxTempLoc;
        //    public static int minTemp;
        //    public static int minTempLoc;
        //}



        //public  struct BMU_MAIN_INFO
        //{
        //    public static double sumVolt;
        //    public static double maxVolt;
        //    public static int maxVoltLoc;
        //    public static double minVolt;
        //    public static int minVoltLoc;
        //    public static int maxTemp;
        //    public static int maxTempLoc;
        //    public static int minTemp;
        //    public static int minTempLoc;
        //}
    }
}
