using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace Driver.BMS.BHN01
{
    public partial class GetCellVolt : Form
    {
        int count = 0;

        TextBox[] volt = new TextBox[60];

         uint gdevicetype = 0;
         uint gbaudrate = 0;
         uint gdeviceIndex = 0;
         uint gchannel = 0;
         uint gslaveNo=0;

         uint CellNum = 0;

         Thread ReceiveCanMsgThread;
         Thread TranMainMsgThread;

        public GetCellVolt(uint slaveNo,uint cellNum)
        {
            InitializeComponent();

            CellNum = cellNum;
            textSlaveNo.Text = slaveNo.ToString();
            gslaveNo = slaveNo;
            //gdevicetype = deviceType;
            //gbaudrate = baudRate;
            //gdeviceIndex = deviceInd;
            //gchannel = canChannel;

            volt[0] = textBox1;
            volt[1] = textBox2;
            volt[2] = textBox3;
            volt[3] = textBox4;
            volt[4] = textBox5;
            volt[5] = textBox6;
            volt[6] = textBox7;
            volt[7] = textBox8;
            volt[8] = textBox9;
            volt[9] = textBox10;

            volt[10] = textBox11;
            volt[11] = textBox12;
            volt[12] = textBox13;
            volt[13] = textBox14;
            volt[14] = textBox15;
            volt[15] = textBox16;
            volt[16] = textBox17;
            volt[17] = textBox18;
            volt[18] = textBox19;
            volt[19] = textBox20;

            volt[20] = textBox21;
            volt[21] = textBox22;
            volt[22] = textBox23;
            volt[23] = textBox24;
            volt[24] = textBox25;
            volt[25] = textBox26;
            volt[26] = textBox27;
            volt[27] = textBox28;
            volt[28] = textBox29;
            volt[29] = textBox30;

            volt[30] = textBox31;
            volt[31] = textBox32;
            volt[32] = textBox33;
            volt[33] = textBox34;
            volt[34] = textBox35;
            volt[35] = textBox36;
            volt[36] = textBox37;
            volt[37] = textBox38;
            volt[38] = textBox39;
            volt[39] = textBox40;

            volt[40] = textBox41;
            volt[41] = textBox42;
            volt[42] = textBox43;
            volt[43] = textBox44;
            volt[44] = textBox45;
            volt[45] = textBox46;
            volt[46] = textBox47;
            volt[47] = textBox48;
            volt[48] = textBox49;
            volt[49] = textBox50;


            volt[50] = textBox51;
            volt[51] = textBox52;
            volt[52] = textBox53;
            volt[53] = textBox54;
            volt[54] = textBox55;
            volt[55] = textBox56;
            volt[56] = textBox57;
            volt[57] = textBox58;
            volt[58] = textBox59;
            volt[59] = textBox60;

            ReceiveCanMsgThread = new Thread(RecvCanFunction);
            ReceiveCanMsgThread.Start();

        }


        /// <summary>
        /// RecvCanMsgFromCanDevice
        /// </summary>
        public void RecvCanFunction()
        {
            while (true)
            {
                Thread.Sleep(300);

                for (int i = 0; i < 60; i++)
                {

                    if (i  < CellNum)
                    {
                        volt[i].Text = Global.slaveVolt[gslaveNo, i].ToString();
                    }
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;

            if (count >= 5)
            {
                ReceiveCanMsgThread.Abort();
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
