//************************************************************************************************
//新增日期:     2015-10-16
//作者    :     卢远山
//内容说明：                         用于TCPIP简易传输
//              
//              
//
//*************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Driver.Port.TCPIP
{
    public class TcpIpClass
    {
        TcpClient client = null;
        NetworkStream stream = null;
        /// <summary>
        /// 连接TCPIP
        /// </summary>
        /// <param name="ipaddress">IP地址</param>
        /// <param name="port">端口号</param>
        public void Connect(string ipaddress,int port)
        {
             try
             {
                    client = new TcpClient(ipaddress, port);
                    stream = client.GetStream();
             }
            catch
             {

             }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="msg">需要发送的字符串</param>
        public void Send(string msg)
        {
            Byte[] sendBytes = Encoding.Default.GetBytes(msg);
            if (client.Connected == true)
            {
                stream.Write(sendBytes, 0, sendBytes.Length);
            }
            else 
            { 

            }
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns>返回读取的字符串</returns>
        public string Read()
        {
            byte[] read =new byte[100];
            stream.Read(read,0,100);
            string readmsg = Encoding.Default.GetString(read);
            return readmsg;
        }

        /// <summary>
        /// 断开TCPIP连接
        /// </summary>
        public void DisConnect()
        {
            stream.Close();
            client.Close();
        }
    }
}
