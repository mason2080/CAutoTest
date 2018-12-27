//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于csv文件格式文件的简易读写功能
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Driver.File.Csv
{
    public class CsvFileClass
    {
        //public string[] TxtReadAll(string path)
        //{
        //    string[] read=new string[100];
        //    FileStream filest = new FileStream(@path, FileMode.Open, FileAccess.ReadWrite);
        //    StreamReader sr = new StreamReader(filest);

        //    for (uint i = 0; i < 100; i++)
        //    { 
        //        read[i]=sr.ReadLine();
        //    }
        //    //string str = sr.ReadToEnd();
        //    sr.Close();
        //    filest.Close();
        //    return read;
        //}

        public void CsvWriteLine(string path, string info)
        {
            //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";
            string info1="";

            //string [] data=new string[100];
            //foreach(string s in data)
            //{
            //    //info1 +=( "aa" + ",");
            //}
            try
            {
                FileStream fs = new FileStream(@path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                StreamWriter swFromFile = new StreamWriter(fs);
                fs.Seek(0, SeekOrigin.End);
                //swFromFile.Write(info1);
                swFromFile.WriteLine(info);
                swFromFile.Flush();
                swFromFile.Close();
                fs.Close();
            }
            catch
            {
            }
        }



        public void CsvWriteLine(string path, string[] info)
        {
            //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";
            string info1 = "";
            foreach (string s in info)
            {
                //info1 += ("aa" + ",");
            }
            try
            {
                FileStream fs = new FileStream(@path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                StreamWriter swFromFile = new StreamWriter(fs);
                fs.Seek(0, SeekOrigin.End);
                //swFromFile.Write(info1);
                swFromFile.WriteLine(info);
                swFromFile.Flush();
                swFromFile.Close();
                fs.Close();
            }
            catch
            {
            }
        }
    }
}
