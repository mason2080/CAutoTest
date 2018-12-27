using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Driver.File.Ini;
using System.Data;
using System.Security.Cryptography;
using Driver.File.Txt;


namespace BHN02_BMS_Monitor
{
    public partial class Config : Form
    {
        IniFileClass iniFile = new IniFileClass();
        public delegate void FormCloseHandler();
        public event FormCloseHandler FormClose;

        Thread oneKeySetThread,oneKeyReadThread;
        delegate void Uidelegate();
        Uidelegate UpdateUi;

        enum BmuStep
        {
            SetBmuNo = 1,
            SetVoltNumber,
            SetTempNumber,
            SetChanVolNum,
            SetTemControl,
            SetFuseChan,
            SetLevel1,
            SetLevel2,
            SetLevel3,
            Set2dCode,

            ReadBmuNo,
            ReadVoltNumber,
            ReadTempNumber,
            ReadChanVolNum,
            ReadTemControl,
            ReadFuseChan,
            ReadLevel1,
            ReadLevel2,
            ReadLevel3,
            Read2dCode
        }

        enum BcuStep
        {
            SetBmuNo = 30,
            SetVoltNumber,
            SetTempNumber,
            SetChanVolNum,
            SetTemControl,
            SetFuseChan,
            SetLevel1,
            SetLevel2,
            SetLevel3,
            Set2dCode,
            SetChgInfo,
            SetSoc,
            SetSoh,
            SetAh,
            SetChgAh,
            SetCarNumber,
            SetConfigVersion,
            SetProductNo,
            SetClock,
            SetCellNum,
            SetChgTimes,
            SetBatInfo,

            ReadBmuNo,
            ReadVoltNumber,
            ReadTempNumber,
            ReadChanVolNum,
            ReadTemControl,
            ReadFuseChan,
            ReadLevel1,
            ReadLevel2,
            ReadLevel3,
            Read2dCode,
            ReadChgInfo,
            ReadSoc,
            ReadSoh,
            ReadAh,
            ReadChgAh,
            ReadCarNumber,
            ReadConfigVersion,
            ReadProductNo,
            ReadClock,
            ReadCellNum,
            ReadChgTimes,
            ReadBatInfo,
        }

        struct BmuConfigData
        {
            public byte voltNum;
            public byte tempNum;
            public byte[] fuseChannel;
            public byte[] fuseLocation;
            public byte[] chanVolNum;
            public Int16 fanOnTemp;
            public Int16 fanOffTemp;
            public Int16 fanOffDiff;
            public Int16 fanOnDiff;
            public Int16 heatOnTemp;
            public Int16 heatOffTemp;
            public Int16 heatOffDiff;

            public double[] voltHith;
            public double[] voltLow;
            public Int16[] tempHigh;
            public Int16[] tempLow;
            public double[] voltDiff;
            public Int16[] tempDiff;


        }

        struct BcuConfigData
        {
            public byte slaveNum;
            public double ah;
            public double realAh;
            public double soc;
            public double soh;
        }

        BcuConfigData bcuConfigData = new BcuConfigData();
        BmuConfigData[] bmuConfigData = new BmuConfigData[30];


        DataTable tempDataTable = new DataTable();
        DataTable tempDataTableRead = new DataTable();

        byte OneKeySetBcuStepNo = 0;
        byte OneKeyReadBcuStepNo = 0;
        bool OneKeySetFlag= false;
        bool cellReadOkFlag = false;
        bool ForceStop = false;

        Welcome sForm = null;


        private static Config f1;

        public static Config GetSingleInstance()
        {
            if (f1 == null || f1.IsDisposed)
            {
                f1 = new Config();
            }
            return f1;
        }


        public void OnFormClose()
        {
            
        }

        private void UpdateUiFun()
        {
            tabBcu.SelectedIndex = 1;
        }
        private void Config_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            ProjectGlobal.zlgCan.Can_InitChanWithFilter(500);
            textBoardType.Text = "BCU";

            btnOneKeySet.Visible = false;
            UpdateUi = new Uidelegate(UpdateUiFun);

            for (byte j = 0; j < 30; j++)
            {
                bmuConfigData[j].fuseChannel = new byte[10];
                bmuConfigData[j].fuseLocation = new byte[10];
                bmuConfigData[j].chanVolNum = new byte[5];
                bmuConfigData[j].voltHith = new double[3];
                bmuConfigData[j].voltLow = new double[3];
                bmuConfigData[j].tempHigh = new Int16[3];
                bmuConfigData[j].tempLow = new Int16[3];
                bmuConfigData[j].voltDiff = new double[3];
                bmuConfigData[j].tempDiff = new Int16[3];
            }

            tempDataTable.Columns.Clear();
            tempDataTable.Rows.Clear();
            tempDataTable.Columns.Add("板号");
            tempDataTable.Columns.Add("电压数");
            tempDataTable.Columns.Add("温度数");

            for (byte j = 0; j < 30; j++)
            {
                DataRow dr = tempDataTable.NewRow();
                dr["板号"] = "";
                dr["电压数"] = "";
                dr["温度数"] = "";
                tempDataTable.Rows.Add(dr);
            }
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.DataSource = tempDataTable;
            dataGridView1.Columns[0].Width = 35;
            dataGridView1.Columns[1].Width = 35;
            dataGridView1.Columns[2].Width = 40;

            ///////////////////////////////////////////////////////////////////
            tempDataTableRead.Columns.Clear();
            tempDataTableRead.Rows.Clear();
            tempDataTableRead.Columns.Add("板号");
            tempDataTableRead.Columns.Add("电压数");
            tempDataTableRead.Columns.Add("温度数");

            for (byte j = 0; j < 30; j++)
            {
                DataRow dr = tempDataTableRead.NewRow();

                dr["板号"] = "";
                dr["电压数"] = "";
                dr["温度数"] = "";

                tempDataTableRead.Rows.Add(dr);
            }

            dataGridView2.ColumnHeadersVisible = true;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.DataSource = tempDataTableRead;
            dataGridView2.Columns[0].Width = 35;
            dataGridView2.Columns[1].Width = 35;
            dataGridView2.Columns[2].Width = 40;

        }
        public Config()
        {

            Thread showSplashThread = new Thread(new ThreadStart(ShowSplash));
            showSplashThread.Start();
            InitializeComponent();
            //System.Threading.Thread.Sleep(2000);//欢迎窗口停留时间2s 
            showSplashThread.Abort();
            sForm.Close();
        }

