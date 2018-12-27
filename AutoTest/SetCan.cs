using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoTest
{
    public partial class SetCan : Form
    {
        public uint gdevicetype = 0;
        public uint gbaudrate = 0;
        public uint gdeviceIndex = 0;
        public uint gchannel = 0;

        private static SetCan f1;

        public static SetCan GetSingleInstance()
        {
            if (f1 == null || f1.IsDisposed)
            {
                f1 = new SetCan();
            }
            return f1;
        }

        public SetCan()
        {
            InitializeComponent();

        }

        private void SetCan_Load(object sender, EventArgs e)
        {
            switch (ProjectGlobal.zlgCan.gdevicetype)
            {
                case 21:
                    {
                        deviceType.SelectedIndex = 0;
                        break;
                    }
                case 4:
                    {
                        deviceType.SelectedIndex = 1;
                        break;
                    }
                case 14:
                    {
                        deviceType.SelectedIndex = 2;
                        break;
                    }
            }

            deviceIndex.SelectedIndex =(int) ProjectGlobal.zlgCan.gdeviceIndex;
            chan.SelectedIndex = (int)ProjectGlobal.zlgCan.gchannel;
            switch (ProjectGlobal.zlgCan.gbaudrate)
            {
                case 500:
                    {
                        baudRate.SelectedIndex = 0;
                        break;
                    }
                case 250:
                    {
                        baudRate.SelectedIndex = 1;
                        break;
                    }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
          switch (deviceType.SelectedIndex)
            {
                case 0:
                    {
                        gdevicetype = 21;
                        break;
                    }
                case 1:
                    {
                         gdevicetype = 4;
                        break;
                    }
                case 2:
                    {
                         gdevicetype = 14;
                        break;
                    }
            }

           switch (baudRate.SelectedIndex)
            {
                case 0:
                    {
                        gbaudrate = 500;
                        break;
                    }
                case 1:
                    {
                        gbaudrate = 250;
                        break;
                    }
            }


            gdeviceIndex =(uint)deviceIndex.SelectedIndex;
            gchannel = (uint)chan.SelectedIndex;


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }



    }
}
