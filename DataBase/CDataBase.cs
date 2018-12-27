using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Driver.DataBase.Sql
{
    public class CDataBase
    {
        SqlConnection conn = null;
        SqlCommand cmd = null;
        string sql = null;

        public CDataBase(string sqlConnect)
        {
            conn = new SqlConnection(sqlConnect);
            cmd = conn.CreateCommand();
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        public void conncet()
        {
           // string SQLCONNECT = @"Data Source=T450-PC\SQL;Initial Catalog=NBET;Integrated Security=True";

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }
        /// <summary>
        /// 在数据库中增加一行数据
        /// </summary>
        /// <param name="sqlcmd">命令如：@" INSERT INTO Result (Time,Life) VALUES ('2015', '1') ";</param>
        public void Add(string sqlcmd)
        {
            if (conn.State == ConnectionState.Open)
            {
                cmd.CommandText = @sqlcmd;
                //cmd.CommandText = @" INSERT INTO Result (Time,Life,SOC,VOLT,CURR) VALUES ('Gates', 'Bill', 'Xuanwumen 10', 'Beijing','1') ";
                cmd.ExecuteNonQuery();
            }
        }

        public void disConnect()
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
        
        }

        public DataTable readAll()
        {
            if (conn.State == ConnectionState.Open)
            {
                DataSet ds = new DataSet();
                cmd.CommandText = "select * from Result";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    da.Fill(ds);
                }
                catch
                { }

                DataTable dt = new DataTable();
                dt = ds.Tables[0].Copy();


                return dt;
            }
            else 
            {
                return null;
            }
        }

    }
}


//1、SQL Server的连接方式

//以本地服务器(LocalHost)，数据库(Northwind)为例，可以有以下一些连接方式
//SqlConnection conn=new SqlConnection( "Server=LocalHost;Integrated Security=SSPI;Database=Northwind");
//SqlConnection conn = new SqlConnection("Data Source=LocalHost;Integrated Security=SSPI;Initial Catalog=Northwind;");
//SqlConnection conn = new SqlConnection(" Data Source=LocalHost;Initial Catalog=Northwind;Integrated Security=SSPI;Persist Security Info=False;Workstation Id=XURUI;Packet Size=4096; ");
//SqlConnection myConn = new SqlConnection("Persist Security Info=False;Integrated Security=SSPI;Database=northwind;Server=LocalHost");
//SqlConnection conn = new SqlConnection(" Uid=sa;Pwd=***;Initial Catalog=Northwind;Data Source=LocalHost;Connect Timeout=900");
//心得：

//a.Server和Database，Data Source和Initial Catalog配对使用的，可以互相替换（见笑）
//b.Integrated Security默认值是False，此时需要提供Uid和Pwd，即将以Sql Server 用户身份登陆数据库；如果设置为True，Yes 或 SSPI，这不能出现Uid和Pwd，将以Windows用户省份登陆数据库。强烈推荐用后一种形式，安全性更高。
//c.Integrated Security和Persist Security Info同时出现，后者设置为False，可保证信息安全。