        private void ShowSplash()
        {
           
            try
            {
                sForm = new Welcome();
                sForm.ShowDialog();
            }
            catch (ThreadAbortException e)
            {
            }
        }
// Thread was aborted normally if (_log.IsDebugEnabled) {  _log.Debug(“Splash window was aborted normally: ” + e.Message); } }  finally {  sForm = null; } } 
        private void button1_Click(object sender, EventArgs e)
        {
            string temp;
            string[] num = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            openFileDialog1.Filter = "项目配置文件|*.ini";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog1.FileName;
                textConfigPath.Text = fName;
                if (File.Exists(fName))
                {
                    string md5RealCode = iniFile.IniReadValue(fName, "总信息", "MD5码");


                    File.Copy(fName,@"D:\temp.ini",true );
                    iniFile.IniWriteValue(@"D:\temp.ini", "总信息", "MD5码", " \"MD5码\"");
                    byte[] data1=File.ReadAllBytes(@"D:\temp.ini");
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] data = md5.ComputeHash(data1,0,data1.Length-2);
                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    if (md5RealCode != sBuilder.ToString())
                    {
                        MessageBox.Show("Md5码错误，文件损坏或改动");
                        return;
                    }


                    this.textWBmuNumBcu.TextChanged -= new EventHandler(textWBmuNumBcu_TextChanged);
                    textProject.Text = iniFile.IniReadValue(fName, "总信息", "工程代号");

                    textWBmuNumBcu.Text = iniFile.IniReadValue(fName, "主板", "从板个数");
                    textWAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池额定容量");
                    textWRealAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池实际容量");
                    textWSocBcu.Text = iniFile.IniReadValue(fName, "主板", "SOC");
                    textWSohBcu.Text = iniFile.IniReadValue(fName, "主板", "SOH");
                    textWProNoBcu.Text = iniFile.IniReadValue(fName, "主板", "产品编号");
                    textWCarNoBcu.Text = iniFile.IniReadValue(fName, "主板", "车架(牌)号");
                    textWConfigVerBcu.Text = iniFile.IniReadValue(fName, "主板", "配置文件版本");

                    if (byte.Parse(textWBmuNumBcu.Text) > 0)
                    {

                        for (byte jj = 0; jj < 30; jj++)
                        {

                            if (jj < byte.Parse(textWBmuNumBcu.Text))
                            {
                                dataGridView1.Rows[jj].Cells[0].Value = (jj + 1).ToString();
                                temp = "从板" + (jj + 1).ToString() + "电池节数";
                                dataGridView1.Rows[jj].Cells[1].Value = iniFile.IniReadValue(fName, "主板", temp);
                                temp = "从板" + (jj + 1).ToString() + "温度个数";
                                dataGridView1.Rows[jj].Cells[2].Value = iniFile.IniReadValue(fName, "主板", temp);
                            }
                            else
                            {
                                dataGridView1.Rows[jj].Cells[0].Value = "";
                                dataGridView1.Rows[jj].Cells[1].Value = "";
                                dataGridView1.Rows[jj].Cells[2].Value = "";
                            }
                        }
                    }
                    this.textWBmuNumBcu.TextChanged += new EventHandler(textWBmuNumBcu_TextChanged);

                    textWSumvHBcu1.Text = iniFile.IniReadValue(fName, "主板", "总电压过高一级");
                    textWSumvLBcu1.Text = iniFile.IniReadValue(fName, "主板", "总电压过低一级 ");
                    textWTempHBcu1.Text = iniFile.IniReadValue(fName, "主板", "温度过高一级");
                    textWTempLBcu1.Text = iniFile.IniReadValue(fName, "主板", "温度过低一级");
                    textWAVUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡一级");
                    textWSVUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡一级");
                    textWATUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡一级");
                    textWSTUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡一级");
                    textWCellVHBcu1.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高一级");
                    textWCellVLBcu1.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低一级");
                    textWChgCurHBcu1.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高一级");
                    textWdChgCurHBcu1.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高一级");
                    textWSocHBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOC过高一级");
                    textWSocLBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOC过低一级");
                    textWResHBcu1.Text = iniFile.IniReadValue(fName, "主板", "内阻过大一级");
                    textWPoleTHBcu1.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高一级");
                    textWSohHBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOH过低一级");

                    textWSumvHBcu2.Text = iniFile.IniReadValue(fName, "主板", "总电压过高二级");
                    textWSumvLBcu2.Text = iniFile.IniReadValue(fName, "主板", "总电压过低二级 ");
                    textWTempHBcu2.Text = iniFile.IniReadValue(fName, "主板", "温度过高二级");
                    textWTempLBcu2.Text = iniFile.IniReadValue(fName, "主板", "温度过低二级");
                    textWAVUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡二级");
                    textWSVUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡二级");
                    textWATUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡二级");
                    textWSTUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡二级");
                    textWCellVHBcu2.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高二级");
                    textWCellVLBcu2.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低二级");
                    textWChgCurHBcu2.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高二级");
                    textWdChgCurHBcu2.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高二级");
                    textWSocHBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOC过高二级");
                    textWSocLBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOC过低二级");
                    textWResHBcu2.Text = iniFile.IniReadValue(fName, "主板", "内阻过大二级");
                    textWPoleTHBcu2.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高二级");
                    textWSohHBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOH过低二级");

                    textWSumvHBcu3.Text = iniFile.IniReadValue(fName, "主板", "总电压过高三级");
                    textWSumvLBcu3.Text = iniFile.IniReadValue(fName, "主板", "总电压过低三级 ");
                    textWTempHBcu3.Text = iniFile.IniReadValue(fName, "主板", "温度过高三级");
                    textWTempLBcu3.Text = iniFile.IniReadValue(fName, "主板", "温度过低三级");
                    textWAVUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡三级");
                    textWSVUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡三级");
                    textWATUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡三级");
                    textWSTUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡三级");
                    textWCellVHBcu3.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高三级");
                    textWCellVLBcu3.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低三级");
                    textWChgCurHBcu3.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高三级");
                    textWdChgCurHBcu3.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高三级");
                    textWSocHBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOC过高三级");
                    textWSocLBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOC过低三级");
                    textWResHBcu3.Text = iniFile.IniReadValue(fName, "主板", "内阻过大三级");
                    textWPoleTHBcu3.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高三级");
                    textWSohHBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOH过低三级");

                    textWFanOnTempBcu.Text = iniFile.IniReadValue(fName, "主板", "风机开启温度");
                    textWFanOffTempBcu.Text = iniFile.IniReadValue(fName, "主板", "风机关闭温度");
                    textWFanOnDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "风机开启温差");
                    textWFanOffDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "风机关闭温差");
                    textWHeatOnTempBcu.Text = iniFile.IniReadValue(fName, "主板", "加热开启温度");
                    textWHeatOffTempBcu.Text = iniFile.IniReadValue(fName, "主板", "加热关闭温度");
                    textWHeatOffDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "加热关闭温差");

                    textWChgMaxCellV.Text = iniFile.IniReadValue(fName, "主板", "最大允许单体电压");
                    textWChgMinCellV.Text = iniFile.IniReadValue(fName, "主板", "最小允许单体电压");
                    textWChgMaxCurr.Text = iniFile.IniReadValue(fName, "主板", "最大允许充电电流");
                    textWChgMinCurr.Text = iniFile.IniReadValue(fName, "主板", "最小允许充电电流");
                    textWChgMaxSumv.Text = iniFile.IniReadValue(fName, "主板", "最大允许总电压");
                    textWChgMaxt.Text = iniFile.IniReadValue(fName, "主板", "最大允许充电温度");
                    textWChgMint.Text = iniFile.IniReadValue(fName, "主板", "最小允许充电温度");
                    textWChgStep.Text = iniFile.IniReadValue(fName, "主板", "电流步长");

                    textWBatNumBcu.Text = iniFile.IniReadValue(fName, "主板", "电池串联");
                    textWBatParBcu.Text = iniFile.IniReadValue(fName, "主板", "电池并联");
                    textWRateAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定容量");
                    textWBatEnergyBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定能量(kWh)");
                    textWBatRealAhBcu.Text = iniFile.IniReadValue(fName, "主板", "实际容量");
                    textWRateSumvBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定总电压");
                    switch (iniFile.IniReadValue(fName, "主板", "电池类型"))
                    {
                        case "磷酸铁锂":
                            {
                                comboBoxWBatType.SelectedIndex=0;
                                break;
                            }
                        case "锰酸锂":
                            {
                                comboBoxWBatType.SelectedIndex = 1;
                                break;
                            }
                        case "三元材料":
                            {
                                comboBoxWBatType.SelectedIndex = 2;
                                break;
                            }
                        case "铅酸":
                            {
                                comboBoxWBatType.SelectedIndex = 3;
                                break;
                            }
                        case "镍氢":
                            {
                                comboBoxWBatType.SelectedIndex = 4;
                                break;
                            }
                        case "钴酸锂":
                            {
                                comboBoxWBatType.SelectedIndex = 5;
                                break;
                            }
                        case "聚合物锂离子":
                            {
                                comboBoxWBatType.SelectedIndex = 6;
                                break;
                            }
                        case "钛酸锂":
                            {
                                comboBoxWBatType.SelectedIndex = 7;
                                break;
                            }
                        case "其他电池":
                            {
                                comboBoxWBatType.SelectedIndex = 8;
                                break;
                            }
                    }

                    textWInAhBcu.Text = iniFile.IniReadValue(fName, "主板", "累计充电总安时");
                    textWOutAhBcu.Text = iniFile.IniReadValue(fName, "主板", "累计放电总安时");
                    textWChgTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池放空次数");
                    textWFullTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池充满次数");
                    textWEmptyTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池充电次数");

                   

                    /////////////////////////////////////////////////////////////
                    try{
                        for (byte i = 0; i < byte.Parse(textWBmuNumBcu.Text); i++)
                        {
                            temp = "从板" + (i + 1).ToString();
                            bmuConfigData[i].voltNum = byte.Parse(iniFile.IniReadValue(fName, temp, "电压个数"));
                            bmuConfigData[i].tempNum = byte.Parse(iniFile.IniReadValue(fName, temp, "温度个数"));

                            for (byte j = 0; j < 10; j++)
                            {
                                bmuConfigData[i].fuseChannel[j] = byte.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "第" + num[j] + "个保险通道"));
                                bmuConfigData[i].fuseLocation[j] = byte.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "第" + num[j] + "个保险位置"));
                            }

                            for (byte j = 0; j < 5; j++)
                            {
                                bmuConfigData[i].chanVolNum[j] = byte.Parse(iniFile.IniReadValue(fName, ("从板" + (i + 1).ToString()), ("第" + (j + 1).ToString() + "通道电压个数")));
                            }

                            for (byte j = 0; j < 3; j++)
                            {
                                bmuConfigData[i].voltHith[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压过高" + num[j] + "级"));
                                bmuConfigData[i].voltLow[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压过低" + num[j] + "级"));
                                bmuConfigData[i].tempHigh[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度过高" + num[j] + "级"));
                                bmuConfigData[i].tempLow[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度过低" + num[j] + "级"));
                                bmuConfigData[i].voltDiff[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压不均衡" + num[j] + "级"));
                                bmuConfigData[i].tempDiff[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度不均衡" + num[j] + "级"));
                            }

                            bmuConfigData[i].fanOnTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机开启温度"));
                            bmuConfigData[i].fanOnDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机开启温差"));
                            bmuConfigData[i].fanOffTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机关闭温度"));
                            bmuConfigData[i].fanOffDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机关闭温差"));
                            bmuConfigData[i].heatOnTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热开启温度"));
                            bmuConfigData[i].heatOffTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热关闭温度"));
                            bmuConfigData[i].heatOffDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热关闭温差"));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("从板参数异常:存在空值\r\n 请检查配置文件！！");
                        return;
                    }

                    }

                    MessageBox.Show("配置文件导入成功，文件完整性检测通过");
                    btnOneKeySet.Visible = true;
                    showBmuConfigData(byte.Parse(textCurrBmuNo.Text));

            }
        }

        private void showBmuConfigData(byte bmuNo)
        {
            textWVolNum.Text = bmuConfigData[bmuNo - 1].voltNum.ToString();
            textWTempNum .Text= bmuConfigData[bmuNo - 1].tempNum.ToString();

            textWVolNumChan1.Text = bmuConfigData[bmuNo - 1].chanVolNum[0].ToString();
            textWVolNumChan2.Text = bmuConfigData[bmuNo - 1].chanVolNum[1].ToString();
            textWVolNumChan3.Text = bmuConfigData[bmuNo - 1].chanVolNum[2].ToString();
            textWVolNumChan4.Text = bmuConfigData[bmuNo - 1].chanVolNum[3].ToString();
            textWVolNumChan5.Text = bmuConfigData[bmuNo - 1].chanVolNum[4].ToString();

            textFanOnTemp.Text = bmuConfigData[bmuNo - 1].fanOnTemp.ToString();
            textFanOffTemp.Text = bmuConfigData[bmuNo - 1].fanOffTemp.ToString();
            textFanOnDiff.Text = bmuConfigData[bmuNo - 1].fanOnDiff.ToString();
            textFanOffDiff.Text = bmuConfigData[bmuNo - 1].fanOffDiff.ToString();
            textHeatOnTemp.Text = bmuConfigData[bmuNo - 1].heatOnTemp.ToString();
            textHeatOffTemp.Text = bmuConfigData[bmuNo - 1].heatOffTemp.ToString();
            textHeatOffDiff.Text = bmuConfigData[bmuNo - 1].heatOffDiff.ToString();

            textWFuseChan1.Text = bmuConfigData[bmuNo - 1].fuseChannel[0].ToString();
            textWFuseChan2.Text = bmuConfigData[bmuNo - 1].fuseChannel[1].ToString();
            textWFuseChan3.Text = bmuConfigData[bmuNo - 1].fuseChannel[2].ToString();
            textWFuseChan4.Text = bmuConfigData[bmuNo - 1].fuseChannel[3].ToString();
            textWFuseChan5.Text = bmuConfigData[bmuNo - 1].fuseChannel[4].ToString();
            textWFuseChan6.Text = bmuConfigData[bmuNo - 1].fuseChannel[5].ToString();
            textWFuseChan7.Text = bmuConfigData[bmuNo - 1].fuseChannel[6].ToString();
            textWFuseChan8.Text = bmuConfigData[bmuNo - 1].fuseChannel[7].ToString();
            textWFuseChan9.Text = bmuConfigData[bmuNo - 1].fuseChannel[8].ToString();
            textWFuseChan10.Text = bmuConfigData[bmuNo - 1].fuseChannel[9].ToString();

            textWFuseLoc1.Text = bmuConfigData[bmuNo - 1].fuseLocation[0].ToString();
            textWFuseLoc2.Text = bmuConfigData[bmuNo - 1].fuseLocation[1].ToString();
            textWFuseLoc3.Text = bmuConfigData[bmuNo - 1].fuseLocation[2].ToString();
            textWFuseLoc4.Text = bmuConfigData[bmuNo - 1].fuseLocation[3].ToString();
            textWFuseLoc5.Text = bmuConfigData[bmuNo - 1].fuseLocation[4].ToString();
            textWFuseLoc6.Text = bmuConfigData[bmuNo - 1].fuseLocation[5].ToString();
            textWFuseLoc7.Text = bmuConfigData[bmuNo - 1].fuseLocation[6].ToString();
            textWFuseLoc8.Text = bmuConfigData[bmuNo - 1].fuseLocation[7].ToString();
            textWFuseLoc9.Text = bmuConfigData[bmuNo - 1].fuseLocation[8].ToString();
            textWFuseLoc10.Text = bmuConfigData[bmuNo - 1].fuseLocation[9].ToString();

            textWVolHigh1.Text = bmuConfigData[bmuNo - 1].voltHith[0].ToString();
            textWVolLow1.Text = bmuConfigData[bmuNo - 1].voltLow[0].ToString();
            textWVolUnbal1.Text = bmuConfigData[bmuNo - 1].voltDiff[0].ToString();
            textWTempHigh1.Text = bmuConfigData[bmuNo - 1].tempHigh[0].ToString();
            textWTempLow1.Text = bmuConfigData[bmuNo - 1].tempLow[0].ToString();
            textWTempUnbal1.Text = bmuConfigData[bmuNo - 1].tempDiff[0].ToString();

            textWVolHigh2.Text = bmuConfigData[bmuNo - 1].voltHith[1].ToString();
            textWVolLow2.Text = bmuConfigData[bmuNo - 1].voltLow[1].ToString();
            textWVolUnbal2.Text = bmuConfigData[bmuNo - 1].voltDiff[1].ToString();
            textWTempHigh2.Text = bmuConfigData[bmuNo - 1].tempHigh[1].ToString();
            textWTempLow2.Text = bmuConfigData[bmuNo - 1].tempLow[1].ToString();
            textWTempUnbal2.Text = bmuConfigData[bmuNo - 1].tempDiff[1].ToString();

            textWVolHigh3.Text = bmuConfigData[bmuNo - 1].voltHith[2].ToString();
            textWVolLow3.Text = bmuConfigData[bmuNo - 1].voltLow[2].ToString();
            textWVolUnbal3.Text = bmuConfigData[bmuNo - 1].voltDiff[2].ToString();
            textWTempHigh3.Text = bmuConfigData[bmuNo - 1].tempHigh[2].ToString();
            textWTempLow3.Text = bmuConfigData[bmuNo - 1].tempLow[2].ToString();
            textWTempUnbal3.Text = bmuConfigData[bmuNo - 1].tempDiff[2].ToString();

        }

        private void btnWVoltNum_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();

            configData.len = 4;
            configData.cmd = 1;
            configData.data = data;
            configData.id = (uint)(0x18200091+ byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            try
            {
                data[0] = 0;
                data[1] = byte.Parse(textWVolNum.Text);

                configData.setStep = (byte)BmuStep.SetVoltNumber;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }

        }

        private void btnWTempNum_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];
            ConfigData configData = new ConfigData();
            configData.len = 4;
            configData.cmd = 4;
            configData.data = data;
            configData.id = (uint)(0x18200091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            try
            {
                data[0] = 0;
                data[1] = byte.Parse(textWTempNum.Text);

                configData.setStep = (byte)BmuStep.SetTempNumber;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWChanVoltNum_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 20;
            configData.cmd = 2;
            configData.data = data;
            configData.id = (uint)(0x18200091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;

            try
            {
                data[0] = byte.Parse(textWVolNumChan1.Text);
                data[1] = byte.Parse(textWVolNumChan2.Text);
                data[2] = byte.Parse(textWVolNumChan3.Text);
                data[3] = byte.Parse(textWVolNumChan4.Text);
                data[4] = byte.Parse(textWVolNumChan5.Text);

                configData.setStep = (byte)BmuStep.SetChanVolNum;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void Config_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (oneKeySetThread.ThreadState == ThreadState.Running)
                {
                    oneKeySetThread.Abort();
                }
            }
            catch { }
            FormClose();

        }

        unsafe private void backgroundWorkerSingle_DoWork(object sender, DoWorkEventArgs e)
        {

            for (byte times = 0; times < 5; times++)//retry times
            {


                ConfigData configData = (ConfigData)(e.Argument);

                VCI_CAN_OBJ[] recvCanMsgArray = new VCI_CAN_OBJ[10];

                byte[] sendData = new byte[8];
                uint idRecv = (configData.id & 0xffff0000) + (configData.id & 0x000000ff) * 256 + (configData.id & 0x0000ff00) / 256;

                sendData[0] = configData.len;
                sendData[1] = configData.cmd;
                for (byte i = 0; i < 6; i++)
                {
                    sendData[i + 2] = configData.data[i];
                }
                ProjectGlobal.zlgCan.CanClearBuffer();

                ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);

                uint recvNum = 0;
                for (byte j = 0; j < 50; j++)
                {
                    recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                    if (recvNum > 0)
                    {
                        byte i = 0;
                        for (i = 0; i < recvNum; i++)
                        {
                            fixed (VCI_CAN_OBJ* rc1 = &recvCanMsgArray[i])
                            {
                                if ((rc1->ID == idRecv))//&& ((rc1->Data[0]) == sendData[0]) && (rc1->Data[1] == sendData[1]))
                                {
                                    if (((rc1->Data[2]) == 0) && (rc1->Data[3] == 0))
                                    {
                                        switch (configData.setStep)
                                        {
                                            case (byte)BmuStep.SetVoltNumber:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BMU 电压数设置成功\r\n";
                                                    break;
                                                }
                                            case (byte)BmuStep.SetTempNumber:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BMU 温度数设置成功\r\n";
                                                    break;
                                                }
                                            case (byte)BmuStep.SetBmuNo:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BMU 板号设置成功\r\n";
                                                    break;
                                                }

                                            case (byte)BcuStep.SetSoc:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BCU SOC设置成功\r\n";
                                                    break;
                                                }
                                            case (byte)BcuStep.SetClock:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BCU 时钟设置成功\r\n";
                                                    break;
                                                }
                                            case (byte)BcuStep.SetSoh:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BCU SOH设置成功\r\n";
                                                    break;
                                                }
                                            case (byte)BcuStep.SetChgTimes:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textConfigInfo.Text += "BCU 充电次数设置成功\r\n";
                                                    break;
                                                }


                                            //case (byte)BcuStep.SetClock:
                                            //    {
                                            //      //  ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                            //         textConfigInfo.Text += "BCU 时钟设置成功\r\n";
                                            //         break;
                                            //    }


                                        }
                                        // OneKeySetBcuStepNo++;
                                        return;
                                    }
                                }
                            }
                        }

                    }
                }
                switch (configData.setStep)//Result Control
                {
                    case (byte)BmuStep.SetVoltNumber:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "电压个数设置失败\r\n";
                            break;
                        }
                    case (byte)BmuStep.SetTempNumber:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "电压个数设置失败\r\n";
                            break;
                        }
                    case (byte)BmuStep.SetBmuNo:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BMU板设置失败\r\n";
                            break;
                        }
                    case (byte)BcuStep.SetSoc:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU SOC设置失败\r\n";
                            break;
                        }
                    case (byte)BcuStep.SetSoh:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU SOH设置失败\r\n";
                            break;
                        }
                    case (byte)BcuStep.SetClock:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU 时钟设置失败\r\n";
                            break;
                        }
                    case (byte)BcuStep.SetChgTimes:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU 充电次数设置失败\r\n";
                            break;
                        }


                }
                // OneKeySetBcuStepNo++;
                return;
            }
        }
        /// <summary>
        ///   Step 1:send Start Frame
        ///   Step 2:receive flow control frame
        ///   Step 3:send other frame
        ///   Step 4:receive result frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        unsafe private void backgroundWorkerMulti_DoWork(object sender, DoWorkEventArgs e)
        {
            ConfigData configData = (ConfigData)(e.Argument);
            VCI_CAN_OBJ[] recvCanMsgArray = new VCI_CAN_OBJ[10];
            byte[] sendData = new byte[8];
            uint idRecv = (configData.id & 0xffff0000) + (configData.id & 0x000000ff) * 256 + (configData.id & 0x0000ff00) / 256;
            byte sendLength;
            byte sendSerialNo;

            for (byte times = 0; times < configData.retryTimes; times++)//retry times
            {
                #region //send start frame
                sendData[0] = 0x10;
                sendData[1] = configData.len;
                sendData[2] = configData.cmd;
                for (byte i = 0; i < 5; i++)
                {
                    sendData[i + 3] = configData.data[i];
                }
                ProjectGlobal.zlgCan.CanClearBuffer();
                ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);
                sendLength = 5;//Already send 5 bytes
                #endregion
                #region//receive flow control frame
                uint recvNum = 0;

                for (byte j = 0; j < 50; j++)
                {
                    recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                    if (recvNum > 0)
                    {
                        for (byte i = 0; i < recvNum; i++)
                        {
                            fixed (VCI_CAN_OBJ* rc1 = &recvCanMsgArray[i])
                            {
                                if ((rc1->ID == idRecv))//&& ((rc1->Data[0]) == sendData[0]) && (rc1->Data[1] == sendData[1]))
                                {
                #endregion
                                    #region//send other frame
                                    if ((rc1->Data[0]) == 0x30)
                                    {
                                        sendSerialNo = 1;

                                        while (sendLength < configData.len)
                                        {
                                            sendData[0] = (byte)(0x20 + sendSerialNo);
                                            sendSerialNo++;

                                            for (byte k = 0; k < 7; k++)
                                            {
                                                if ((sendLength + k) < configData.len - 1)
                                                {
                                                    sendData[k + 1] = configData.data[sendLength + k];
                                                }
                                            }
                                            ProjectGlobal.zlgCan.CanClearBuffer();
                                            ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);

                                            sendLength += 7;

                                            Thread.Sleep(20);
                                        }
                                    #endregion
                                        #region //receive result frame
                                        recvNum = 0;

                                        for (byte t = 0; t < 50; t++)
                                        {
                                            recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                                            if (recvNum > 0)
                                            {
                                                for (byte l = 0; l < recvNum; l++)
                                                {
                                                    fixed (VCI_CAN_OBJ* rc2 = &recvCanMsgArray[i])
                                                    {
                                                        if ((rc2->ID == idRecv) && (rc2->Data[2] == 0) && (rc1->Data[3] == 0))
                                                        {

                                                            switch (configData.setStep)
                                                            {
                                                                case (byte)BmuStep.SetChanVolNum:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "通道电压数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.SetFuseChan:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "保险通道设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.SetTemControl:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "温度参数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.SetLevel1:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "一级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.SetLevel2:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "二级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.SetLevel3:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "三级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BmuStep.Set2dCode:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BMU二维码设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.Set2dCode:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 二维码设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetLevel1:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 一级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetLevel2:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 二级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetLevel3:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 三级故障设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetChgInfo:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 充电参数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetTemControl:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 加热参数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetAh:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 安时数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetCarNumber:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 车牌号设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetProductNo:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 产品编号设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetConfigVersion:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 配置版本设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetVoltNumber:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 从板电池数设置成功\r\n";
                                                                        break;
                                                                    }

                                                                case (byte)BcuStep.SetTempNumber:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 从板温度数设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetChgAh:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 充放电安时设置成功\r\n";
                                                                        break;
                                                                    }
                                                                case (byte)BcuStep.SetBatInfo:
                                                                    {
                                                                        ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                        textConfigInfo.Text += "BCU 电池信息设置成功\r\n";
                                                                        break;
                                                                    }
                                                            }
                                                            //  OneKeySetBcuStepNo++;
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                }
            }

            switch (configData.setStep)
            {
                case (byte)BmuStep.SetChanVolNum:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 通道电压个数设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.SetFuseChan:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 保险通道设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.SetTemControl:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 温度控制参数设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.SetLevel1:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 一级故障参数设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.SetLevel2:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 二级故障参数设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.SetLevel3:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 三级故障参数设置失败\r\n";
                        break;
                    }
                case (byte)BmuStep.Set2dCode:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 二维码设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.Set2dCode:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 二维码设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetLevel1:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 一级故障设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetLevel2:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 二级故障设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetLevel3:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 三级故障设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetChgInfo:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 充电参数设置失败\r\n";
                        break;
                    }

                case (byte)BcuStep.SetTemControl:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 加热参数设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetAh:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 安时数设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetCarNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 车牌号设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetProductNo:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 产品编号设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetConfigVersion:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 配置版本号设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetVoltNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 从板电池数设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetTempNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 从板温度数设置失败\r\n";
                        break;
                    }
                case (byte)BcuStep.SetChgAh:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 充放电安时设置失败\r\n";
                        break;
                    }

                case (byte)BcuStep.SetBatInfo:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 电池信息设置失败\r\n";
                        break;
                    }
            }
            //OneKeySetBcuStepNo++;
            return;
        }

        private void btnWFuseChan_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 10;
            configData.cmd = 3;
            configData.data = data;
            configData.id = (uint)(0x18200091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;

            try
            {
                data[0] = (byte)(byte.Parse(textWFuseChan1.Text) * 16 + byte.Parse(textWFuseChan1.Text));
                data[1] = (byte)(byte.Parse(textWFuseChan2.Text) * 16 + byte.Parse(textWFuseChan2.Text));
                data[2] = (byte)(byte.Parse(textWFuseChan3.Text) * 16 + byte.Parse(textWFuseChan3.Text));
                data[3] = (byte)(byte.Parse(textWFuseChan4.Text) * 16 + byte.Parse(textWFuseChan4.Text));
                data[4] = (byte)(byte.Parse(textWFuseChan5.Text) * 16 + byte.Parse(textWFuseChan5.Text));
                data[5] = (byte)(byte.Parse(textWFuseChan6.Text) * 16 + byte.Parse(textWFuseChan6.Text));
                data[6] = (byte)(byte.Parse(textWFuseChan7.Text) * 16 + byte.Parse(textWFuseChan7.Text));
                data[7] = (byte)(byte.Parse(textWFuseChan8.Text) * 16 + byte.Parse(textWFuseChan8.Text));
                data[8] = (byte)(byte.Parse(textWFuseChan9.Text) * 16 + byte.Parse(textWFuseChan9.Text));
                data[9] = (byte)(byte.Parse(textWFuseChan10.Text) * 16 + byte.Parse(textWFuseChan10.Text));
                configData.setStep = (byte)BmuStep.SetFuseChan;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWTempControl_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 7;
            configData.cmd = 5;
            configData.data = data;
            configData.id = (uint)(0x18200091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;

            int temp;
            ((Button)sender).BackColor = Color.Gray;
            try
            {

                temp = int.Parse(textFanOnTemp.Text);
                data[0] = (byte)(temp + 40);
                temp = int.Parse(textFanOffTemp.Text);
                data[1] = (byte)(temp + 40);

                data[2] = byte.Parse(textFanOnDiff.Text);
                data[3] = byte.Parse(textFanOffDiff.Text);

                temp = int.Parse(textHeatOnTemp.Text);
                data[4] = (byte)(temp + 40);

                temp = int.Parse(textHeatOffTemp.Text);
                data[5] = (byte)(temp + 40);

                data[6] = byte.Parse(textHeatOffDiff.Text);

                configData.setStep = (byte)BmuStep.SetTemControl;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevel1_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 11;
            configData.cmd = 1;
            configData.data = data;
            configData.id = (uint)(0x18220091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWVolHigh1.Text) * 1000;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWVolLow1.Text) * 1000;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = double.Parse(textWVolUnbal1.Text) * 1000;
                data[4] = (byte)(temp / 256);
                data[5] = (byte)(temp);


                temp = int.Parse(textWTempHigh1.Text);
                data[6] = (byte)(temp + 40);

                temp = int.Parse(textWTempLow1.Text);
                data[7] = (byte)(temp + 40);

                data[8] = byte.Parse(textWTempUnbal1.Text);

                data[9] = 0;
                data[10] = 0;

                configData.setStep = (byte)BmuStep.SetLevel1;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevel2_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 11;
            configData.cmd = 2;
            configData.data = data;
            configData.id = (uint)(0x18220091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWVolHigh2.Text) * 1000;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWVolLow2.Text) * 1000;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = double.Parse(textWVolUnbal2.Text) * 1000;
                data[4] = (byte)(temp / 256);
                data[5] = (byte)(temp);


                temp = int.Parse(textWTempHigh2.Text);
                data[6] = (byte)(temp + 40);

                temp = int.Parse(textWTempLow2.Text);
                data[7] = (byte)(temp + 40);

                data[8] = byte.Parse(textWTempUnbal2.Text);

                data[9] = 0;
                data[10] = 0;

                configData.setStep = (byte)BmuStep.SetLevel2;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevel3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 11;
            configData.cmd = 3;
            configData.data = data;
            configData.id = (uint)(0x18220091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWVolHigh3.Text) * 1000;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWVolLow3.Text) * 1000;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = double.Parse(textWVolUnbal3.Text) * 1000;
                data[4] = (byte)(temp / 256);
                data[5] = (byte)(temp);


                temp = int.Parse(textWTempHigh3.Text);
                data[6] = (byte)(temp + 40);

                temp = int.Parse(textWTempLow3.Text);
                data[7] = (byte)(temp + 40);

                data[8] = byte.Parse(textWTempUnbal3.Text);

                data[9] = 0;
                data[10] = 0;

                configData.setStep = (byte)BmuStep.SetLevel3;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWBmuNo_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();

            configData.len = 1;
            configData.cmd = 6;
            configData.data = data;
            configData.id = 0x18200091 + (uint)(byte.Parse(textCurrBmuNo.Text) * 256);
            try
            {
                byte[] sendData = new byte[8];

                sendData[0] = configData.len;
                sendData[1] = configData.cmd;
                sendData[2] = byte.Parse(textNewBmuNo.Text);

                btnWBmuNo.BackColor = Color.FromArgb(192, 255, 192);
                ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);
            }
            catch { }
        }

        private void btnW2dCode_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x182A0091 + (uint)(byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            try
            {

                // char[] w2dCode = textW2dCode.Text.ToArray();
                byte[] tempdata = new byte[50];
                tempdata = Encoding.Default.GetBytes(textW2dCode.Text.Substring(4, textW2dCode.TextLength - 4));

                for (byte i = 0; i < tempdata.Length; i++)
                {
                    configData.data[i] = tempdata[i];
                }
                configData.setStep = (byte)BmuStep.Set2dCode;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnBmuNoPlus_Click(object sender, EventArgs e)
        {
            if (byte.Parse(textCurrBmuNo.Text) < 30)
            {

                textCurrBmuNo.Text = (byte.Parse(textCurrBmuNo.Text) + 1).ToString();

                showBmuConfigData(byte.Parse(textCurrBmuNo.Text));
            }
        }

        private void btnBmuNoMinus_Click(object sender, EventArgs e)
        {
            if (byte.Parse(textCurrBmuNo.Text) > 1)
            {
                textCurrBmuNo.Text = (byte.Parse(textCurrBmuNo.Text) - 1).ToString();
                showBmuConfigData(byte.Parse(textCurrBmuNo.Text));
            }
        }

        private void btnW2dCodeBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x18AC0091;
            configData.objectSender = sender;
            try
            {

                byte[] tempdata = new byte[50];
                tempdata = Encoding.Default.GetBytes(textW2dCodeBcu.Text.Substring(4, textW2dCodeBcu.TextLength - 4));

                for (byte i = 0; i < tempdata.Length; i++)
                {
                    configData.data[i] = tempdata[i];
                }

                configData.setStep = (byte)BcuStep.Set2dCode;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevelBcu1_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 28;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x18a10091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWSumvHBcu1.Text) * 10;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWSumvLBcu1.Text) * 10;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = int.Parse(textWTempHBcu1.Text);
                data[4] = (byte)(temp + 40);

                temp = int.Parse(textWTempLBcu1.Text);
                data[5] = (byte)(temp + 40);

                data[6] = byte.Parse(textWATUnbaBcu1.Text);

                temp = double.Parse(textWSVUnbaBcu1.Text) * 1000;
                data[7] = (byte)(temp / 256);
                data[8] = (byte)(temp);

                temp = double.Parse(textWAVUnbaBcu1.Text) * 1000;
                data[9] = (byte)(temp / 256);
                data[10] = (byte)(temp);


                temp = double.Parse(textWCellVHBcu1.Text) * 1000;
                data[11] = (byte)(temp / 256);
                data[12] = (byte)(temp);

                temp = double.Parse(textWCellVLBcu1.Text) * 1000;
                data[13] = (byte)(temp / 256);
                data[14] = (byte)(temp);

                temp = double.Parse(textWChgCurHBcu1.Text) * 10;
                data[15] = (byte)(temp / 256);
                data[16] = (byte)(temp);

                temp = double.Parse(textWdChgCurHBcu1.Text) * 10;
                data[17] = (byte)(temp / 256);
                data[18] = (byte)(temp);

                data[19] = byte.Parse(textWSTUnbaBcu1.Text);

                temp = double.Parse(textWSocHBcu1.Text) * 10;
                data[20] = (byte)(temp / 256);
                data[21] = (byte)(temp);

                temp = double.Parse(textWSocLBcu1.Text) * 10;
                data[22] = (byte)(temp / 256);
                data[23] = (byte)(temp);

                temp = double.Parse(textWResHBcu1.Text) * 10;
                data[24] = (byte)(temp);

                temp = int.Parse(textWPoleTHBcu1.Text);
                data[25] = (byte)(temp + 40);

                temp = double.Parse(textWSohHBcu1.Text) * 10;
                data[26] = (byte)(temp / 256);
                data[27] = (byte)(temp);

                configData.setStep = (byte)BcuStep.SetLevel1;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevelBcu2_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 28;
            configData.cmd = 2;
            configData.data = data;
            configData.id = 0x18a10091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWSumvHBcu2.Text) * 10;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWSumvLBcu2.Text) * 10;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = int.Parse(textWTempHBcu2.Text);
                data[4] = (byte)(temp + 40);

                temp = int.Parse(textWTempLBcu2.Text);
                data[5] = (byte)(temp + 40);

                data[6] = byte.Parse(textWATUnbaBcu2.Text);

                temp = double.Parse(textWSVUnbaBcu2.Text) * 1000;
                data[7] = (byte)(temp / 256);
                data[8] = (byte)(temp);

                temp = double.Parse(textWAVUnbaBcu2.Text) * 1000;
                data[9] = (byte)(temp / 256);
                data[10] = (byte)(temp);


                temp = double.Parse(textWCellVHBcu2.Text) * 1000;
                data[11] = (byte)(temp / 256);
                data[12] = (byte)(temp);

                temp = double.Parse(textWCellVLBcu2.Text) * 1000;
                data[13] = (byte)(temp / 256);
                data[14] = (byte)(temp);

                temp = double.Parse(textWChgCurHBcu2.Text) * 10;
                data[15] = (byte)(temp / 256);
                data[16] = (byte)(temp);

                temp = double.Parse(textWdChgCurHBcu2.Text) * 10;
                data[17] = (byte)(temp / 256);
                data[18] = (byte)(temp);

                data[19] = byte.Parse(textWSTUnbaBcu2.Text);

                temp = double.Parse(textWSocHBcu2.Text) * 10;
                data[20] = (byte)(temp / 256);
                data[21] = (byte)(temp);

                temp = double.Parse(textWSocLBcu2.Text) * 10;
                data[22] = (byte)(temp / 256);
                data[23] = (byte)(temp);

                temp = double.Parse(textWResHBcu2.Text) * 10;
                data[24] = (byte)(temp);

                temp = int.Parse(textWPoleTHBcu2.Text);
                data[25] = (byte)(temp + 40);

                temp = double.Parse(textWSohHBcu2.Text) * 10;
                data[26] = (byte)(temp / 256);
                data[27] = (byte)(temp);

                configData.setStep = (byte)BcuStep.SetLevel2;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWLevelBcu3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 28;
            configData.cmd = 3;
            configData.data = data;
            configData.id = 0x18a10091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWSumvHBcu3.Text) * 10;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWSumvLBcu3.Text) * 10;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = int.Parse(textWTempHBcu3.Text);
                data[4] = (byte)(temp + 40);

                temp = int.Parse(textWTempLBcu3.Text);
                data[5] = (byte)(temp + 40);

                data[6] = byte.Parse(textWATUnbaBcu3.Text);

                temp = double.Parse(textWSVUnbaBcu3.Text) * 1000;
                data[7] = (byte)(temp / 256);
                data[8] = (byte)(temp);

                temp = double.Parse(textWAVUnbaBcu3.Text) * 1000;
                data[9] = (byte)(temp / 256);
                data[10] = (byte)(temp);


                temp = double.Parse(textWCellVHBcu3.Text) * 1000;
                data[11] = (byte)(temp / 256);
                data[12] = (byte)(temp);

                temp = double.Parse(textWCellVLBcu3.Text) * 1000;
                data[13] = (byte)(temp / 256);
                data[14] = (byte)(temp);

                temp = double.Parse(textWChgCurHBcu3.Text) * 10;
                data[15] = (byte)(temp / 256);
                data[16] = (byte)(temp);

                temp = double.Parse(textWdChgCurHBcu3.Text) * 10;
                data[17] = (byte)(temp / 256);
                data[18] = (byte)(temp);

                data[19] = byte.Parse(textWSTUnbaBcu3.Text);

                temp = double.Parse(textWSocHBcu3.Text) * 10;
                data[20] = (byte)(temp / 256);
                data[21] = (byte)(temp);

                temp = double.Parse(textWSocLBcu3.Text) * 10;
                data[22] = (byte)(temp / 256);
                data[23] = (byte)(temp);

                temp = double.Parse(textWResHBcu3.Text) * 10;
                data[24] = (byte)(temp);

                temp = int.Parse(textWPoleTHBcu3.Text);
                data[25] = (byte)(temp + 40);

                temp = double.Parse(textWSohHBcu3.Text) * 10;
                data[26] = (byte)(temp / 256);
                data[27] = (byte)(temp);

                configData.setStep = (byte)BcuStep.SetLevel3;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWChginfoBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 14;//其中1为cmd值
            configData.cmd = 2;
            configData.data = data;
            configData.id = 0x18a20091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWChgMaxCellV.Text) * 1000;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);

                temp = double.Parse(textWChgMaxSumv.Text) * 10;
                data[2] = (byte)(temp / 256);
                data[3] = (byte)(temp);

                temp = double.Parse(textWChgMaxCurr.Text) * 10;
                data[4] = (byte)(temp / 256);
                data[5] = (byte)(temp);


                temp = int.Parse(textWChgMint.Text);
                data[6] = (byte)(temp + 40);


                temp = double.Parse(textWChgMinCurr.Text) * 10;
                data[7] = (byte)(temp / 256);
                data[8] = (byte)(temp);

                temp = double.Parse(textWChgStep.Text) * 10;
                data[9] = (byte)(temp / 256);
                data[10] = (byte)(temp);

                temp = double.Parse(textWChgMinCellV.Text) * 1000;
                data[11] = (byte)(temp / 256);
                data[12] = (byte)(temp);

                temp = int.Parse(textWChgMaxt.Text);
                data[13] = (byte)(temp + 40);

                configData.setStep = (byte)BcuStep.SetChgInfo;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWHeatControl_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 7;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x18a20091;
            configData.objectSender = sender;
            int temp;
            try
            {

                temp = int.Parse(textWFanOnTempBcu.Text);
                data[0] = (byte)(temp + 40);
                temp = int.Parse(textWFanOffTempBcu.Text);
                data[1] = (byte)(temp + 40);

                data[2] = byte.Parse(textWFanOnDiffBcu.Text);
                data[3] = byte.Parse(textWFanOffDiffBcu.Text);

                temp = int.Parse(textWHeatOnTempBcu.Text);
                data[4] = (byte)(temp + 40);

                temp = int.Parse(textWHeatOffTempBcu.Text);
                data[5] = (byte)(temp + 40);

                data[6] = byte.Parse(textWHeatOffDiffBcu.Text);

                configData.setStep = (byte)BcuStep.SetTemControl;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void button41_Click(object sender, EventArgs e)
        {
            tabBcu.SelectedIndex = 1;
        }

        private void button42_Click(object sender, EventArgs e)
        {
            tabBcu.SelectedIndex = 0;
        }

        private void btnWSocBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();

            configData.len = 1 + 2;
            configData.cmd = 7;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWSocBcu.Text) * 10;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);


                configData.setStep = (byte)BcuStep.SetSoc;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWSohBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();

            configData.len = 1 + 2;
            configData.cmd = 12;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {

                temp = double.Parse(textWSohBcu.Text) * 10;
                data[0] = (byte)(temp / 256);
                data[1] = (byte)(temp);


                configData.setStep = (byte)BcuStep.SetSoh;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWAhBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 8;
            configData.cmd = 8;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {
                temp = double.Parse(textWAhBcu.Text) * 1000;
                data[0] = (byte)(temp / 0x1000000);
                data[1] = (byte)(temp / 0x10000);
                data[2] = (byte)(temp / 0x100);
                data[3] = (byte)(temp);


                temp = double.Parse(textWRealAhBcu.Text) * 1000;
                data[4] = (byte)(temp / 0x1000000);
                data[5] = (byte)(temp / 0x10000);
                data[6] = (byte)(temp / 0x100);
                data[7] = (byte)(temp);


                configData.setStep = (byte)BcuStep.SetAh;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }

        }

        private void btnWProNoBcu_Click(object sender, EventArgs e)
        {

            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 16;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {
                byte[] tempdata = new byte[50];
                tempdata = Encoding.Default.GetBytes(textWProNoBcu.Text);

                for (byte i = 0; i < tempdata.Length; i++)
                {
                    configData.data[i] = tempdata[i];
                }

                configData.setStep = (byte)BcuStep.SetProductNo;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWCarNoBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 8;
            configData.cmd = 11;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            try
            {
                byte[] tempdata = new byte[50];
                tempdata = Encoding.Default.GetBytes(textWCarNoBcu.Text);

                for (byte i = 0; i < tempdata.Length; i++)
                {
                    configData.data[i] = tempdata[i];
                }

                configData.setStep = (byte)BcuStep.SetCarNumber;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWConfigVerBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 8;
            configData.cmd = 9;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {
                byte[] tempdata = new byte[50];
                tempdata = Encoding.Default.GetBytes(textWConfigVerBcu.Text);

                for (byte i = 0; i < tempdata.Length; i++)
                {
                    configData.data[i] = tempdata[i];
                }

                configData.setStep = (byte)BcuStep.SetProductNo;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWClockBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();

            configData.len = 1 + 6;
            configData.cmd = 14;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            double temp;
            try
            {

                textWClock.Text = System.DateTime.Now.Year.ToString() + "-"
                + System.DateTime.Now.Month.ToString() + "-"
                + System.DateTime.Now.Day.ToString() + " "
                + System.DateTime.Now.Hour.ToString() + "-"
                + System.DateTime.Now.Minute.ToString() + "-"
                + System.DateTime.Now.Second.ToString();


                data[0] = (byte)(System.DateTime.Now.Year - 2000);
                data[1] = (byte)(System.DateTime.Now.Month);
                data[2] = (byte)(System.DateTime.Now.Day);
                data[3] = (byte)(System.DateTime.Now.Hour);
                data[4] = (byte)(System.DateTime.Now.Minute);
                data[5] = (byte)(System.DateTime.Now.Second);


                configData.setStep = (byte)BcuStep.SetClock;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void textWBmuNumBcu_TextChanged(object sender, EventArgs e)
        {
            for (byte jj = 0; jj < 30; jj++)
            {
                if (jj < byte.Parse(textWBmuNumBcu.Text))
                {
                    dataGridView1.Rows[jj].Cells[0].Value = (jj + 1).ToString();
                    dataGridView1.Rows[jj].Cells[1].Value = "";
                    dataGridView1.Rows[jj].Cells[2].Value = "";
                }
                else
                {
                    dataGridView1.Rows[jj].Cells[0].Value = "";
                    dataGridView1.Rows[jj].Cells[1].Value = "";
                    dataGridView1.Rows[jj].Cells[2].Value = "";
                }
            }
        }

        private void btnWCellInfoBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[70];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 63;
            configData.cmd = 2;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {

                    configData.data[0] = byte.Parse(textWBmuNumBcu.Text);
                    configData.data[1] = 0;
                    configData.data[2] = 0; //集中式主板电池数量

                    for (byte i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {

                        configData.data[i * 2 + 3] = 0;
                        configData.data[i * 2 + 4] = byte.Parse(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    }

                    configData.setStep = (byte)BcuStep.SetVoltNumber;

                    if (backgroundWorkerMulti.IsBusy == false)
                    {
                        ((Button)sender).BackColor = Color.Gray;
                        backgroundWorkerMulti.RunWorkerAsync(configData);
                    }
                }
                else
                {
                    MessageBox.Show("请输入BMU个数");
                }
            }
            catch { }


        }

        private void btnWTempInfoBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[70];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 62;
            configData.cmd = 5;
            configData.data = data;
            configData.id = 0x18a00091;
            configData.objectSender = sender;
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {

                    configData.data[0] = 0;
                    configData.data[1] = 0;//集中式主板温度数量

                    for (byte i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {

                        configData.data[i * 2 + 2] = 0;
                        configData.data[i * 2 + 3] = byte.Parse(dataGridView1.Rows[i].Cells[2].Value.ToString());
                    }

                    configData.setStep = (byte)BcuStep.SetTempNumber;

                    if (backgroundWorkerMulti.IsBusy == false)
                    {
                        ((Button)sender).BackColor = Color.Gray;
                        backgroundWorkerMulti.RunWorkerAsync(configData);
                    }
                }
                else
                {
                    MessageBox.Show("请输入BMU个数");
                }
            }
            catch { }
        }

        private void backgroundWorkerMulti_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void btnOneKeySetBcu_Click(object sender, EventArgs e)//简单调用全部按钮事件完成一键设置功能
        {
            if (oneKeySetThread == null)
            {
                ForceStop = false;
                oneKeySetThread = new Thread(OneKeySet);
                tabBcu.SelectedIndex = 0;
                textConfigInfo.ForeColor = Color.White;
                oneKeySetThread.Start();
            }
            else 
            {
                try
                {
                    if (oneKeySetThread.IsAlive == true)
                    {
                        MessageBox.Show("上一次操作未完成");
                    }
                    else
                    {
                        ForceStop = false;
                        oneKeySetThread = new Thread(OneKeySet);
                        tabBcu.SelectedIndex = 0;
                        textConfigInfo.ForeColor = Color.White;
                        oneKeySetThread.Start();
                    }
                }
                catch { }
            }
        }

        private void btnOneKeyRead_Click(object sender, EventArgs e)
        {
            if (oneKeyReadThread == null)
            {
                ForceStop = false;
                ProjectGlobal.zlgCan.CanClearBuffer();
                oneKeyReadThread = new Thread(OneKeyRead);
                textConfigInfo.ForeColor = Color.White;
                oneKeyReadThread.Start();
                tabBcu.SelectedIndex = 0;
            }
            else 
            {
                try
                {
                    if (oneKeyReadThread.IsAlive == true)
                    {
                        MessageBox.Show("上一次操作未完成");
                    }
                    else
                    {
                        ForceStop = false;
                        ProjectGlobal.zlgCan.CanClearBuffer();
                        oneKeyReadThread = new Thread(OneKeyRead);
                        textConfigInfo.ForeColor = Color.White;
                        oneKeyReadThread.Start();
                        tabBcu.SelectedIndex = 0;
                    }
                }
                catch { }
            }


        }

        private void OneKeySet()
        {
            OneKeySetBcuStepNo = 0;
            OneKeySetFlag = true;

            btnR2dCodeBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWSocBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWSohBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWAhBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWProNoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWCarNoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWConfigVerBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWClockBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWHeatControl.BackColor = Color.FromArgb(224, 224, 224);
            btnWChginfoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWTempInfoBcu.BackColor = Color.FromArgb(224, 224, 224);

            btnWLevelBcu1.BackColor = Color.FromArgb(224, 224, 224);
            btnWLevelBcu2.BackColor = Color.FromArgb(224, 224, 224);
            btnWLevelBcu3.BackColor = Color.FromArgb(224, 224, 224);
            btnWBatInfoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWChgAhBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnWChgTimeBcu.BackColor = Color.FromArgb(224, 224, 224);



            btnWFuseChan.BackColor = Color.FromArgb(224, 224, 224);

            btnWVoltNum.BackColor = Color.FromArgb(224, 224, 224);
            btnWTempNum.BackColor = Color.FromArgb(224, 224, 224);
            btnWChanVoltNum.BackColor = Color.FromArgb(224, 224, 224);
            btnWTempControl.BackColor = Color.FromArgb(224, 224, 224);
            btnWLevel1.BackColor = Color.FromArgb(224, 224, 224);
            btnWLevel2.BackColor = Color.FromArgb(224, 224, 224);
            btnWLevel3.BackColor = Color.FromArgb(224, 224, 224);
            btnW2dCode.BackColor = Color.FromArgb(224, 224, 224);

            if (tabControl1.SelectedIndex == 0)//Set BCU Info
            {
                textConfigInfo.Text = "***一键设置BCU所有参数***\r\n";
                while (OneKeySetBcuStepNo < 18)
                {
                    if (ForceStop == true)
                    {
                        return;
                    }
                    if ((backgroundWorkerSingle.IsBusy == false) && (backgroundWorkerMulti.IsBusy == false))
                    {
                        #region //////////
                        switch (OneKeySetBcuStepNo)
                        {
                            case 0:
                                {
                                    btnWSocBcu.PerformClick();
                                    break;
                                }
                            case 1:
                                {
                                    btnWSohBcu.PerformClick();
                                    break;
                                }
                            case 2:
                                {
                                    btnWAhBcu.PerformClick();
                                    break;
                                }
                            case 3:
                                {
                                    btnWProNoBcu.PerformClick();
                                    break;
                                }
                            case 4:
                                {
                                    btnWCarNoBcu.PerformClick();
                                    break;
                                }
                            case 5:
                                {
                                    btnWConfigVerBcu.PerformClick();
                                    break;
                                }
                            case 6:
                                {
                                    btnWClockBcu.PerformClick();
                                    break;
                                }
                            case 7:
                                {
                                    btnWHeatControl.PerformClick();
                                    break;
                                }
                            case 8:
                                {
                                    btnWChginfoBcu.PerformClick();
                                    break;
                                }
                            case 9:
                                {
                                    btnWCellInfoBcu.PerformClick();
                                    break;
                                }
                            case 10:
                                {
                                    btnWTempInfoBcu.PerformClick();
                                    break;
                                }
                            case 11:
                                {
                                    this.Invoke(UpdateUi);
                                    btnWLevelBcu1.PerformClick();
                                    break;
                                }
                            case 12:
                                {
                                    btnWLevelBcu2.PerformClick();
                                    break;
                                }
                            case 13:
                                {
                                    btnWLevelBcu3.PerformClick();
                                    break;
                                }
                            case 14:
                                {
                                    btnWBatInfoBcu.PerformClick();
                                    break;
                                }
                            case 15:
                                {
                                    btnWChgAhBcu.PerformClick();
                                    break;
                                }
                            case 16:
                                {
                                    btnWChgTimeBcu.PerformClick();
                                    break;
                                }

                        }

                        #endregion
                        OneKeySetBcuStepNo++;
                    }
                }
            }
            else//Set Bmu Info
            {
                textConfigInfo.Text = "***一键设置BMU所有参数***\r\n";
                while (OneKeySetBcuStepNo < 10 )
                {
                    if (ForceStop == true)
                    {
                        return;
                    }

                    if ((backgroundWorkerSingle.IsBusy == false) && (backgroundWorkerMulti.IsBusy == false))
                    {
                        #region //////////
                        switch (OneKeySetBcuStepNo)
                        {
                            //case 0:
                            //    {
                            //        btnWBmuNo.PerformClick();
                            //        break;
                            //    }

                            case 1:
                                {
                                    btnWVoltNum.PerformClick();
                                    break;
                                }
                            case 2:
                                {
                                    btnWTempNum.PerformClick();
                                    break;
                                }
                            case 3:
                                {
                                    btnWChanVoltNum.PerformClick();
                                    break;
                                }
                            case 4:
                                {
                                    btnWTempControl.PerformClick();
                                    break;
                                }
                                
                               case 5:
                                {
                                    btnWFuseChan.PerformClick();
                                    break;
                                }
                            case 6:
                                {
                                    btnWLevel1.PerformClick();
                                    break;
                                }
                            case 7:
                                {
                                    btnWLevel2.PerformClick();
                                    break;
                                }
                            case 8:
                                {
                                    btnWLevel3.PerformClick();
                                    break;
                                }
   
                        }
                        #endregion
                        OneKeySetBcuStepNo++;
                    }
                }
            }

           
            if (textConfigInfo.Text.IndexOf("失败") > 0)
            {
                MessageBox.Show("设置失败");
                textConfigInfo.Text += "********失败*********\r\n";
                textConfigInfo.ForeColor = Color.Red;
            }
            else
            {
                MessageBox.Show("设置成功");
                textConfigInfo.Text += "........成功.........\r\n";
                textConfigInfo.ForeColor = Color.LimeGreen;
            }

            //OneKeySetFlag = false;
        }

        private void OneKeyRead()
        {
            OneKeyReadBcuStepNo = 0;

            btnRSocBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRSohBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRAhBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRProNoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRCarNoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRConfigVerBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRClockBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRHeatControl.BackColor = Color.FromArgb(224, 224, 224);
            btnRChginfoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRTempInfoBcu.BackColor = Color.FromArgb(224, 224, 224);

            btnRLevelBcu1.BackColor = Color.FromArgb(224, 224, 224);
            btnRLevelBcu2.BackColor = Color.FromArgb(224, 224, 224);
            btnRLevelBcu3.BackColor = Color.FromArgb(224, 224, 224);
            btnRBatInfoBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRChgAhBcu.BackColor = Color.FromArgb(224, 224, 224);
            btnRChgTimeBcu.BackColor = Color.FromArgb(224, 224, 224);

            btnRVoltNum.BackColor = Color.FromArgb(224, 224, 224);
            btnRTempNum.BackColor = Color.FromArgb(224, 224, 224);
            btnRChanVoltNum.BackColor = Color.FromArgb(224, 224, 224);
            btnRTempControl.BackColor = Color.FromArgb(224, 224, 224);
            btnRLevel1.BackColor = Color.FromArgb(224, 224, 224);
            btnRLevel2.BackColor = Color.FromArgb(224, 224, 224);
            btnRLevel3.BackColor = Color.FromArgb(224, 224, 224);
            btnR2dCode.BackColor = Color.FromArgb(224, 224, 224);
            btnRFuseChan.BackColor = Color.FromArgb(224, 224, 224);



            if (tabControl1.SelectedIndex == 0)//Set BCU Info
            {
                textConfigInfo.Text = "***一键读取BCU所有参数***\r\n";
                while (OneKeyReadBcuStepNo < 19)
                {
                    if (ForceStop == true)
                    {
                        return;
                    }

                    if ((backgroundWorkerSingleRead.IsBusy == false) && (backgroundWorkerMultiRead.IsBusy == false))
                    {
                        #region //////////
                        switch (OneKeyReadBcuStepNo)
                        {
                            case 0:
                                {
                                    btnRSocBcu.PerformClick();
                                    break;
                                }
                            case 1:
                                {
                                    btnRSohBcu.PerformClick();
                                    break;
                                }
                            case 2:
                                {
                                    btnRAhBcu.PerformClick();
                                    break;
                                }
                            case 3:
                                {
                                    btnRProNoBcu.PerformClick();
                                    break;
                                }
                            case 4:
                                {
                                    btnRCarNoBcu.PerformClick();
                                    break;
                                }
                            case 5:
                                {
                                    btnRConfigVerBcu.PerformClick();
                                    break;
                                }
                            case 6:
                                {
                                    btnRClockBcu.PerformClick();
                                    break;
                                }
                            case 7:
                                {
                                    btnR2dCodeBcu.PerformClick();
                                    break;
                                }

                            case 8:
                                {
                                    cellReadOkFlag = false;
                                    btnRCellInfoBcu.PerformClick();
                                    break;
                                }
                            case 9:
                                {
                                    if (cellReadOkFlag == true)
                                    {
                                        btnRTempInfoBcu.PerformClick();
                                    }
                                    break;
                                }
                                
                            case 10:
                                {
                                    btnRHeatControl.
                                    PerformClick();
                                    break;
                                }
                            case 11:
                                {
                                    btnRChginfoBcu.PerformClick();
                                    break;
                                }

                            case 12:
                                {
                                    this.Invoke(UpdateUi);
                                    btnRLevelBcu1.PerformClick();
                                    break;
                                }
                            case 13:
                                {
                                    btnRLevelBcu2.PerformClick();
                                    break;
                                }
                            case 14:
                                {
                                    btnRLevelBcu3.PerformClick();
                                    break;
                                }
                            case 15:
                                {
                                    btnRBatInfoBcu.PerformClick();
                                    break;
                                }
                            case 16:
                                {
                                    btnRChgAhBcu.PerformClick();
                                    break;
                                }
                            case 17:
                                {
                                    btnRChgTimeBcu.PerformClick();
                                    break;
                                }

                        }

                        #endregion
                        OneKeyReadBcuStepNo++;
                    }
                }
            }
            else//Set Bmu Info
            {
                textConfigInfo.Text = "***一键读取BMU所有参数***\r\n";
                while (OneKeyReadBcuStepNo < 11)
                {
                    if (ForceStop == true)
                    {
                        return;
                    }

                    if ((backgroundWorkerSingleRead.IsBusy == false) && (backgroundWorkerMultiRead.IsBusy == false))
                    {
                        #region //////////
                        switch (OneKeyReadBcuStepNo)
                        {
                            //case 0:
                            //    {
                            //        btnRBmuNo.PerformClick();
                            //        break;
                            //    }
                            case 1:
                                {
                                    btnRVoltNum.PerformClick();
                                    break;
                                }
                            case 2:
                                {
                                    btnRTempNum.PerformClick();
                                    break;
                                }
                            case 3:
                                {
                                    btnRChanVoltNum.PerformClick();
                                    break;
                                }
                            case 4:
                                {
                                    btnRTempControl.PerformClick();
                                    break;
                                }
                            case 5:
                                {
                                    btnRFuseChan.PerformClick();
                                    break;
                                }
                            case 6:
                                {
                                    btnRLevel1.PerformClick();
                                    break;
                                }
                            case 7:
                                {
                                    btnRLevel2.PerformClick();
                                    break;
                                }
                            case 8:
                                {
                                    btnRLevel3.PerformClick();
                                    break;
                                }
                            case 9:
                                {
                                    btnR2dCode.PerformClick();
                                    break;
                                }

                                                   
                        }
                        #endregion
                        OneKeyReadBcuStepNo++;
                    }
                }
            }


            if (textConfigInfo.Text.IndexOf("失败") > 0)
            {
                MessageBox.Show("读取失败");
                textConfigInfo.Text += "********失败*********\r\n";
                textConfigInfo.ForeColor = Color.Red;
            }
            else
            {
                MessageBox.Show("读取成功");
                textConfigInfo.Text += "........成功.........\r\n";
                textConfigInfo.ForeColor = Color.LimeGreen;
            }

            if (textProject.TextLength == 6)
            {

                btnJudge.PerformClick();
            }

            //OneKeySetFlag = false;
        }



        private void btnWChgTimeBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[8];


            ConfigData configData = new ConfigData();
            configData.len = 1 + 6;
            configData.cmd = 4;
            configData.data = data;
            configData.id = 0x18a40091;

            configData.objectSender = sender;
            double temp;
            try
            {




                temp = double.Parse(textWEmptyTimesBcu.Text);
                data[0] = (byte)(temp / 0x100);
                data[1] = (byte)(temp);


                temp = double.Parse(textWFullTimesBcu.Text);
                data[2] = (byte)(temp / 0x100);
                data[3] = (byte)(temp);

                temp = double.Parse(textWChgTimesBcu.Text);
                data[4] = (byte)(temp / 0x100);
                data[5] = (byte)(temp);



                configData.setStep = (byte)BcuStep.SetChgTimes;

                if (backgroundWorkerSingle.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingle.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWChgAhBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 8;
            configData.cmd = 3;
            configData.data = data;
            configData.id = 0x18a40091;
            configData.objectSender = sender;
            double temp;
            try
            {
                temp = double.Parse(textWInAhBcu.Text) * 1000;
                data[0] = (byte)(temp / 0x1000000);
                data[1] = (byte)(temp / 0x10000);
                data[2] = (byte)(temp / 0x100);
                data[3] = (byte)(temp);


                temp = double.Parse(textWOutAhBcu.Text) * 1000;
                data[4] = (byte)(temp / 0x1000000);
                data[5] = (byte)(temp / 0x10000);
                data[6] = (byte)(temp / 0x100);
                data[7] = (byte)(temp);


                configData.setStep = (byte)BcuStep.SetChgAh;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnWBatInfoBcu_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[50];
            ConfigData configData = new ConfigData();
            configData.retryTimes = 5;
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.data = data;
            configData.id = 0x18a40091;
            configData.objectSender = sender;
            double temp;
            try
            {
                temp = double.Parse(textWBatNumBcu.Text);
                data[0] = (byte)(temp / 0x100);
                data[1] = (byte)(temp);

                data[2] = byte.Parse(textWBatParBcu.Text);

                data[3] = 5;//电池厂家

                data[4] = (byte)(comboBoxWBatType.SelectedIndex+1);// byte.Parse(textWBatTypeBcu.Text); 

                temp = double.Parse(textWBatEnergyBcu.Text)*10;
                data[5] = (byte)(temp / 0x100);
                data[6] = (byte)(temp);

                temp = double.Parse(textWRateAhBcu.Text) * 1000;
                data[7] = (byte)(temp / 0x1000000);
                data[8] = (byte)(temp / 0x10000);
                data[9] = (byte)(temp / 0x100);
                data[10] = (byte)(temp);

                temp = double.Parse(textWRealAhBcu.Text) * 1000;
                data[11] = (byte)(temp / 0x1000000);
                data[12] = (byte)(temp / 0x10000);
                data[13] = (byte)(temp / 0x100);
                data[14] = (byte)(temp);

                temp = double.Parse(textWRateSumvBcu.Text) * 10;
                data[15] = (byte)(temp / 0x100);
                data[16] = (byte)(temp);

                data[17] = 0;
                data[18] = 0;
                data[19] = 0;
                data[20] = 0;
                data[21] = 0;
                data[22] = 0;
                data[23] = 0;
                data[24] = 0;
                data[25] = 0;
                data[26] = 0;
                data[27] = 0;
                data[28] = 0;
                data[29] = 0;

                configData.setStep = (byte)BcuStep.SetBatInfo;

                if (backgroundWorkerMulti.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMulti.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void textConfigInfo_DoubleClick(object sender, EventArgs e)
        {
            textConfigInfo.Text = "";
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                textBoardType.Text = "BCU";
            }
            else 
            {
                textBoardType.Text = "BMU";
            }
        }

        unsafe private void backgroundWorkerSingleRead_DoWork(object sender, DoWorkEventArgs e)
        {

            for (byte times = 0; times < 5; times++)//retry times
            {

                ConfigData configData = (ConfigData)(e.Argument);
                VCI_CAN_OBJ[] recvCanMsgArray = new VCI_CAN_OBJ[10];

                byte[] sendData = new byte[8];
                uint idRecv = (configData.id & 0xffff0000) + (configData.id & 0x000000ff) * 256 + (configData.id & 0x0000ff00) / 256;

                sendData[0] = 01;// configData.len 单帧读取
                sendData[1] = configData.cmd;

                ProjectGlobal.zlgCan.CanClearBuffer();

                ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);

                uint recvNum = 0;
                for (byte j = 0; j < 50; j++)
                {
                    recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                    if (recvNum > 0)
                    {
                        byte i = 0;
                        for (i = 0; i < recvNum; i++)
                        {
                            fixed (VCI_CAN_OBJ* rc1 = &recvCanMsgArray[i])
                            {
                                if ((rc1->ID == idRecv))//&& ((rc1->Data[0]) == sendData[0]) && (rc1->Data[1] == sendData[1]))
                                {
                                    if (((rc1->Data[2]) == 0) && (rc1->Data[3] == 0))//无故障
                                    {
                                        switch (configData.setStep)
                                        {
                                            case (byte)BcuStep.ReadSoc:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textRSocBcu.Text = ((rc1->Data[4] * 256 + rc1->Data[5]) * 0.1d).ToString();
                                                    textConfigInfo.Text += "BCU SOC读取成功\r\n";
                                                    break;
                                                }
                                            case (byte)BcuStep.ReadSoh:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textRSohBcu.Text = ((rc1->Data[4] * 256 + rc1->Data[5]) * 0.1d).ToString();
                                                    textConfigInfo.Text += "BCU SOH读取成功\r\n";
                                                    break;
                                                }

                                            case (byte)BmuStep.ReadVoltNumber:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textRVolNum.Text = ((rc1->Data[4] * 256 + rc1->Data[5]) ).ToString();
                                                    textConfigInfo.Text += "BMU 电池节数读取成功\r\n";
                                                    break;
                                                }

                                            case (byte)BmuStep.ReadTempNumber:
                                                {
                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                    textRTempNum.Text = ((rc1->Data[4] * 256 + rc1->Data[5])).ToString();
                                                    textConfigInfo.Text += "BMU 温度个数读取成功\r\n";
                                                    break;
                                                }
                                        }
                                        return;
                                    }
                                }
                            }
                        }

                    }
                }
                switch (configData.setStep)//Result Control
                {
                    case (byte)BcuStep.ReadSoc:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU SOC读取失败\r\n";
                            break;
                        }
                    case (byte)BcuStep.ReadSoh:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BCU SOH读取失败\r\n";
                            break;
                        }

                    case (byte)BmuStep.ReadVoltNumber:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BMU 电池节数读取失败\r\n";
                            break;
                        }

                    case (byte)BmuStep.ReadTempNumber:
                        {
                            ((Button)configData.objectSender).BackColor = Color.Red;
                            textConfigInfo.Text += "BMU 温度个数读取失败\r\n";
                            break;
                        }
                }
                return;
            }
        }

        private void btnRSocBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 2;
            configData.cmd = 7;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            try
            {
                configData.setStep = (byte)BcuStep.ReadSoc;
                if (backgroundWorkerSingleRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingleRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRSohBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 2;
            configData.cmd = 12;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            try
            {
                configData.setStep = (byte)BcuStep.ReadSoh;
                if (backgroundWorkerSingleRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingleRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        unsafe private void backgroundWorkerMultiRead_DoWork(object sender, DoWorkEventArgs e)
        {
            ConfigData configData = (ConfigData)(e.Argument);
            VCI_CAN_OBJ[] recvCanMsgArray = new VCI_CAN_OBJ[10];
            byte[] sendData = new byte[8];
            uint idRecv = (configData.id & 0xffff0000) + (configData.id & 0x000000ff) * 256 + (configData.id & 0x0000ff00) / 256;
            byte sendLength;
            byte sendSerialNo;
            byte needRecvFrameNum=0;
            byte needRecvFrameCnt=0;
            byte[] RecvData = new byte[100];
 
            for (byte times = 0; times < configData.retryTimes; times++)//retry times
            {
                #region //send start frame
                sendData[0] = 0x01;
                sendData[1] = configData.cmd;

                ProjectGlobal.zlgCan.CanClearBuffer();
                ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);
                sendLength = 5;//Already send 5 bytes

                #endregion
                #region//receive flow control frame
                uint recvNum = 0;

                for (byte j = 0; j < 50; j++)
                {
                    recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                    if (recvNum > 0)
                    {
                        for (byte i = 0; i < recvNum; i++)
                        {
                            fixed (VCI_CAN_OBJ* rc1 = &recvCanMsgArray[i])
                            {
                                if ((rc1->ID == idRecv) && (rc1->Data[0] == 0x10) && (rc1->Data[2] == configData.cmd) && (rc1->Data[3] == 0) && (rc1->Data[4] == 0))
                                {
                #endregion
                                    RecvData[0] = rc1->Data[5];
                                    RecvData[1] = rc1->Data[6];
                                    RecvData[2] = rc1->Data[7];

                                    needRecvFrameCnt = 0;

                                    if ((rc1->Data[1] - 6) % 7 == 0)
                                    {
                                        needRecvFrameNum = (byte)((rc1->Data[1] - 6) / 7);
                                    }
                                    else
                                    {
                                        needRecvFrameNum = (byte)((rc1->Data[1] - 6) / 7 +1 );
                                    }

                                    #region //send Flow Control frame

                                            sendData[0] = 0x30;
                                            sendData[1] = 0x00;
                                            sendData[2] = 0x14;
                                            ProjectGlobal.zlgCan.CanClearBuffer();
                                            ProjectGlobal.zlgCan.Can_SendOneFrame(configData.id, sendData, 1);
                                    #endregion

                                        #region //receive result frame
                                        recvNum = 0;

                                        for (byte t = 0; t < 50; t++)
                                        {
                                            recvCanMsgArray = ProjectGlobal.zlgCan.Can_Receive(ref recvNum);
                                            if (recvNum > 0)
                                            {
                                                for (byte l = 0; l < recvNum; l++)
                                                {
                                                    fixed (VCI_CAN_OBJ* rc2 = &recvCanMsgArray[i])
                                                    {
                                                        if ((rc2->ID == idRecv))// && (rc2->Data[2] == 0) && (rc1->Data[3] == 0))
                                                        {
                                                            for(byte ii=0;ii<7;ii++)
                                                            {
                                                                RecvData[((rc2->Data[0] & 0x0f) - 1) * 7 + 3+ii] = rc2->Data[ii+1];
                                                            }
                                                            needRecvFrameCnt++;

                                                            if (needRecvFrameCnt >= needRecvFrameNum)//全部接收完成
                                                            {
                                                                
                                                                switch (configData.setStep)
                                                                {
                                                                    case (byte)BcuStep.ReadAh:
                                                                        {
                                                                            textRAhBcu.Text =(( RecvData[0] * 0x1000000 + RecvData[1] * 0x10000 + RecvData[2] * 0x100 + RecvData[3] )/1000d).ToString();
                                                                            textRRealAhBcu.Text = ((RecvData[4] * 0x1000000 + RecvData[5] * 0x10000 + RecvData[6] * 0x100 + RecvData[7]) / 1000d).ToString();
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 安时数读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadProductNo:
                                                                        {
                                                                            textRProNoBcu.Text=Encoding.Default.GetString(RecvData, 0, 16);
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 产品编号读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadCarNumber:
                                                                        {
                                                                            textRCarNoBcu.Text = Encoding.Default.GetString(RecvData, 0, 8);
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 车牌号读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadConfigVersion:
                                                                        {
                                                                            textRConfigVerBcu.Text = Encoding.Default.GetString(RecvData, 0, 8);
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 配置版本读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.Read2dCode:
                                                                        {
                                                                            textR2dCodeBcu.Text ="BNET"+Encoding.Default.GetString(RecvData, 0, 30);
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 二维码读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadClock:
                                                                        {
                                                                            textRClock.Text = "20" + RecvData[0].ToString() + "-" + RecvData[1].ToString()+"-"+RecvData[2].ToString()+" "
                                                                                                   +RecvData[3].ToString() + "-" + RecvData[4].ToString()+"-"+RecvData[5].ToString();
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 时钟读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadTemControl:
                                                                        {
                                                                            textRFanOnTempBcu.Text = (RecvData[0] - 40).ToString();
                                                                            textRFanOffTempBcu.Text  = (RecvData[1] - 40).ToString();
                                                                            textRFanOnDiffBcu.Text= (RecvData[2]).ToString();
                                                                            textRFanOffDiffBcu.Text = (RecvData[3]).ToString();

                                                                            textRHeatOnTempBcu.Text = (RecvData[4] - 40).ToString();
                                                                            textRHeatOffTempBcu.Text = (RecvData[5] - 40).ToString();
                                                                            textRHeatOffDiffBcu.Text = (RecvData[6]).ToString();


                                                                           ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 加热控制读取成功\r\n";
                                                                            break;
                                                                        }

                                                                    case (byte)BcuStep.ReadChgInfo:
                                                                        {
                                                                            textRChgMaxCellV.Text = ((RecvData[0] * 256 + RecvData[1])/1000d).ToString();
                                                                            textRChgMaxSumv.Text = ((RecvData[2] * 256 + RecvData[3]) / 10d).ToString();
                                                                            textRChgMaxCurr.Text = ((RecvData[4] * 256 + RecvData[5]) / 10d).ToString();
                                                                            textRChgMint.Text  = (RecvData[6] - 40).ToString();
                                                                            textRChgMinCurr.Text  = ((RecvData[7] * 256 + RecvData[8]) / 10d).ToString();
                                                                            textRChgStep.Text= ((RecvData[9] * 256 + RecvData[10]) / 10d).ToString();
                                                                            textRChgMinCellV.Text = ((RecvData[11] * 256 + RecvData[12]) / 1000d).ToString();
                                                                            textRChgMaxt.Text= (RecvData[13] - 40).ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 充电控制读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadVoltNumber:
                                                                        {
                                                                            lock (this)
                                                                            {
                                                                                if ((RecvData[0] > 0) && RecvData[0] < 30)
                                                                                {

                                                                                        textRBmuNumBcu.Text = RecvData[0].ToString();
                                                                                        for (byte tt = 0; tt < 30; tt++)
                                                                                        {
                                                                                            if (tt< RecvData[0])
                                                                                            {
                                                                                                dataGridView2.Rows[tt].Cells[0].Value = (tt + 1).ToString();
                                                                                                dataGridView2.Rows[tt].Cells[1].Value = (RecvData[tt * 2 + 3] * 256 + RecvData[tt * 2 + 4]).ToString();
                                                                                                dataGridView2.Rows[tt].Cells[2].Value = "";
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                dataGridView2.Rows[tt].Cells[0].Value = "";
                                                                                                dataGridView2.Rows[tt].Cells[1].Value="";
                                                                                                dataGridView2.Rows[tt].Cells[2].Value="";
                                                                                            }
                                                                                        }
                                                                                        cellReadOkFlag = true;
                                                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                                    textConfigInfo.Text += "BCU 电池节数读取成功\r\n";
                                                                                }
                                                                            }
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadTempNumber:
                                                                        {
                                                                            lock (this)
                                                                            {
                                                                                try
                                                                                {
                                                                                    for (byte jj = 0; jj < byte.Parse(textRBmuNumBcu.Text); jj++)
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            dataGridView2.Rows[jj].Cells[2].Value = RecvData[jj * 2 + 2] * 256 + RecvData[jj * 2 + 3];
                                                                                        }
                                                                                        catch { }
                                                                                    }
                                                                                    ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                                    textConfigInfo.Text += "BCU 温度个数读取成功\r\n";
                                                                                }
                                                                                catch (System.IndexOutOfRangeException)
                                                                                { }
                                                                            }
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadLevel1:
                                                                        {
                                                                            textRSumvHBcu1.Text  = ((RecvData[0] * 256 + RecvData[1]) / 10d).ToString();
                                                                            textRSumvLBcu1.Text = ((RecvData[2] * 256 + RecvData[3]) / 10d).ToString();
                                                                            textRTempHBcu1.Text= (RecvData[4] - 40).ToString();
                                                                            textRTempLBcu1.Text = (RecvData[5] - 40).ToString();
                                                                            textRATUnbaBcu1.Text = (RecvData[6]).ToString();
                                                                            textRSVUnbaBcu1.Text= ((RecvData[7] * 256 + RecvData[8]) / 1000d).ToString();
                                                                            textRAVUnbaBcu1.Text = ((RecvData[9] * 256 + RecvData[10]) / 1000d).ToString();
                                                                            textRCellVHBcu1.Text = ((RecvData[11] * 256 + RecvData[12]) / 1000d).ToString();
                                                                            textRCellVLBcu1.Text = ((RecvData[13] * 256 + RecvData[14]) / 1000d).ToString();
                                                                            textRChgCurHBcu1.Text = ((RecvData[15] * 256 + RecvData[16]) / 10d).ToString();
                                                                            textRdChgCurHBcu1.Text = ((RecvData[17] * 256 + RecvData[18]) / 10d).ToString();
                                                                            textRSTUnbaBcu1.Text = (RecvData[19]).ToString();
                                                                            textRSocHBcu1.Text = ((RecvData[20] * 256 + RecvData[21]) / 10d).ToString();
                                                                            textRSocLBcu1.Text = ((RecvData[22] * 256 + RecvData[23]) / 10d).ToString();
                                                                            textRResHBcu1.Text= (( RecvData[24]) / 10d).ToString();
                                                                            textRPoleTHBcu1.Text = (RecvData[25] - 40).ToString();
                                                                            textRSohHBcu1.Text   = ((RecvData[26] * 256 + RecvData[27]) / 10d).ToString();
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 一级故障读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadLevel2:
                                                                        {
                                                                            textRSumvHBcu2.Text = ((RecvData[0] * 256 + RecvData[1]) / 10d).ToString();
                                                                            textRSumvLBcu2.Text = ((RecvData[2] * 256 + RecvData[3]) / 10d).ToString();
                                                                            textRTempHBcu2.Text = (RecvData[4] - 40).ToString();
                                                                            textRTempLBcu2.Text = (RecvData[5] - 40).ToString();
                                                                            textRATUnbaBcu2.Text = (RecvData[6]).ToString();
                                                                            textRSVUnbaBcu2.Text = ((RecvData[7] * 256 + RecvData[8]) / 1000d).ToString();
                                                                            textRAVUnbaBcu2.Text = ((RecvData[9] * 256 + RecvData[10]) / 1000d).ToString();
                                                                            textRCellVHBcu2.Text = ((RecvData[11] * 256 + RecvData[12]) / 1000d).ToString();
                                                                            textRCellVLBcu2.Text = ((RecvData[13] * 256 + RecvData[14]) / 1000d).ToString();
                                                                            textRChgCurHBcu2.Text = ((RecvData[15] * 256 + RecvData[16]) / 10d).ToString();
                                                                            textRdChgCurHBcu2.Text = ((RecvData[17] * 256 + RecvData[18]) / 10d).ToString();
                                                                            textRSTUnbaBcu2.Text = (RecvData[19]).ToString();
                                                                            textRSocHBcu2.Text = ((RecvData[20] * 256 + RecvData[21]) / 10d).ToString();
                                                                            textRSocLBcu2.Text = ((RecvData[22] * 256 + RecvData[23]) / 10d).ToString();
                                                                            textRResHBcu2.Text = ((RecvData[24]) / 10d).ToString();
                                                                            textRPoleTHBcu2.Text = (RecvData[25] - 40).ToString();
                                                                            textRSohHBcu2.Text = ((RecvData[26] * 256 + RecvData[27]) / 10d).ToString();
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 二级故障读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadLevel3:
                                                                        {
                                                                            textRSumvHBcu3.Text = ((RecvData[0] * 256 + RecvData[1]) / 10d).ToString();
                                                                            textRSumvLBcu3.Text = ((RecvData[2] * 256 + RecvData[3]) / 10d).ToString();
                                                                            textRTempHBcu3.Text = (RecvData[4] - 40).ToString();
                                                                            textRTempLBcu3.Text = (RecvData[5] - 40).ToString();
                                                                            textRATUnbaBcu3.Text = (RecvData[6]).ToString();
                                                                            textRSVUnbaBcu3.Text = ((RecvData[7] * 256 + RecvData[8]) / 1000d).ToString();
                                                                            textRAVUnbaBcu3.Text = ((RecvData[9] * 256 + RecvData[10]) / 1000d).ToString();
                                                                            textRCellVHBcu3.Text = ((RecvData[11] * 256 + RecvData[12]) / 1000d).ToString();
                                                                            textRCellVLBcu3.Text = ((RecvData[13] * 256 + RecvData[14]) / 1000d).ToString();
                                                                            textRChgCurHBcu3.Text = ((RecvData[15] * 256 + RecvData[16]) / 10d).ToString();
                                                                            textRdChgCurHBcu3.Text = ((RecvData[17] * 256 + RecvData[18]) / 10d).ToString();
                                                                            textRSTUnbaBcu3.Text = (RecvData[19]).ToString();
                                                                            textRSocHBcu3.Text = ((RecvData[20] * 256 + RecvData[21]) / 10d).ToString();
                                                                            textRSocLBcu3.Text = ((RecvData[22] * 256 + RecvData[23]) / 10d).ToString();
                                                                            textRResHBcu3.Text = ((RecvData[24]) / 10d).ToString();
                                                                            textRPoleTHBcu3.Text = (RecvData[25] - 40).ToString();
                                                                            textRSohHBcu3.Text = ((RecvData[26] * 256 + RecvData[27]) / 10d).ToString();
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 三级故障读取成功\r\n";
                                                                            break;
                                                                        }

                                                                    case (byte)BcuStep.ReadBatInfo:
                                                                        {
                                                                            textRBatNumBcu.Text = ((RecvData[0] * 256 + RecvData[1]) ).ToString();
                                                                            textRBatParBcu.Text  = (RecvData[2]).ToString();

                                                                            comboBoxRBatType.SelectedIndex = RecvData[4] - 1;

                                                                            textRBatEnergyBcu.Text  = ((RecvData[5] * 256 + RecvData[6]) / 10d).ToString();
                                                                            textRRateAhBcu.Text = ((RecvData[7] * 0x1000000 + RecvData[8] * 0x10000 + RecvData[9] * 0x100 + RecvData[10]) / 1000d).ToString();
                                                                            textRBatRealAhBcu.Text = ((RecvData[11] * 0x1000000 + RecvData[12] * 0x10000 + RecvData[13] * 0x100 + RecvData[14]) / 1000d).ToString();

                                                                            textRRateSumvBcu.Text = ((RecvData[15] * 256 + RecvData[16]) / 10d).ToString();


                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 电池信息读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadChgAh:
                                                                        {
                                                                            textRInAhBcu.Text = ((RecvData[0] * 0x1000000 + RecvData[1] * 0x10000 + RecvData[2] * 0x100 + RecvData[3]) / 1000d).ToString();
                                                                            textROutAhBcu.Text = ((RecvData[4] * 0x1000000 + RecvData[5] * 0x10000 + RecvData[6] * 0x100 + RecvData[7]) / 1000d).ToString();


                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 充放电AH读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BcuStep.ReadChgTimes:
                                                                        {
                                                                            textREmptyTimesBcu.Text = ((RecvData[0] * 256 + RecvData[1])).ToString();
                                                                            textRFullTimesBcu.Text  = ((RecvData[2] * 256 + RecvData[3])).ToString();
                                                                            textRChgTimesBcu.Text  = ((RecvData[4] * 256 + RecvData[5])).ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BCU 充放电次数读取成功\r\n";
                                                                            break;
                                                                        }

                                                                    case (byte)BmuStep.ReadFuseChan:
                                                                        {
                                                                            textRFuseChan1.Text = (RecvData[0] / 16).ToString();
                                                                            textRFuseLoc1.Text = (RecvData[0] &0x0f).ToString();
                                                                            textRFuseChan2.Text = (RecvData[1] / 16).ToString();
                                                                            textRFuseLoc2.Text = (RecvData[1] & 0x0f).ToString();
                                                                            textRFuseChan3.Text = (RecvData[2] / 16).ToString();
                                                                            textRFuseLoc3.Text = (RecvData[2] & 0x0f).ToString();
                                                                            textRFuseChan4.Text = (RecvData[3] / 16).ToString();
                                                                            textRFuseLoc4.Text = (RecvData[3] & 0x0f).ToString();
                                                                            textRFuseChan5.Text = (RecvData[4] / 16).ToString();
                                                                            textRFuseLoc5.Text = (RecvData[4] & 0x0f).ToString();
                                                                            textRFuseChan6.Text = (RecvData[5] / 16).ToString();
                                                                            textRFuseLoc6.Text = (RecvData[5] & 0x0f).ToString();
                                                                            textRFuseChan7.Text = (RecvData[6] / 16).ToString();
                                                                            textRFuseLoc7.Text = (RecvData[6] & 0x0f).ToString();
                                                                            textRFuseChan8.Text = (RecvData[7] / 16).ToString();
                                                                            textRFuseLoc8.Text = (RecvData[7] & 0x0f).ToString();
                                                                            textRFuseChan9.Text = (RecvData[8] / 16).ToString();
                                                                            textRFuseLoc9.Text = (RecvData[8] & 0x0f).ToString();
                                                                            textRFuseChan10.Text = (RecvData[9] / 16).ToString();
                                                                            textRFuseLoc10.Text = (RecvData[9] & 0x0f).ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 保险位置读取成功\r\n";
                                                                            break;
                                                                        }

                                                                    case (byte)BmuStep.ReadChanVolNum:
                                                                        {
                                                                            textRVolNumChan1.Text = RecvData[0].ToString();
                                                                            textRVolNumChan2.Text = RecvData[1].ToString();
                                                                            textRVolNumChan3.Text = RecvData[2].ToString();
                                                                            textRVolNumChan4.Text = RecvData[3].ToString();
                                                                            textRVolNumChan5.Text = RecvData[4].ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 通道电压数读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BmuStep.ReadTemControl:
                                                                        {
                                                                            textRFanOnTemp.Text = (RecvData[0] - 40).ToString();
                                                                            textRFanOffTemp.Text = (RecvData[1] - 40).ToString();
                                                                            textRFanOnDiff.Text = (RecvData[2]).ToString();
                                                                            textRFanOffDiff.Text = (RecvData[3]).ToString();

                                                                            textRHeatOnTemp.Text = (RecvData[4] - 40).ToString();
                                                                            textRHeatOffTemp.Text = (RecvData[5] - 40).ToString();
                                                                            textRHeatOffDiff.Text = (RecvData[6]).ToString();


                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 加热参数读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BmuStep.ReadLevel1:
                                                                        {
                                                                            textRVolHigh1.Text = ((RecvData[0] * 256 + RecvData[1]) / 1000d).ToString();
                                                                            textRVolLow1.Text  = ((RecvData[2] * 256 + RecvData[3]) / 1000d).ToString();
                                                                            textRVolUnbal1.Text= ((RecvData[4] * 256 + RecvData[5]) / 1000d).ToString();
                                                                            textRTempHigh1.Text= (RecvData[6] - 40).ToString();
                                                                            textRTempLow1.Text = (RecvData[7] - 40).ToString();
                                                                            textRTempUnbal1.Text = (RecvData[8]).ToString();
                                                                           
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 一级故障读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BmuStep.ReadLevel2:
                                                                        {
                                                                            textRVolHigh2.Text = ((RecvData[0] * 256 + RecvData[1]) / 1000d).ToString();
                                                                            textRVolLow2.Text = ((RecvData[2] * 256 + RecvData[3]) / 1000d).ToString();
                                                                            textRVolUnbal2.Text = ((RecvData[4] * 256 + RecvData[5]) / 1000d).ToString();
                                                                            textRTempHigh2.Text = (RecvData[6] - 40).ToString();
                                                                            textRTempLow2.Text = (RecvData[7] - 40).ToString();
                                                                            textRTempUnbal2.Text = (RecvData[8]).ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 二级故障读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BmuStep.ReadLevel3:
                                                                        {
                                                                            textRVolHigh3.Text = ((RecvData[0] * 256 + RecvData[1]) / 1000d).ToString();
                                                                            textRVolLow3.Text = ((RecvData[2] * 256 + RecvData[3]) / 1000d).ToString();
                                                                            textRVolUnbal3.Text = ((RecvData[4] * 256 + RecvData[5]) / 1000d).ToString();
                                                                            textRTempHigh3.Text = (RecvData[6] - 40).ToString();
                                                                            textRTempLow3.Text = (RecvData[7] - 40).ToString();
                                                                            textRTempUnbal3.Text = (RecvData[8]).ToString();

                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 三级故障读取成功\r\n";
                                                                            break;
                                                                        }
                                                                    case (byte)BmuStep.Read2dCode:
                                                                        {
                                                                            textR2dCode.Text = "BNET" + Encoding.Default.GetString(RecvData, 0, 30);
                                                                            ((Button)configData.objectSender).BackColor = Color.FromArgb(192, 255, 192);
                                                                            textConfigInfo.Text += "BMU 二维码读取成功\r\n";
                                                                            break;
                                                                        }

                                                                }

                                                                return;
                                                            }
                                                            //  OneKeySetBcuStepNo++;
                                                            
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                }
                            }
                        }
                    }
                }
            }

            switch (configData.setStep)
            {
                case (byte)BcuStep.ReadAh:
                    {
                          ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 安时数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadProductNo:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 产品编号读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadCarNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 车牌号读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadConfigVersion:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 配置版本读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.Read2dCode:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 二维码读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadClock:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 时钟读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadTemControl:
                    {


                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 加热控制参数读取失败\r\n";
                        break;
                    }

                case (byte)BcuStep.ReadChgInfo:
                    {


                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 充电控制参数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadVoltNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 电池节数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadTempNumber:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 温度个数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadLevel1:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 一级故障参数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadLevel2:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 二级故障参数读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadLevel3:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 三级故障参数读取失败\r\n";
                        break;
                    }

                case (byte)BcuStep.ReadBatInfo:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 电池信息读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadChgAh:
                    {


                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 充放电AH读取失败\r\n";
                        break;
                    }
                case (byte)BcuStep.ReadChgTimes:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BCU 充放电次数读取失败\r\n";
                        break;
                    }

                case (byte)BmuStep.ReadFuseChan:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 保险位置读取失败\r\n";
                        break;
                    }

                case (byte)BmuStep.ReadChanVolNum:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 通道电压个数读取失败\r\n";
                        break;
                    }
                case (byte)BmuStep.ReadTemControl:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 加热控制参数读取失败\r\n";
                        break;
                    }
                case (byte)BmuStep.ReadLevel1:
                    {

                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 一级故障阀值读取失败\r\n";
                        break;
                    }
                case (byte)BmuStep.ReadLevel2:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 二级故障阀值读取失败\r\n";
                        break;
                    }
                case (byte)BmuStep.ReadLevel3:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 三级故障阀值读取失败\r\n";
                        break;
                    }
                case (byte)BmuStep.Read2dCode:
                    {
                        ((Button)configData.objectSender).BackColor = Color.Red;
                        textConfigInfo.Text += "BMU 二维码读取失败\r\n";
                        break;
                    }
            }
            //OneKeySetBcuStepNo++;
            return;
        }

        private void btnRAhBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 8;
            configData.cmd = 8;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadAh;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRProNoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 16;
            configData.cmd = 1;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadProductNo;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRCarNoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 16;
            configData.cmd = 11;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadCarNumber;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRConfigVerBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 16;
            configData.cmd = 9;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadConfigVersion;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnR2dCodeBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.id = 0x188c0091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.Read2dCode;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRClockBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 6;
            configData.cmd = 14;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadClock;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRHeatControl_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 7;
            configData.cmd = 1;
            configData.id = 0x18820091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadTemControl;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRChginfoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 14;
            configData.cmd = 2;
            configData.id = 0x18820091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadChgInfo;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRCellInfoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 63;
            configData.cmd = 2;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadVoltNumber;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRTempInfoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 62;
            configData.cmd = 5;
            configData.id = 0x18800091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadTempNumber;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevelBcu1_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 28;
            configData.cmd = 1;
            configData.id = 0x18810091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadLevel1;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevelBcu2_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 28;
            configData.cmd = 2;
            configData.id = 0x18810091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadLevel2;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevelBcu3_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 28;
            configData.cmd = 3;
            configData.id = 0x18810091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadLevel3;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRBatInfoBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.id = 0x18840091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadBatInfo;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRChgAhBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 8;
            configData.cmd = 3;
            configData.id = 0x18840091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadChgAh;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRChgTimeBcu_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 6;
            configData.cmd = 4;
            configData.id = 0x18840091;
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BcuStep.ReadChgTimes;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnStopOneKey_Click(object sender, EventArgs e)
        {
            ForceStop = true;
            //try
            //{
            //    backgroundWorkerSingleRead.CancelAsync();
            //    backgroundWorkerMultiRead.CancelAsync();
            //    backgroundWorkerMultiRead.Dispose();
            //    backgroundWorkerSingleRead.Dispose();
            //    oneKeyReadThread.Abort();
            //    oneKeySetThread.Abort();
            //}
            //catch { }
        }

        private void btnRFuseChan_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 10;
            configData.cmd = 3;
            configData.id =(uint)( 0x18000091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BmuStep.ReadFuseChan;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRVoltNum_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 4;
            configData.cmd = 1;
            configData.id = (uint)(0x18000091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BmuStep.ReadVoltNumber;
                if (backgroundWorkerSingleRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingleRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRTempNum_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 4;
            configData.cmd = 4;
            configData.id = (uint)(0x18000091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BmuStep.ReadTempNumber;
                if (backgroundWorkerSingleRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerSingleRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRChanVoltNum_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 20;
            configData.cmd = 2;
            configData.id = (uint)(0x18000091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BmuStep.ReadChanVolNum;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRTempControl_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 7;
            configData.cmd = 5;
            configData.id = (uint)(0x18000091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            try
            {
                configData.setStep = (byte)BmuStep.ReadTemControl;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    ((Button)sender).BackColor = Color.Gray;
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevel1_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 11;
            configData.cmd = 1;
            configData.id = (uint)(0x18020091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            ((Button)sender).BackColor = Color.Gray;
            try
            {
                configData.setStep = (byte)BmuStep.ReadLevel1;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevel2_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 11;
            configData.cmd = 2;
            configData.id = (uint)(0x18020091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            ((Button)sender).BackColor = Color.Gray;
            try
            {
                configData.setStep = (byte)BmuStep.ReadLevel2;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnRLevel3_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 11;
            configData.cmd = 3;
            configData.id = (uint)(0x18020091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            ((Button)sender).BackColor = Color.Gray;
            try
            {
                configData.setStep = (byte)BmuStep.ReadLevel3;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnR2dCode_Click(object sender, EventArgs e)
        {
            ConfigData configData = new ConfigData();
            configData.len = 1 + 30;
            configData.cmd = 1;
            configData.id = (uint)(0x180A0091 + byte.Parse(textCurrBmuNo.Text) * 256);
            configData.objectSender = sender;
            configData.retryTimes = 5;
            ((Button)sender).BackColor = Color.Gray;
            try
            {
                configData.setStep = (byte)BmuStep.Read2dCode;
                if (backgroundWorkerMultiRead.IsBusy == false)
                {
                    backgroundWorkerMultiRead.RunWorkerAsync(configData);
                }
            }
            catch { }
        }

        private void btnNBmuNoMinus_Click(object sender, EventArgs e)
        {
            if (byte.Parse(textNewBmuNo.Text) > 1)
            {
                textNewBmuNo.Text = (byte.Parse(textNewBmuNo.Text) - 1).ToString();
            }
        }

        private void btnNBmuNoPlus_Click(object sender, EventArgs e)
        {
            if (byte.Parse(textCurrBmuNo.Text) < 30)
            {

                textNewBmuNo.Text= (byte.Parse(textCurrBmuNo.Text) + 1).ToString();
            }
        }

        private void btnOneKeyRead_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string temp;
            string[] num = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            openFileDialog1.Filter = "项目配置文件|*.ini";
            byte[] data;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog1.FileName;
                textConfigPath.Text = fName;
                toolStripStatusLabel1.Text = fName;

                if (File.Exists(fName))
                {
                    string md5RealCode = iniFile.IniReadValue(fName, "总信息", "MD5码");


                    File.Copy(fName, @"D:\temp.ini", true);
                    iniFile.IniWriteValue(@"D:\temp.ini", "总信息", "MD5码", " \"MD5码\"");
                    byte[] data1 = File.ReadAllBytes(@"D:\temp.ini");
                    MD5 md5 = new MD5CryptoServiceProvider();
                    for (byte k = 0; k < 2; k++)//兼容新旧上位机格式
                    {
                        data = md5.ComputeHash(data1, 0, data1.Length - 2*k);
                        StringBuilder sBuilder = new StringBuilder();
                        for (int i = 0; i < data.Length; i++)
                        {
                            sBuilder.Append(data[i].ToString("x2"));
                        }

                        if (md5RealCode != sBuilder.ToString())
                        {
                            if (k == 1)
                            {
                                MessageBox.Show("Md5码错误，文件损坏或改动");
                                return;
                            }
                        }
                        else 
                        {
                            break;
                        }
                    }


                    try
                    {
                        this.textWBmuNumBcu.TextChanged -= new EventHandler(textWBmuNumBcu_TextChanged);
                      

                        textProject.Text = iniFile.IniReadValue(fName, "总信息", "工程代号");

                        textWBmuNumBcu.Text = iniFile.IniReadValue(fName, "主板", "从板个数");
                        textWAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池额定容量");
                        textWRealAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池实际容量");
                        textWSocBcu.Text = iniFile.IniReadValue(fName, "主板", "SOC");
                        textWSohBcu.Text = iniFile.IniReadValue(fName, "主板", "SOH");
                        textWProNoBcu.Text = iniFile.IniReadValue(fName, "主板", "产品编号");
                        textWCarNoBcu.Text = iniFile.IniReadValue(fName, "主板", "车架(牌)号");
                        textWConfigVerBcu.Text = iniFile.IniReadValue(fName, "主板", "配置文件版本");

                        if (byte.Parse(textWBmuNumBcu.Text) > 0)
                        {

                            for (byte jj = 0; jj < 30; jj++)
                            {

                                if (jj < byte.Parse(textWBmuNumBcu.Text))
                                {
                                    dataGridView1.Rows[jj].Cells[0].Value = (jj + 1).ToString();
                                    temp = "从板" + (jj + 1).ToString() + "电池节数";
                                    dataGridView1.Rows[jj].Cells[1].Value = iniFile.IniReadValue(fName, "主板", temp);
                                    temp = "从板" + (jj + 1).ToString() + "温度个数";
                                    dataGridView1.Rows[jj].Cells[2].Value = iniFile.IniReadValue(fName, "主板", temp);
                                }
                                else
                                {
                                    dataGridView1.Rows[jj].Cells[0].Value = "";
                                    dataGridView1.Rows[jj].Cells[1].Value = "";
                                    dataGridView1.Rows[jj].Cells[2].Value = "";
                                }
                            }
                        }
                        this.textWBmuNumBcu.TextChanged += new EventHandler(textWBmuNumBcu_TextChanged);

                        textWSumvHBcu1.Text = iniFile.IniReadValue(fName, "主板", "总电压过高一级");
                        textWSumvLBcu1.Text = iniFile.IniReadValue(fName, "主板", "总电压过低一级 ");
                        textWTempHBcu1.Text = iniFile.IniReadValue(fName, "主板", "温度过高一级");
                        textWTempLBcu1.Text = iniFile.IniReadValue(fName, "主板", "温度过低一级");
                        textWAVUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡一级");
                        textWSVUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡一级");
                        textWATUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡一级");
                        textWSTUnbaBcu1.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡一级");
                        textWCellVHBcu1.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高一级");
                        textWCellVLBcu1.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低一级");
                        textWChgCurHBcu1.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高一级");
                        textWdChgCurHBcu1.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高一级");
                        textWSocHBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOC过高一级");
                        textWSocLBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOC过低一级");
                        textWResHBcu1.Text = iniFile.IniReadValue(fName, "主板", "内阻过大一级");
                        textWPoleTHBcu1.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高一级");
                        textWSohHBcu1.Text = iniFile.IniReadValue(fName, "主板", "SOH过低一级");

                        textWSumvHBcu2.Text = iniFile.IniReadValue(fName, "主板", "总电压过高二级");
                        textWSumvLBcu2.Text = iniFile.IniReadValue(fName, "主板", "总电压过低二级 ");
                        textWTempHBcu2.Text = iniFile.IniReadValue(fName, "主板", "温度过高二级");
                        textWTempLBcu2.Text = iniFile.IniReadValue(fName, "主板", "温度过低二级");
                        textWAVUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡二级");
                        textWSVUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡二级");
                        textWATUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡二级");
                        textWSTUnbaBcu2.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡二级");
                        textWCellVHBcu2.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高二级");
                        textWCellVLBcu2.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低二级");
                        textWChgCurHBcu2.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高二级");
                        textWdChgCurHBcu2.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高二级");
                        textWSocHBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOC过高二级");
                        textWSocLBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOC过低二级");
                        textWResHBcu2.Text = iniFile.IniReadValue(fName, "主板", "内阻过大二级");
                        textWPoleTHBcu2.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高二级");
                        textWSohHBcu2.Text = iniFile.IniReadValue(fName, "主板", "SOH过低二级");

                        textWSumvHBcu3.Text = iniFile.IniReadValue(fName, "主板", "总电压过高三级");
                        textWSumvLBcu3.Text = iniFile.IniReadValue(fName, "主板", "总电压过低三级 ");
                        textWTempHBcu3.Text = iniFile.IniReadValue(fName, "主板", "温度过高三级");
                        textWTempLBcu3.Text = iniFile.IniReadValue(fName, "主板", "温度过低三级");
                        textWAVUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "整组电压不均衡三级");
                        textWSVUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "单组电压不均衡三级");
                        textWATUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "整组温度不均衡三级");
                        textWSTUnbaBcu3.Text = iniFile.IniReadValue(fName, "主板", "单组温度不均衡三级");
                        textWCellVHBcu3.Text = iniFile.IniReadValue(fName, "主板", "单体电压过高三级");
                        textWCellVLBcu3.Text = iniFile.IniReadValue(fName, "主板", "单体电压过低三级");
                        textWChgCurHBcu3.Text = iniFile.IniReadValue(fName, "主板", "充电电流过高三级");
                        textWdChgCurHBcu3.Text = iniFile.IniReadValue(fName, "主板", "放电电流过高三级");
                        textWSocHBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOC过高三级");
                        textWSocLBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOC过低三级");
                        textWResHBcu3.Text = iniFile.IniReadValue(fName, "主板", "内阻过大三级");
                        textWPoleTHBcu3.Text = iniFile.IniReadValue(fName, "主板", "极柱温度过高三级");
                        textWSohHBcu3.Text = iniFile.IniReadValue(fName, "主板", "SOH过低三级");

                        textWFanOnTempBcu.Text = iniFile.IniReadValue(fName, "主板", "风机开启温度");
                        textWFanOffTempBcu.Text = iniFile.IniReadValue(fName, "主板", "风机关闭温度");
                        textWFanOnDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "风机开启温差");
                        textWFanOffDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "风机关闭温差");
                        textWHeatOnTempBcu.Text = iniFile.IniReadValue(fName, "主板", "加热开启温度");
                        textWHeatOffTempBcu.Text = iniFile.IniReadValue(fName, "主板", "加热关闭温度");
                        textWHeatOffDiffBcu.Text = iniFile.IniReadValue(fName, "主板", "加热关闭温差");

                        textWChgMaxCellV.Text = iniFile.IniReadValue(fName, "主板", "最大允许单体电压");
                        textWChgMinCellV.Text = iniFile.IniReadValue(fName, "主板", "最小允许单体电压");
                        textWChgMaxCurr.Text = iniFile.IniReadValue(fName, "主板", "最大允许充电电流");
                        textWChgMinCurr.Text = iniFile.IniReadValue(fName, "主板", "最小允许充电电流");
                        textWChgMaxSumv.Text = iniFile.IniReadValue(fName, "主板", "最大允许总电压");
                        textWChgMaxt.Text = iniFile.IniReadValue(fName, "主板", "最大允许充电温度");
                        textWChgMint.Text = iniFile.IniReadValue(fName, "主板", "最小允许充电温度");
                        textWChgStep.Text = iniFile.IniReadValue(fName, "主板", "电流步长");

                        textWBatNumBcu.Text = iniFile.IniReadValue(fName, "主板", "电池串联");
                        textWBatParBcu.Text = iniFile.IniReadValue(fName, "主板", "电池并联");
                        textWRateAhBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定容量");
                        textWBatEnergyBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定能量(kWh)");
                        textWBatRealAhBcu.Text = iniFile.IniReadValue(fName, "主板", "实际容量");
                        textWRateSumvBcu.Text = iniFile.IniReadValue(fName, "主板", "电池包额定总电压");
                        switch (iniFile.IniReadValue(fName, "主板", "电池类型"))
                        {
                            case "磷酸铁锂":
                                {
                                    comboBoxWBatType.SelectedIndex = 0;
                                    break;
                                }
                            case "锰酸锂":
                                {
                                    comboBoxWBatType.SelectedIndex = 1;
                                    break;
                                }
                            case "三元材料":
                                {
                                    comboBoxWBatType.SelectedIndex = 2;
                                    break;
                                }
                            case "铅酸":
                                {
                                    comboBoxWBatType.SelectedIndex = 3;
                                    break;
                                }
                            case "镍氢":
                                {
                                    comboBoxWBatType.SelectedIndex = 4;
                                    break;
                                }
                            case "钴酸锂":
                                {
                                    comboBoxWBatType.SelectedIndex = 5;
                                    break;
                                }
                            case "聚合物锂离子":
                                {
                                    comboBoxWBatType.SelectedIndex = 6;
                                    break;
                                }
                            case "钛酸锂":
                                {
                                    comboBoxWBatType.SelectedIndex = 7;
                                    break;
                                }
                            case "其他电池":
                                {
                                    comboBoxWBatType.SelectedIndex = 8;
                                    break;
                                }
                        }

                        textWInAhBcu.Text = iniFile.IniReadValue(fName, "主板", "累计充电总安时");
                        textWOutAhBcu.Text = iniFile.IniReadValue(fName, "主板", "累计放电总安时");
                        textWChgTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池放空次数");
                        textWFullTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池充满次数");
                        textWEmptyTimesBcu.Text = iniFile.IniReadValue(fName, "主板", "电池充电次数");
                    }
                    catch { }



                    /////////////////////////////////////////////////////////////
                    try
                    {
                        for (byte i = 0; i < byte.Parse(textWBmuNumBcu.Text); i++)
                        {
                            temp = "从板" + (i + 1).ToString();
                            bmuConfigData[i].voltNum = byte.Parse(iniFile.IniReadValue(fName, temp, "电压个数"));
                            bmuConfigData[i].tempNum = byte.Parse(iniFile.IniReadValue(fName, temp, "温度个数"));

                            for (byte j = 0; j < 10; j++)
                            {
                                bmuConfigData[i].fuseChannel[j] = byte.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "第" + num[j] + "个保险通道"));
                                bmuConfigData[i].fuseLocation[j] = byte.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "第" + num[j] + "个保险位置"));
                            }

                            for (byte j = 0; j < 5; j++)
                            {
                                bmuConfigData[i].chanVolNum[j] = byte.Parse(iniFile.IniReadValue(fName, ("从板" + (i + 1).ToString()), ("第" + (j + 1).ToString() + "通道电压个数")));
                            }

                            for (byte j = 0; j < 3; j++)
                            {
                                bmuConfigData[i].voltHith[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压过高" + num[j] + "级"));
                                bmuConfigData[i].voltLow[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压过低" + num[j] + "级"));
                                bmuConfigData[i].tempHigh[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度过高" + num[j] + "级"));
                                bmuConfigData[i].tempLow[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度过低" + num[j] + "级"));
                                bmuConfigData[i].voltDiff[j] = double.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "电压不均衡" + num[j] + "级"));
                                bmuConfigData[i].tempDiff[j] = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "温度不均衡" + num[j] + "级"));
                            }

                            bmuConfigData[i].fanOnTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机开启温度"));
                            bmuConfigData[i].fanOnDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机开启温差"));
                            bmuConfigData[i].fanOffTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机关闭温度"));
                            bmuConfigData[i].fanOffDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "风机关闭温差"));
                            bmuConfigData[i].heatOnTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热开启温度"));
                            bmuConfigData[i].heatOffTemp = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热关闭温度"));
                            bmuConfigData[i].heatOffDiff = Int16.Parse(iniFile.IniReadValue(fName, "从板" + (i + 1).ToString(), "加热关闭温差"));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("从板参数异常:存在空值\r\n 请检查配置文件！！");
                        return;
                    }

                }

                MessageBox.Show("配置文件导入成功________OK");
                btnOneKeySet.Visible = true;
                showBmuConfigData(byte.Parse(textCurrBmuNo.Text));

            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
           
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void btnReadToWrite_Click(object sender, EventArgs e)
        {

            string temp;

            textWSocBcu.Text = textRSocBcu.Text;
            textWSohBcu.Text = textRSohBcu.Text;
            textWAhBcu.Text = textRAhBcu.Text;
            textWRealAhBcu.Text = textRRealAhBcu.Text;
            textWProNoBcu.Text = textRProNoBcu.Text;
            textWCarNoBcu.Text = textRCarNoBcu.Text;
            textWConfigVerBcu.Text = textRConfigVerBcu.Text;
            textW2dCodeBcu.Text = textR2dCodeBcu.Text;


            textWFanOnTempBcu.Text = textRFanOnTempBcu.Text;
            textWFanOffTempBcu.Text = textRFanOffTempBcu.Text;
            textWHeatOnTempBcu.Text = textRHeatOnTempBcu.Text;
            textWHeatOffTempBcu.Text = textRHeatOffTempBcu.Text;
            textWFanOnDiffBcu.Text = textRFanOnDiffBcu.Text;
            textWFanOffDiffBcu.Text = textRFanOffDiffBcu.Text;
            textWHeatOffDiffBcu.Text = textRHeatOffDiffBcu.Text;


            textWChgMaxCellV.Text = textRChgMaxCellV.Text;
            textWChgMinCellV.Text = textRChgMinCellV.Text;
            textWChgMaxCurr.Text = textRChgMaxCurr.Text;
            textWChgMinCurr.Text = textRChgMinCurr.Text;
            textWChgMaxSumv.Text = textRChgMaxSumv.Text;
            textWChgMaxt.Text = textRChgMaxt.Text;
            textWChgMint.Text = textRChgMint.Text;
            textWChgStep.Text = textRChgStep.Text;

            textWSumvHBcu1.Text = textRSumvHBcu1.Text;
            textWSumvLBcu1.Text = textRSumvLBcu1.Text;
            textWTempHBcu1.Text = textRTempHBcu1.Text;
            textWTempLBcu1.Text = textRTempLBcu1.Text;
            textWAVUnbaBcu1.Text = textRAVUnbaBcu1.Text;
            textWSVUnbaBcu1.Text = textRSVUnbaBcu1.Text;
            textWATUnbaBcu1.Text = textRATUnbaBcu1.Text;
            textWSTUnbaBcu1.Text = textRSTUnbaBcu1.Text;
            textWCellVHBcu1.Text = textRCellVHBcu1.Text;
            textWCellVLBcu1.Text = textRCellVLBcu1.Text;
            textWChgCurHBcu1.Text = textRChgCurHBcu1.Text;
            textWdChgCurHBcu1.Text = textRdChgCurHBcu1.Text;
            textWChgMaxSumv.Text = textRChgMaxSumv.Text;
            textWSocHBcu1.Text = textRSocHBcu1.Text;
            textWSocLBcu1.Text = textRSocLBcu1.Text;
            textWChgStep.Text = textRChgStep.Text;
            textWResHBcu1.Text = textRResHBcu1.Text;
            textWPoleTHBcu1.Text = textRPoleTHBcu1.Text;
            textWSohHBcu1.Text = textRSohHBcu1.Text;

            textWSumvHBcu2.Text = textRSumvHBcu2.Text;
            textWSumvLBcu2.Text = textRSumvLBcu2.Text;
            textWTempHBcu2.Text = textRTempHBcu2.Text;
            textWTempLBcu2.Text = textRTempLBcu2.Text;
            textWAVUnbaBcu2.Text = textRAVUnbaBcu2.Text;
            textWSVUnbaBcu2.Text = textRSVUnbaBcu2.Text;
            textWATUnbaBcu2.Text = textRATUnbaBcu2.Text;
            textWSTUnbaBcu2.Text = textRSTUnbaBcu2.Text;
            textWCellVHBcu2.Text = textRCellVHBcu2.Text;
            textWCellVLBcu2.Text = textRCellVLBcu2.Text;
            textWChgCurHBcu2.Text = textRChgCurHBcu2.Text;
            textWdChgCurHBcu2.Text = textRdChgCurHBcu2.Text;
            textWChgMaxSumv.Text = textRChgMaxSumv.Text;
            textWSocHBcu2.Text = textRSocHBcu2.Text;
            textWSocLBcu2.Text = textRSocLBcu2.Text;
            textWChgStep.Text = textRChgStep.Text;
            textWResHBcu2.Text = textRResHBcu2.Text;
            textWPoleTHBcu2.Text = textRPoleTHBcu2.Text;
            textWSohHBcu2.Text = textRSohHBcu2.Text;


            textWSumvHBcu3.Text = textRSumvHBcu3.Text;
            textWSumvLBcu3.Text = textRSumvLBcu3.Text;
            textWTempHBcu3.Text = textRTempHBcu3.Text;
            textWTempLBcu3.Text = textRTempLBcu3.Text;
            textWAVUnbaBcu3.Text = textRAVUnbaBcu3.Text;
            textWSVUnbaBcu3.Text = textRSVUnbaBcu3.Text;
            textWATUnbaBcu3.Text = textRATUnbaBcu3.Text;
            textWSTUnbaBcu3.Text = textRSTUnbaBcu3.Text;
            textWCellVHBcu3.Text = textRCellVHBcu3.Text;
            textWCellVLBcu3.Text = textRCellVLBcu3.Text;
            textWChgCurHBcu3.Text = textRChgCurHBcu3.Text;
            textWdChgCurHBcu3.Text = textRdChgCurHBcu3.Text;
            textWChgMaxSumv.Text = textRChgMaxSumv.Text;
            textWSocHBcu3.Text = textRSocHBcu3.Text;
            textWSocLBcu3.Text = textRSocLBcu3.Text;
            textWChgStep.Text = textRChgStep.Text;
            textWResHBcu3.Text = textRResHBcu3.Text;
            textWPoleTHBcu3.Text = textRPoleTHBcu3.Text;
            textWSohHBcu3.Text = textRSohHBcu3.Text;


            textWBatNumBcu.Text = textRBatNumBcu.Text;
            textWBatParBcu.Text = textRBatParBcu.Text;
            textWRateAhBcu.Text = textRRateAhBcu.Text;
            textWBatEnergyBcu.Text = textRBatEnergyBcu.Text;
            textWRateSumvBcu.Text = textRRateSumvBcu.Text;
            textWBatRealAhBcu.Text = textRBatRealAhBcu.Text;

            comboBoxWBatType.SelectedIndex = comboBoxRBatType.SelectedIndex;


            textWInAhBcu.Text = textRInAhBcu.Text;
            textWOutAhBcu.Text = textROutAhBcu.Text;
            textWEmptyTimesBcu.Text = textREmptyTimesBcu.Text;
            textWFullTimesBcu.Text = textRFullTimesBcu.Text;
            textWRateSumvBcu.Text = textRRateSumvBcu.Text;
            textWChgTimesBcu.Text = textRChgTimesBcu.Text;

            this.textWBmuNumBcu.TextChanged -= new EventHandler(textWBmuNumBcu_TextChanged);
            textWBmuNumBcu.Text = textRBmuNumBcu.Text;


            for (byte k = 0; k < 30; k++)
            {
                dataGridView1.Rows[k].Cells[0].Value = dataGridView2.Rows[k].Cells[0].Value.ToString();
                dataGridView1.Rows[k].Cells[1].Value = dataGridView2.Rows[k].Cells[1].Value.ToString();
                dataGridView1.Rows[k].Cells[2].Value = dataGridView2.Rows[k].Cells[2].Value.ToString();
            }

            this.textWBmuNumBcu.TextChanged += new EventHandler(textWBmuNumBcu_TextChanged);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }

        private void btnSaveBcu_Click(object sender, EventArgs e)
        {
            string temp;
            string[] num = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            saveFileDialog1.Filter = "BCU配置文件(.ini)|*.ini";
            saveFileDialog1.FileName = "BCU.ini";
           
            string fName;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fName = saveFileDialog1.FileName;
                    textConfigPath.Text = fName;

                    FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    fs.Close();
                    iniFile.IniWriteValue(fName, "总信息", "工号", "BNET");
                    iniFile.IniWriteValue(fName, "总信息", "工程代号", textProject.Text);
                    iniFile.IniWriteValue(fName, "总信息", "MD5码", " \"MD5码\"");

                    iniFile.IniWriteValue(fName, "主板", "从板个数", textWBmuNumBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池额定容量", textWAhBcu.Text.ToString());
                    iniFile.IniWriteValue(fName, "主板", "电池实际容量", textWRealAhBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC", textWSocBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOH", textWSohBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "产品编号", textWProNoBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "车架(牌)号", textWCarNoBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "配置文件版本", textWConfigVerBcu.Text);

                    for (byte j = 0; j < byte.Parse(textWBmuNumBcu.Text); j++)
                    {
                        temp = "从板" + (j + 1).ToString() + "电池节数";
                        iniFile.IniWriteValue(fName, "主板", temp, dataGridView1.Rows[j].Cells[1].Value.ToString());
                    }

                    for (byte j = 0; j < byte.Parse(textWBmuNumBcu.Text); j++)
                    {
                        temp = "从板" + (j + 1).ToString() + "温度个数";
                        iniFile.IniWriteValue(fName, "主板", temp, dataGridView1.Rows[j].Cells[2].Value.ToString());
                    }

                    iniFile.IniWriteValue(fName, "主板", "总电压过高一级", textWSumvHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "总电压过低一级", textWSumvLBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过高一级", textWTempHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过低一级", textWTempLBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组电压不均衡一级", textWAVUnbaBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组电压不均衡一级", textWSVUnbaBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组温度不均衡一级", textWATUnbaBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组温度不均衡一级", textWSTUnbaBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过高一级", textWCellVHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过低一级", textWCellVLBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "充电电流过高一级", textWChgCurHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "放电电流过高一级", textWdChgCurHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过高一级", textWSocHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过低一级", textWSocLBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "内阻过大一级", textWResHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "极柱温度过高一级", textWPoleTHBcu1.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOH过低一级", textWSohHBcu1.Text);

                    iniFile.IniWriteValue(fName, "主板", "总电压过高二级", textWSumvHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "总电压过低二级", textWSumvLBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过高二级", textWTempHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过低二级", textWTempLBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组电压不均衡二级", textWAVUnbaBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组电压不均衡二级", textWSVUnbaBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组温度不均衡二级", textWATUnbaBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组温度不均衡二级", textWSTUnbaBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过高二级", textWCellVHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过低二级", textWCellVLBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "充电电流过高二级", textWChgCurHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "放电电流过高二级", textWdChgCurHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过高二级", textWSocHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过低二级", textWSocLBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "内阻过大二级", textWResHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "极柱温度过高二级", textWPoleTHBcu2.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOH过低二级", textWSohHBcu2.Text);

                    iniFile.IniWriteValue(fName, "主板", "总电压过高三级", textWSumvHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "总电压过低三级", textWSumvLBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过高三级", textWTempHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "温度过低三级", textWTempLBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组电压不均衡三级", textWAVUnbaBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组电压不均衡三级", textWSVUnbaBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "整组温度不均衡三级", textWATUnbaBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "单组温度不均衡三级", textWSTUnbaBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过高三级", textWCellVHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "单体电压过低三级", textWCellVLBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "充电电流过高三级", textWChgCurHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "放电电流过高三级", textWdChgCurHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过高三级", textWSocHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOC过低三级", textWSocLBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "内阻过大三级", textWResHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "极柱温度过高三级", textWPoleTHBcu3.Text);
                    iniFile.IniWriteValue(fName, "主板", "SOH过低三级", textWSohHBcu3.Text);

                    iniFile.IniWriteValue(fName, "主板", "风机开启温度", textWFanOnTempBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "风机关闭温度", textWFanOffTempBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "加热开启温度", textWHeatOnTempBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "加热关闭温度", textWHeatOffTempBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "风机开启温差", textWFanOnDiffBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "风机关闭温差", textWFanOffDiffBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "加热关闭温差", textWHeatOffDiffBcu.Text);


                    iniFile.IniWriteValue(fName, "主板", "最大允许单体电压", textWChgMaxCellV.Text);
                    iniFile.IniWriteValue(fName, "主板", "最小允许单体电压", textWChgMinCellV.Text);
                    iniFile.IniWriteValue(fName, "主板", "最大允许充电电流", textWChgMaxCurr.Text);
                    iniFile.IniWriteValue(fName, "主板", "最小允许充电电流", textWChgMinCurr.Text);
                    iniFile.IniWriteValue(fName, "主板", "最大允许总电压", textWChgMaxSumv.Text);
                    iniFile.IniWriteValue(fName, "主板", "最大允许充电温度", textWChgMaxt.Text);
                    iniFile.IniWriteValue(fName, "主板", "最小允许充电温度", textWChgMint.Text);
                    iniFile.IniWriteValue(fName, "主板", "电流步长", textWChgStep.Text);

                    iniFile.IniWriteValue(fName, "主板", "电池串联", textWBatNumBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池并联", textWBatParBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池包额定容量", textWRateAhBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池包额定能量(kWh)", textWBatEnergyBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池包额定总电压", textWRateSumvBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "实际容量", textWBatRealAhBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池厂家", "");

                    switch (comboBoxWBatType.SelectedIndex)
                    {
                        case 0:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "磷酸铁锂");
                                break;
                            }
                        case 1:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "锰酸锂");
                                break;
                            }
                        case 2:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "三元材料");
                                break;
                            }
                        case 3:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "铅酸");
                                break;
                            }
                        case 4:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "镍氢");
                                break;
                            }
                        case 5:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "钴酸锂");
                                break;
                            }
                        case 6:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "聚合物锂离子");
                                break;
                            }
                        case 7:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "钛酸锂");
                                break;
                            }
                        case 8:
                            {
                                iniFile.IniWriteValue(fName, "主板", "电池类型", "其他电池");
                                break;
                            }
                    }
                    iniFile.IniWriteValue(fName, "主板", "电池生产日期", "2016-01-01 01:01:01");
                    iniFile.IniWriteValue(fName, "主板", "电池包组号", "1");

                    iniFile.IniWriteValue(fName, "主板", "累计充电总安时", textWInAhBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "累计放电总安时", textWOutAhBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池放空次数", textWEmptyTimesBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池充满次数", textWFullTimesBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "电池充电次数", textWChgTimesBcu.Text);
                    iniFile.IniWriteValue(fName, "主板", "分流器型号", "4");

                    FileStream fs1 = new FileStream(fName, FileMode.Open, FileAccess.ReadWrite);
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] data = md5.ComputeHash(fs1);

                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    fs1.Close();

                    iniFile.IniWriteValue(fName, "总信息", "MD5码", sBuilder.ToString());
                }
        }

        private void btnSaveBmu_Click(object sender, EventArgs e)
        {
            string temp;
            string[] num = { "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
            saveFileDialog1.Filter = "BMU配置文件(.ini)|*.ini";
            saveFileDialog1.FileName = "BMU" + textCurrBmuNo.Text;
            string fName;




                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fName = saveFileDialog1.FileName;
                    textConfigPath.Text = fName;

                    FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    fs.Close();
                    iniFile.IniWriteValue(fName, "总信息", "工号", "BNET");
                    iniFile.IniWriteValue(fName, "总信息", "工程代号", textProject.Text);
                    iniFile.IniWriteValue(fName, "总信息", "MD5码", " \"MD5码\"");

                    /////////////////////////////////////////////////////////////
                    try
                    {
                            byte i = byte.Parse(textCurrBmuNo.Text);

                            temp = "从板" + i.ToString();
                            iniFile.IniWriteValue(fName, temp, "从板板号", i.ToString());

                            iniFile.IniWriteValue(fName, temp, "电压个数", textWVolNum.Text);
                            iniFile.IniWriteValue(fName, temp, "温度个数", textWTempNum.Text);

                            iniFile.IniWriteValue(fName, temp, "第" + num[0] + "个保险通道", textWFuseChan1.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[1] + "个保险通道", textWFuseChan2.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[2] + "个保险通道", textWFuseChan3.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[3] + "个保险通道", textWFuseChan4.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[4] + "个保险通道", textWFuseChan5.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[5] + "个保险通道", textWFuseChan6.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[6] + "个保险通道", textWFuseChan7.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[7] + "个保险通道", textWFuseChan8.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[8] + "个保险通道", textWFuseChan9.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[9] + "个保险通道", textWFuseChan10.Text);

                            iniFile.IniWriteValue(fName, temp, "第" + num[0] + "个保险位置", textWFuseLoc1.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[1] + "个保险位置", textWFuseLoc2.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[2] + "个保险位置", textWFuseLoc3.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[3] + "个保险位置", textWFuseLoc4.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[4] + "个保险位置", textWFuseLoc5.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[5] + "个保险位置", textWFuseLoc6.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[6] + "个保险位置", textWFuseLoc7.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[7] + "个保险位置", textWFuseLoc8.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[8] + "个保险位置", textWFuseLoc9.Text);
                            iniFile.IniWriteValue(fName, temp, "第" + num[9] + "个保险位置", textWFuseLoc10.Text);

                            iniFile.IniWriteValue(fName, temp, "第1通道电压个数", textWVolNumChan1.Text);
                            iniFile.IniWriteValue(fName, temp, "第2通道电压个数", textWVolNumChan2.Text);
                            iniFile.IniWriteValue(fName, temp, "第3通道电压个数", textWVolNumChan3.Text);
                            iniFile.IniWriteValue(fName, temp, "第4通道电压个数", textWVolNumChan4.Text);
                            iniFile.IniWriteValue(fName, temp, "第5通道电压个数", textWVolNumChan5.Text);

                            iniFile.IniWriteValue(fName, temp, "风机开启温度", textFanOnTemp.Text);
                            iniFile.IniWriteValue(fName, temp, "风机关闭温度", textFanOffTemp.Text);
                            iniFile.IniWriteValue(fName, temp, "加热开启温度", textHeatOnTemp.Text);
                            iniFile.IniWriteValue(fName, temp, "加热关闭温度", textHeatOffTemp.Text);
                            iniFile.IniWriteValue(fName, temp, "风机开启温差", textFanOnDiff.Text);
                            iniFile.IniWriteValue(fName, temp, "风机关闭温差", textFanOffDiff.Text);
                            iniFile.IniWriteValue(fName, temp, "加热关闭温差", textHeatOffDiff.Text);

                            iniFile.IniWriteValue(fName, temp, "电压过高" + num[0] + "级",textWVolHigh1 .Text);
                            iniFile.IniWriteValue(fName, temp, "电压过低" + num[0] + "级", textWVolLow1.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过高" + num[0] + "级", textWTempHigh1.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过低" + num[0] + "级", textWTempLow1.Text);
                            iniFile.IniWriteValue(fName, temp, "电压不均衡" + num[0] + "级", textWVolUnbal1.Text);
                            iniFile.IniWriteValue(fName, temp, "温度不均衡" + num[0] + "级", textWTempUnbal1.Text);
                            iniFile.IniWriteValue(fName, temp, "SOC过低" + num[0] + "级", "0");

                            iniFile.IniWriteValue(fName, temp, "电压过高" + num[1] + "级", textWVolHigh2.Text);
                            iniFile.IniWriteValue(fName, temp, "电压过低" + num[1] + "级", textWVolLow2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过高" + num[1] + "级", textWTempHigh2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过低" + num[1] + "级", textWTempLow2.Text);
                            iniFile.IniWriteValue(fName, temp, "电压不均衡" + num[1] + "级", textWVolUnbal2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度不均衡" + num[1] + "级", textWTempUnbal2.Text);
                            iniFile.IniWriteValue(fName, temp, "SOC过低" + num[1] + "级", "0");

                            iniFile.IniWriteValue(fName, temp, "电压过高" + num[2] + "级", textWVolHigh2.Text);
                            iniFile.IniWriteValue(fName, temp, "电压过低" + num[2] + "级", textWVolLow2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过高" + num[2] + "级", textWTempHigh2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度过低" + num[2] + "级", textWTempLow2.Text);
                            iniFile.IniWriteValue(fName, temp, "电压不均衡" + num[2] + "级", textWVolUnbal2.Text);
                            iniFile.IniWriteValue(fName, temp, "温度不均衡" + num[2] + "级", textWTempUnbal2.Text);
                            iniFile.IniWriteValue(fName, temp, "SOC过低" + num[2] + "级", "0");

                        FileStream fs1 = new FileStream(fName, FileMode.Open, FileAccess.ReadWrite);
                        MD5 md5 = new MD5CryptoServiceProvider();
                        byte[] data = md5.ComputeHash(fs1);

                        StringBuilder sBuilder = new StringBuilder();
                        for (int k = 0; k< data.Length; k++)
                        {
                            sBuilder.Append(data[k].ToString("x2"));
                        }

                        fs1.Close();

                        iniFile.IniWriteValue(fName, "总信息", "MD5码", sBuilder.ToString());
                    }
                    catch
                    {
                        MessageBox.Show("保存失败");
                        return;
                    }


                return;
            }
        }

        private void textBoardType_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                tabControl1.SelectedIndex = 1;
            }
            else 
            {
                tabControl1.SelectedIndex = 0;
            }
        }

        private void btnJudge_Click(object sender, EventArgs e)
        {
            bool judgeOk = true;

           #region Judge_Bcu
            if (tabControl1.SelectedIndex == 0)
            {
                //BCU
                if (textRSocBcu.Text == textWSocBcu.Text)
                {
                    textRSocBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocBcu.BackColor = Color.Red;
                    judgeOk = false;
                }

                if (textRSohBcu.Text == textWSohBcu.Text)
                {
                    textRSohBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSohBcu.BackColor = Color.Red;
                    judgeOk = false;
                }


                if (textRAhBcu.Text == textWAhBcu.Text)
                {
                    textRAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }


                if (textRRealAhBcu.Text == textWRealAhBcu.Text)
                {
                    textRRealAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRRealAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }

                if (textRProNoBcu.Text == textWProNoBcu.Text)
                {
                    textRProNoBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRProNoBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCarNoBcu.Text == textWCarNoBcu.Text)
                {
                    textRCarNoBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCarNoBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRConfigVerBcu.Text == textWConfigVerBcu.Text)
                {
                    textRConfigVerBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRConfigVerBcu.BackColor = Color.Red;
                    judgeOk = false;
                }

                /////////////////////////
                if (textRFanOnTempBcu.Text == textWFanOnTempBcu.Text)
                {
                    textRFanOnTempBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOnTempBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOffTempBcu.Text == textWFanOffTempBcu.Text)
                {
                    textRFanOffTempBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOffTempBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOnDiffBcu.Text == textWFanOnDiffBcu.Text)
                {
                    textRFanOnDiffBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOnDiffBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOffDiffBcu.Text == textWFanOffDiffBcu.Text)
                {
                    textRFanOffDiffBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOffDiffBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOnTempBcu.Text == textWHeatOnTempBcu.Text)
                {
                    textRHeatOnTempBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOnTempBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOffTempBcu.Text == textWHeatOffTempBcu.Text)
                {
                    textRHeatOffTempBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOffTempBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOffDiffBcu.Text == textWHeatOffDiffBcu.Text)
                {
                    textRHeatOffDiffBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOffDiffBcu.BackColor = Color.Red;
                    judgeOk = false;
                }

                //////////////////////////////////////
                if (textRChgMaxCellV.Text == textWChgMaxCellV.Text)
                {
                    textRChgMaxCellV.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMaxCellV.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMinCellV.Text == textWChgMinCellV.Text)
                {
                    textRChgMinCellV.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMinCellV.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMaxCurr.Text == textWChgMaxCurr.Text)
                {
                    textRChgMaxCurr.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMaxCurr.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMinCurr.Text == textWChgMinCurr.Text)
                {
                    textRChgMinCurr.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMinCurr.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMaxSumv.Text == textWChgMaxSumv.Text)
                {
                    textRChgMaxSumv.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMaxSumv.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMaxt.Text == textWChgMaxt.Text)
                {
                    textRChgMaxt.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMaxt.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgMint.Text == textWChgMint.Text)
                {
                    textRChgMint.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgMint.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgStep.Text == textWChgStep.Text)
                {
                    textRChgStep.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgStep.BackColor = Color.Red;
                    judgeOk = false;
                }
                ////////////////////////////////
                if (textRBmuNumBcu.Text == textWBmuNumBcu.Text)
                {
                    textRBmuNumBcu.BackColor = Color.FromArgb(192, 255, 192);

                    for (byte k = 0; k < byte.Parse(textRBmuNumBcu.Text); k++)
                    {
                        if (dataGridView1.Rows[k].Cells[1].Value.ToString() == dataGridView2.Rows[k].Cells[1].Value.ToString())
                        {
                            dataGridView2.Rows[k].Cells[1].Style.BackColor = Color.FromArgb(192, 255, 192);
                        }
                        else
                        {
                            dataGridView2.Rows[k].Cells[1].Style.BackColor = Color.Red;
                        }

                        if (dataGridView1.Rows[k].Cells[2].Value.ToString() == dataGridView2.Rows[k].Cells[2].Value.ToString())
                        {
                            dataGridView2.Rows[k].Cells[2].Style.BackColor = Color.FromArgb(192, 255, 192);
                        }
                        else
                        {
                            dataGridView2.Rows[k].Cells[2].Style.BackColor = Color.Red;
                        }
                    }
                }
                else
                {
                    textRBmuNumBcu.BackColor = Color.Red;
                    judgeOk = false;
                }

                ////////////////////////////////////////
                if (textRSumvHBcu1.Text == textWSumvHBcu1.Text)
                {
                    textRSumvHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSumvLBcu1.Text == textWSumvLBcu1.Text)
                {
                    textRSumvLBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvLBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHBcu1.Text == textWTempHBcu1.Text)
                {
                    textRTempHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLBcu1.Text == textWTempLBcu1.Text)
                {
                    textRTempLBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRAVUnbaBcu1.Text == textWAVUnbaBcu1.Text)
                {
                    textRAVUnbaBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRAVUnbaBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSVUnbaBcu1.Text == textWSVUnbaBcu1.Text)
                {
                    textRSVUnbaBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSVUnbaBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRATUnbaBcu1.Text == textWATUnbaBcu1.Text)
                {
                    textRATUnbaBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRATUnbaBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSTUnbaBcu1.Text == textWSTUnbaBcu1.Text)
                {
                    textRSTUnbaBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSTUnbaBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVHBcu1.Text == textWCellVHBcu1.Text)
                {
                    textRCellVHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVLBcu1.Text == textWCellVLBcu1.Text)
                {
                    textRCellVLBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVLBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgCurHBcu1.Text == textWChgCurHBcu1.Text)
                {
                    textRChgCurHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgCurHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRdChgCurHBcu1.Text == textWdChgCurHBcu1.Text)
                {
                    textRdChgCurHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRdChgCurHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocHBcu1.Text == textWSocHBcu1.Text)
                {
                    textRSocHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocLBcu1.Text == textWSocLBcu1.Text)
                {
                    textRSocLBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocLBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRResHBcu1.Text == textWResHBcu1.Text)
                {
                    textRResHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRResHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRPoleTHBcu1.Text == textWPoleTHBcu1.Text)
                {
                    textRPoleTHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRPoleTHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSohHBcu1.Text == textWSohHBcu1.Text)
                {
                    textRSohHBcu1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSohHBcu1.BackColor = Color.Red;
                    judgeOk = false;
                }

                ////////////////////////////////////
                ////////////////////////////////////////
                if (textRSumvHBcu2.Text == textWSumvHBcu2.Text)
                {
                    textRSumvHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSumvLBcu2.Text == textWSumvLBcu2.Text)
                {
                    textRSumvLBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvLBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHBcu2.Text == textWTempHBcu2.Text)
                {
                    textRTempHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLBcu2.Text == textWTempLBcu2.Text)
                {
                    textRTempLBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRAVUnbaBcu2.Text == textWAVUnbaBcu2.Text)
                {
                    textRAVUnbaBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRAVUnbaBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSVUnbaBcu2.Text == textWSVUnbaBcu2.Text)
                {
                    textRSVUnbaBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSVUnbaBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRATUnbaBcu2.Text == textWATUnbaBcu2.Text)
                {
                    textRATUnbaBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRATUnbaBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSTUnbaBcu2.Text == textWSTUnbaBcu2.Text)
                {
                    textRSTUnbaBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSTUnbaBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVHBcu2.Text == textWCellVHBcu2.Text)
                {
                    textRCellVHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVLBcu2.Text == textWCellVLBcu2.Text)
                {
                    textRCellVLBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVLBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgCurHBcu2.Text == textWChgCurHBcu2.Text)
                {
                    textRChgCurHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgCurHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRdChgCurHBcu2.Text == textWdChgCurHBcu2.Text)
                {
                    textRdChgCurHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRdChgCurHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocHBcu2.Text == textWSocHBcu2.Text)
                {
                    textRSocHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocLBcu2.Text == textWSocLBcu2.Text)
                {
                    textRSocLBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocLBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRResHBcu2.Text == textWResHBcu2.Text)
                {
                    textRResHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRResHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRPoleTHBcu2.Text == textWPoleTHBcu2.Text)
                {
                    textRPoleTHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRPoleTHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSohHBcu2.Text == textWSohHBcu2.Text)
                {
                    textRSohHBcu2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSohHBcu2.BackColor = Color.Red;
                    judgeOk = false;
                }
                /////////////////////////////
                if (textRSumvHBcu3.Text == textWSumvHBcu3.Text)
                {
                    textRSumvHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSumvLBcu3.Text == textWSumvLBcu3.Text)
                {
                    textRSumvLBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSumvLBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHBcu3.Text == textWTempHBcu3.Text)
                {
                    textRTempHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLBcu3.Text == textWTempLBcu3.Text)
                {
                    textRTempLBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRAVUnbaBcu3.Text == textWAVUnbaBcu3.Text)
                {
                    textRAVUnbaBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRAVUnbaBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSVUnbaBcu3.Text == textWSVUnbaBcu3.Text)
                {
                    textRSVUnbaBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSVUnbaBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRATUnbaBcu3.Text == textWATUnbaBcu3.Text)
                {
                    textRATUnbaBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRATUnbaBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSTUnbaBcu3.Text == textWSTUnbaBcu3.Text)
                {
                    textRSTUnbaBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSTUnbaBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVHBcu3.Text == textWCellVHBcu3.Text)
                {
                    textRCellVHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRCellVLBcu3.Text == textWCellVLBcu3.Text)
                {
                    textRCellVLBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRCellVLBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgCurHBcu3.Text == textWChgCurHBcu3.Text)
                {
                    textRChgCurHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgCurHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRdChgCurHBcu3.Text == textWdChgCurHBcu3.Text)
                {
                    textRdChgCurHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRdChgCurHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocHBcu3.Text == textWSocHBcu3.Text)
                {
                    textRSocHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSocLBcu3.Text == textWSocLBcu3.Text)
                {
                    textRSocLBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSocLBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRResHBcu3.Text == textWResHBcu3.Text)
                {
                    textRResHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRResHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRPoleTHBcu3.Text == textWPoleTHBcu3.Text)
                {
                    textRPoleTHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRPoleTHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRSohHBcu3.Text == textWSohHBcu3.Text)
                {
                    textRSohHBcu3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRSohHBcu3.BackColor = Color.Red;
                    judgeOk = false;
                }
                /////////////////////////////////////////////
                if (textRBatNumBcu.Text == textWBatNumBcu.Text)
                {
                    textRBatNumBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRBatNumBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRBatParBcu.Text == textWBatParBcu.Text)
                {
                    textRBatParBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRBatParBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRRateAhBcu.Text == textWRateAhBcu.Text)
                {
                    textRRateAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRRateAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRBatEnergyBcu.Text == textWBatEnergyBcu.Text)
                {
                    textRBatEnergyBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRBatEnergyBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRBatRealAhBcu.Text == textWBatRealAhBcu.Text)
                {
                    textRBatRealAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRBatRealAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRRateSumvBcu.Text == textWRateSumvBcu.Text)
                {
                    textRRateSumvBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRRateSumvBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (comboBoxRBatType.SelectedIndex == comboBoxWBatType.SelectedIndex)
                {
                    comboBoxRBatType.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    comboBoxRBatType.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRInAhBcu.Text == textWInAhBcu.Text)
                {
                    textRInAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRInAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textROutAhBcu.Text == textWOutAhBcu.Text)
                {
                    textROutAhBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textROutAhBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRChgTimesBcu.Text == textWChgTimesBcu.Text)
                {
                    textRChgTimesBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRChgTimesBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFullTimesBcu.Text == textWFullTimesBcu.Text)
                {
                    textRFullTimesBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFullTimesBcu.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textREmptyTimesBcu.Text == textWEmptyTimesBcu.Text)
                {
                    textREmptyTimesBcu.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textREmptyTimesBcu.BackColor = Color.Red;
                    judgeOk = false;
                }


            }
#endregion






             #region Judge_BMU
            if (tabControl1.SelectedIndex == 1)
            {
                //BMU
                if (textRVolNum.Text == textWVolNum.Text)
                {
                    textRVolNum.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNum.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempNum.Text == textWTempNum.Text)
                {
                    textRTempNum.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempNum.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolNumChan1.Text == textWVolNumChan1.Text)
                {
                    textRVolNumChan1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNumChan1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolNumChan2.Text == textWVolNumChan2.Text)
                {
                    textRVolNumChan2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNumChan2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolNumChan3.Text == textWVolNumChan3.Text)
                {
                    textRVolNumChan3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNumChan3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolNumChan4.Text == textWVolNumChan4.Text)
                {
                    textRVolNumChan4.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNumChan4.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolNumChan5.Text == textWVolNumChan5.Text)
                {
                    textRVolNumChan5.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolNumChan5.BackColor = Color.Red;
                    judgeOk = false;
                }

                if (textRFanOnTemp.Text == textFanOnTemp.Text)
                {
                    textRFanOnTemp.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOnTemp.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOffTemp.Text == textFanOffTemp.Text)
                {
                    textRFanOffTemp.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOffTemp.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOnDiff.Text == textFanOnDiff.Text)
                {
                    textRFanOnDiff.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOnDiff.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFanOffDiff.Text == textFanOffDiff.Text)
                {
                    textRFanOffDiff.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFanOffDiff.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOnTemp.Text == textHeatOnTemp.Text)
                {
                    textRHeatOnTemp.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOnTemp.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOffTemp.Text == textHeatOffTemp.Text)
                {
                    textRHeatOffTemp.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOffTemp.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRHeatOffDiff.Text == textHeatOffDiff.Text)
                {
                    textRHeatOffDiff.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRHeatOffDiff.BackColor = Color.Red;
                    judgeOk = false;
                }
                ///////////////////////
                if (textRFuseChan1.Text == textWFuseChan1.Text)
                {
                    textRFuseChan1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan1.BackColor = Color.Red;
                    judgeOk = false;
                }

                if (textRFuseChan2.Text == textWFuseChan2.Text)
                {
                    textRFuseChan2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan3.Text == textWFuseChan3.Text)
                {
                    textRFuseChan3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan4.Text == textWFuseChan4.Text)
                {
                    textRFuseChan4.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan4.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan5.Text == textWFuseChan5.Text)
                {
                    textRFuseChan5.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan5.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan6.Text == textWFuseChan6.Text)
                {
                    textRFuseChan6.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan6.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan7.Text == textWFuseChan7.Text)
                {
                    textRFuseChan7.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan7.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan8.Text == textWFuseChan8.Text)
                {
                    textRFuseChan8.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan8.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan9.Text == textWFuseChan9.Text)
                {
                    textRFuseChan9.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan9.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseChan10.Text == textWFuseChan10.Text)
                {
                    textRFuseChan10.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseChan10.BackColor = Color.Red;
                    judgeOk = false;
                }
                ////////////////////

                if (textRFuseLoc1.Text== textWFuseLoc1.Text)
                {
                    textRFuseLoc1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc1.BackColor = Color.Red;
                    judgeOk = false;
                }

                if (textRFuseLoc2.Text == textWFuseLoc2.Text)
                {
                    textRFuseLoc2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc3.Text == textWFuseLoc3.Text)
                {
                    textRFuseLoc3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc4.Text == textWFuseLoc4.Text)
                {
                    textRFuseLoc4.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc4.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc5.Text == textWFuseLoc5.Text)
                {
                    textRFuseLoc5.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc5.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc6.Text == textWFuseLoc6.Text)
                {
                    textRFuseLoc6.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc6.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc7.Text == textWFuseLoc7.Text)
                {
                    textRFuseLoc7.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc7.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc8.Text == textWFuseLoc8.Text)
                {
                    textRFuseLoc8.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc8.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc9.Text == textWFuseLoc9.Text)
                {
                    textRFuseLoc9.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc9.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRFuseLoc10.Text == textWFuseLoc10.Text)
                {
                    textRFuseLoc10.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRFuseLoc10.BackColor = Color.Red;
                    judgeOk = false;
                }
                /////////////////////////////////////////
                if (textRVolHigh1.Text == textWVolHigh1.Text)
                {
                    textRVolHigh1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolHigh1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolLow1.Text == textWVolLow1.Text)
                {
                    textRVolLow1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolLow1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolUnbal1.Text == textWVolUnbal1.Text)
                {
                    textRVolUnbal1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolUnbal1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHigh1.Text == textWTempHigh1.Text)
                {
                    textRTempHigh1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHigh1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLow1.Text == textWTempLow1.Text)
                {
                    textRTempLow1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLow1.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempUnbal1.Text == textWTempUnbal1.Text)
                {
                    textRTempUnbal1.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempUnbal1.BackColor = Color.Red;
                    judgeOk = false;
                }
                /////////////////////////////////////////
                if (textRVolHigh2.Text == textWVolHigh2.Text)
                {
                    textRVolHigh2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolHigh2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolLow2.Text == textWVolLow2.Text)
                {
                    textRVolLow2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolLow2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolUnbal2.Text == textWVolUnbal2.Text)
                {
                    textRVolUnbal2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolUnbal2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHigh2.Text == textWTempHigh2.Text)
                {
                    textRTempHigh2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHigh2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLow2.Text == textWTempLow2.Text)
                {
                    textRTempLow2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLow2.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempUnbal2.Text == textWTempUnbal2.Text)
                {
                    textRTempUnbal2.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempUnbal2.BackColor = Color.Red;
                    judgeOk = false;
                }
                /////////////////////////////////////////
                if (textRVolHigh3.Text == textWVolHigh3.Text)
                {
                    textRVolHigh3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolHigh3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolLow3.Text == textWVolLow3.Text)
                {
                    textRVolLow3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolLow3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRVolUnbal3.Text == textWVolUnbal3.Text)
                {
                    textRVolUnbal3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRVolUnbal3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempHigh3.Text == textWTempHigh3.Text)
                {
                    textRTempHigh3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempHigh3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempLow3.Text == textWTempLow3.Text)
                {
                    textRTempLow3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempLow3.BackColor = Color.Red;
                    judgeOk = false;
                }
                if (textRTempUnbal3.Text == textWTempUnbal3.Text)
                {
                    textRTempUnbal3.BackColor = Color.FromArgb(192, 255, 192);
                }
                else
                {
                    textRTempUnbal3.BackColor = Color.Red;
                    judgeOk = false;
                }

            }

            if (judgeOk == true)
            {
                MessageBox.Show("比对成功");
            }
            else
            {
                MessageBox.Show("比对失败");
            }
             #endregion

        }

        private void dataGridView2_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

            e.Cancel = true;
            e.ThrowException =true;
            MessageBox.Show("1");
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}