//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于.txt文件的简易读写功能
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Driver.File.Txt
{
    public class TxtFileClass
    {

        //public Txt(string path)//构造函数
        //{
    
        //}

         public string TxtReadAll(string path)
        {
            FileStream filest = new FileStream(@path, FileMode.Open, FileAccess.ReadWrite); 
            StreamReader sr = new StreamReader(filest,Encoding.Default);
            string str = sr.ReadToEnd();
            sr.Close();
            filest.Close();
            return str;
        }

         public void TxtWriteAll(string path,string info)
         {
             //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";

             FileStream fs = new FileStream(@path,FileMode.Open, FileAccess.Write, FileShare.None);
             StreamWriter swFromFile = new StreamWriter(fs);
             swFromFile.Write(info);
             swFromFile.Flush();
             swFromFile.Close();
             fs.Close();
         }

         public void TxtWriteLine(string path, string info)
         {
             //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";
            
             FileStream fs = new FileStream(@path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
             StreamWriter swFromFile = new StreamWriter(fs);
             if (fs.CanWrite == true)
             {
                 fs.Seek(0, SeekOrigin.End);
                 //swFromFile.Write(info);
                 swFromFile.WriteLine(info);
                 swFromFile.Flush();
             }
             swFromFile.Close();
             fs.Close();
         }

         public void TxtWriteLine1(string path, string info,double d)
         {
             //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";

             FileStream fs = new FileStream(@path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
             StreamWriter swFromFile = new StreamWriter(fs);
             if (fs.CanWrite == true)
             {
                 fs.Seek(0, SeekOrigin.End);
                 //swFromFile.Write(info);
                 swFromFile.WriteLine(info+d.ToString());
                 swFromFile.Flush();
             }
             swFromFile.Close();
             fs.Close();
         }

         public void TxtWriteLine2(string path, string info, ref double d)
         {
             //string logFile = DateTime.Now.ToShortDateString().Replace(@"/", @"-").Replace(@"\", @"-") + ".log";

             FileStream fs = new FileStream(@path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
             StreamWriter swFromFile = new StreamWriter(fs);
             if (fs.CanWrite == true)
             {
                 fs.Seek(0, SeekOrigin.End);
                 //swFromFile.Write(info);
                 swFromFile.WriteLine(info + ToString());
                 swFromFile.Flush();
             }
             swFromFile.Close();
             fs.Close();

             d = 333.11111;
         }
    }


}
