//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于软件运行信息
//              每次程序运行，将自动在运行文件目录/Log 文件中生成以时间为文件名的txt记录文件
//              使用LogInfo(string info)在该文件末尾增加一条记录
//
//*************************************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Driver.File.Txt;

namespace Driver.File.Log
{
   /// <summary>
   /// 用于生成运行信息
   /// </summary>
    public class LogFileClass
    {
        string logpath = "";
        string logpath1 = "";
        string time = "";

        TxtFileClass txtFile = new TxtFileClass();

         /// <summary>
         /// 实例化时以日期自动创建文件路径
         /// </summary>
        public LogFileClass()
        {
            time = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-");
           // time +=" " + DateTime.Now.ToShortTimeString().Replace(":", "-");

            logpath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Log\\" +time +".txt";
            logpath1 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Error\\" + time + ".txt";

            string logdirpath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Log";
            string logdirpath1 = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Error";


            if (System.IO.Directory.Exists(@logdirpath) == false)
            {
                System.IO.Directory.CreateDirectory(@logdirpath);
            }
            if (System.IO.File.Exists(@logpath ) == false)
            {
                System.IO.File.Create(@logpath).Close();         
            }

            if (System.IO.Directory.Exists(@logdirpath1) == false)
            {
                System.IO.Directory.CreateDirectory(@logdirpath1);
            }
            if (System.IO.File.Exists(@logpath1) == false)
            {
                System.IO.File.Create(@logpath1).Close();
            }

        }
        /// <summary>
        /// 自动在运行文件目录/Log 文件中生成以运行exe时间为文件名的txt记录文件
        /// 并在该文件末尾增加一条记录
        /// </summary>
        public void LogHistory(string info)
        {
            time = DateTime.Now.ToShortDateString();
            time += " " + DateTime.Now.ToLongTimeString();
            info = time + ":  " + info+"\r";
            txtFile.TxtWriteLine(logpath, info);
        }

        public void LogError(string info)
        {
            time = DateTime.Now.ToShortDateString();
            time += " " + DateTime.Now.ToLongTimeString();
            info = time + ":  " + info+"\r";
            txtFile.TxtWriteLine(logpath1, info);
        }

        public  string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");

                }
            }

            return returnStr;
        }
    }
}
