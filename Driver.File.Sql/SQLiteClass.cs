using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;

namespace Driver.File.Sql
{
    public class SQLiteClass
    {

        public void CreateTable(string dbPath,string tableName)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE "+tableName+"(id integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE)";
                    command.ExecuteNonQuery();
                }
            } 
        }


        public void DeleteTable(string dbPath, string tableName)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DROP TABLE "+tableName;
                    command.ExecuteNonQuery();
                }
            }
        }

      public  void InsertVariableData(string dbPath,string tableName,string[]value) 
     { 
          SQLiteParameter[] parameters=null;
          string sql=null;

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {

                        sql = "INSERT INTO "+tableName+"(Name,Show,Min,Max,Value)values(@Name,@Show,@Min,@Max,@Value)"; 

                       parameters = new SQLiteParameter[]{ 
                       new SQLiteParameter("@Name",value[0]), 
                       new SQLiteParameter("@Show",value[1]), 
                       new SQLiteParameter("@Min",value[2]), 
                       new SQLiteParameter("@Max",value[3]), 
                       new SQLiteParameter("@Value",value[4]), 
                      };
                   command.Parameters.AddRange(parameters); 
                   command.CommandText=sql;
                   command.ExecuteNonQuery(); 
                }
            }
     }


      public void SaveOneHistoryData(string dbPath, string tableName, string barcode,string dateTime,string result,string pojectName,string productName,DataTable dt)
      {
          SQLiteParameter[] parameters = null;
          string sql = null;
          string[] value = new string[10];

          int index = 0;
          for (int i = 0; i < dt.Rows.Count; i++)
          {
              index = i / 500;

              value[index] += dt.Rows[i][4].ToString() + "," + dt.Rows[i][5].ToString() + "," + dt.Rows[i][6].ToString() + "," + dt.Rows[i][8].ToString() + "," + dt.Rows[i][9].ToString() + "&";
          }

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {

                    sql = "INSERT INTO " + tableName + "(二维码,日期,结果,测试项目,测试产品,数据1,数据2,数据3,数据4,数据5,数据6,数据7,数据8,数据9,数据10)values(@二维码,@日期,@结果,@测试项目,@测试产品,@数据1,@数据2,@数据3,@数据4,@数据5,@数据6,@数据7,@数据8,@数据9,@数据10)";

                    parameters = new SQLiteParameter[]{ 
                    new SQLiteParameter("@二维码",barcode), 
                    new SQLiteParameter("@日期",dateTime), 
                    new SQLiteParameter("@结果",result), 
                    new SQLiteParameter("@测试项目",pojectName), 
                    new SQLiteParameter("@测试产品",productName), 
                    new SQLiteParameter("@数据1",value[0]), 
                    new SQLiteParameter("@数据2",value[1]), 
                    new SQLiteParameter("@数据3",value[2]), 
                    new SQLiteParameter("@数据4",value[3]), 
                    new SQLiteParameter("@数据5",value[4]), 
                    new SQLiteParameter("@数据6",value[5]), 
                    new SQLiteParameter("@数据7",value[6]), 
                    new SQLiteParameter("@数据8",value[7]), 
                    new SQLiteParameter("@数据9",value[8]), 
                    new SQLiteParameter("@数据10",value[9]), 
                      
                  };
                  command.Parameters.AddRange(parameters);
                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }


      public DataTable SearchHistoryByBarCode(string dbPath, string barCode)
      {
          DataTable dt1 = new DataTable();
          DataTable  dt2 = new DataTable();
          DataTable data = new DataTable(); ;
          SQLiteCommand command;
          SQLiteDataAdapter adapter;

          string sql = "SELECT name FROM sqlite_master WHERE type='table' ";//and name like '%_TestProject' ORDER BY name; ";

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (command = new SQLiteCommand(sql, connection))
              {
                  adapter = new SQLiteDataAdapter(command);
                  adapter.Fill(data);

                  for (int i = 0; i < data.Rows.Count; i++)
                  {
                      sql = "SELECT * FROM " + data.Rows[i][0].ToString() + " where 二维码='" + barCode + "'";
                      using (command = new SQLiteCommand(sql, connection))
                      {
                          adapter = new SQLiteDataAdapter(command);
                          adapter.Fill(dt1);
                          dt2.Merge(dt1);
                      }
                  }
                  return dt2;
              }
          }
      }

      public DataTable SearchHistoryByBarCodeAndTime(string dbPath, string barCode,string time)
      {

          DataTable dt1 = new DataTable();
          DataTable dt2 = new DataTable();
          DataTable data = new DataTable(); ;
          SQLiteCommand command;
          SQLiteDataAdapter adapter;

          string sql = "SELECT name FROM sqlite_master WHERE type='table' ";//and name like '%_TestProject' ORDER BY name; ";

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (command = new SQLiteCommand(sql, connection))
              {
                  adapter = new SQLiteDataAdapter(command);
                  adapter.Fill(data);

                  for (int i = 0; i < data.Rows.Count; i++)
                  {                                                         //r where type='table' and name='"+tableName+"'";
                      sql = "SELECT * FROM " + data.Rows[i][0].ToString() + " where 二维码='" + barCode + "'" +" and 日期='" + time + "'";
                      using (command = new SQLiteCommand(sql, connection))
                      {
                          adapter = new SQLiteDataAdapter(command);
                          adapter.Fill(dt1);

                          //dt2.Merge(dt1);

                          if (dt1.Rows.Count > 0)
                          {

                              return dt1;
                          }
                      }
                  }
                  return dt1;
              }
          }
      }


      public void CreateNewTestItemTable(string dbPath, string newTableName)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = @"CREATE TABLE " + newTableName + " ([Enable] BOOL, [功能] TEXT, [参数] TEXT, [设备调用名] TEXT , [步骤名] TEXT ,[执行后延时] TEXT,[重测次数] TEXT, [标记] TEXT, [备注] TEXT, [路径] TEXT)";

                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public void CreateNewDeviceTable(string dbPath, string newTableName)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = @"CREATE TABLE " + newTableName + " ([Enable] BOOL, [调用名] TEXT, [Name] TEXT, [配置] TEXT , [路径] TEXT)";
                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public void CreateNewHistoryTable(string dbPath, string newTableName)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {

                  sql = @"CREATE TABLE " + newTableName + " ( [二维码] TEXT, [日期] TEXT, [结果] TEXT , [测试项目] TEXT, [测试产品] TEXT  ,[数据1] TEXT, [数据2] TEXT, [数据3] TEXT, [数据4] TEXT, [数据5] TEXT, [数据6] TEXT, [数据7] TEXT, [数据8] TEXT, [数据9] TEXT, [数据10] TEXT)";

                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public void CreateNewVariableTable(string dbPath, string newTableName)
      {//显示,名称,最小值,最大值,测量值,结果
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {

                  sql = @"CREATE TABLE " + newTableName + " ([显示] BOOL,[判定] BOOL,[数值] BOOL,[步骤] TEXT,[名称] TEXT, [最小值] TEXT, [最大值] TEXT,[默认值] TEXT, [测量值] TEXT, [结果] TEXT)";

                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public void CreateNewTestProjectTable(string dbPath, string newTableName)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = @"CREATE TABLE " + newTableName + " ([Name] TEXT,[Enable] BOOL)";

                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public DataTable ReadWholeTable(string dbPath,string tableName)
      {
          string sql = "SELECT * FROM " + tableName;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (SQLiteCommand command = new SQLiteCommand(sql, connection))
              {
                  SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                  DataTable data = new DataTable();
                  adapter.Fill(data);
                  return data;
              }
          }
      }

      public DataTable ReadVariableTable(string dbPath, string tableName)
      {
          string sql = "SELECT * FROM " + tableName+ " where 显示=1";

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (SQLiteCommand command = new SQLiteCommand(sql, connection))
              {
                  SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                  DataTable data = new DataTable();
                  adapter.Fill(data);
                  return data;
              }
          }
      }

      public string  ReadVariableValue(string dbPath, string tableName,string name)
      {
          string sql = "SELECT * FROM " + tableName + " where 名称='" + name+"'";

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (SQLiteCommand command = new SQLiteCommand(sql, connection))
              {
                  SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                  DataTable data = new DataTable();
                  adapter.Fill(data);

                  return data.Rows[0][7].ToString();
              }
          }
      }


      public void ClearVariableTableValueColumn(string dbPath, string tableName)
      {

          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = sql = "update " + tableName + " set 测量值='',结果=''";

                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }


      public void UpdateVariableTableValueColumn(string dbPath, string tableName,string name,string value)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = "update " + tableName + " set 测量值= '"+ value +"' where 名称= '" +name+"'";
                  command.CommandText = sql;
                  command.ExecuteNonQuery();

                  sql = "update " + tableName + " set 默认值= '" + value + "' where 名称= '" + name + "'";
                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }

      public void UpdateVariableTableValueArray(string dbPath, string tableName, string name, string value)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  //for (int i = 0; i < 60; i++)
                  //{
                      sql = "update " + tableName + " set 测量值= '" + value + "' where 名称= '" + name + "'";
                      command.CommandText = sql;
                      command.ExecuteNonQuery();

                      //sql = "update " + tableName + " set 默认值= '" + value + "' where 名称= '" + name + "'";
                      //command.CommandText = sql;
                      //command.ExecuteNonQuery();
                  //}
              }
          }
      }


      ///        CheckVariableValueExist
      public int CheckVariableValueExist(string dbPath, string tableName, string name)
      {
          string sql = "select * from " + tableName + " where 名称= '" + name + "'";

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (SQLiteCommand command = new SQLiteCommand(sql, connection))
              {
                  SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                  DataTable data = new DataTable();
                  adapter.Fill(data);

                  return data.Rows.Count;
              }
          }


      }



      public void UpdateVariableTableResultColumn(string dbPath, string tableName, string name, string value)
      {
          string sql = null;

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              connection.Open();
              using (SQLiteCommand command = new SQLiteCommand(connection))
              {
                  sql = "update " + tableName + " set 结果='" + value +"'"+ " where 名称= " + name;
                  command.CommandText = sql;
                  command.ExecuteNonQuery();
              }
          }
      }


        public DataTable  ReadTestItem(string dbPath, string tableName)
      {

          string sql = "SELECT name FROM sqlite_master WHERE type='table' and name like '%_TestItem' ORDER BY name; ";  //清空数据

          using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
          {
              using (SQLiteCommand command = new SQLiteCommand(sql, connection))
              {
                  SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                  DataTable data = new DataTable();
                  adapter.Fill(data);
                  return data;
              }
          }  
        }

        public DataTable ReadDeviceSetting(string dbPath, string tableName)
        {
            string sql = "SELECT name FROM sqlite_master WHERE type='table' and name like '%_DeviceSetting' ORDER BY name; ";  //清空数据
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }

        public DataTable ReadTestProject(string dbPath, string tableName)
        {
            string sql = "SELECT name FROM sqlite_master WHERE type='table' and name like '%_TestProject' ORDER BY name; ";  //清空数据
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + dbPath))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }


        public int UpdateTestItemTable(string dbPath, string tableName, DataTable dt)
        {

            //Sqlite使用事务批量操作 极大的提高速度  
            //DateTime starttime = DateTime.Now;
            using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
            {
                con.Open();
                DbTransaction trans = con.BeginTransaction();//开始事务       
                SQLiteCommand cmd = new SQLiteCommand(con);

                cmd.CommandText = "delete from " + tableName;  //清空数据
                cmd.ExecuteNonQuery();

                try
                {
                    cmd.CommandText = "INSERT INTO " + tableName + " (Enable,功能,参数,设备调用名,步骤名,执行后延时,重测次数,标记,备注,路径)VALUES(@Enable, @功能,@参数,@设备调用名,@步骤名,@执行后延时,@重测次数,@标记,@备注,@路径)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";
                    for (int n = 0; n < dt.Rows.Count; n++)
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@Enable", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@功能", DbType.String)); //MySql 引用MySql.Data.dll  
                        cmd.Parameters.Add(new SQLiteParameter("@参数", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@设备调用名", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@步骤名", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@执行后延时", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@重测次数", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@标记", DbType.String)); //MySql 引用MySql.Data.dll  
                        cmd.Parameters.Add(new SQLiteParameter("@备注", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@路径", DbType.String)); //MySql 引用MySql.Data.dll  

                        cmd.Parameters["@Enable"].Value = dt.Rows[n].ItemArray[0];
                        cmd.Parameters["@功能"].Value = dt.Rows[n].ItemArray[1];
                        cmd.Parameters["@参数"].Value = dt.Rows[n].ItemArray[2];
                        cmd.Parameters["@设备调用名"].Value = dt.Rows[n].ItemArray[3];
                        cmd.Parameters["@步骤名"].Value = dt.Rows[n].ItemArray[4];
                        cmd.Parameters["@执行后延时"].Value = dt.Rows[n].ItemArray[5];
                        cmd.Parameters["@重测次数"].Value = dt.Rows[n].ItemArray[6];
                        cmd.Parameters["@标记"].Value = dt.Rows[n].ItemArray[7];
                        cmd.Parameters["@备注"].Value = dt.Rows[n].ItemArray[8];
                        cmd.Parameters["@路径"].Value = dt.Rows[n].ItemArray[9];

                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();//提交事务    
                    // DateTime endtime = DateTime.Now;
                    // MessageBox.Show("插入成功，用时" + (endtime - starttime).TotalMilliseconds);

                    return 1;

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);


                }
                return 0;
            }
        }


        //public int UpdateDeviceTable(string dbPath, string tableName, DataTable dt)
        //{

        //    //Sqlite使用事务批量操作 极大的提高速度  
        //    //DateTime starttime = DateTime.Now;
        //    using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
        //    {
        //        con.Open();
        //        DbTransaction trans = con.BeginTransaction();//开始事务       
        //        SQLiteCommand cmd = new SQLiteCommand(con);

        //        cmd.CommandText = "delete from " + tableName;  //清空数据
        //        cmd.ExecuteNonQuery();

        //        try
        //        {
        //            cmd.CommandText = "INSERT INTO " + tableName + " (Enable,调用名,Name,配置,路径)VALUES(@Enable,@调用名,@Name,@配置,@路径)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";
        //            for (int n = 0; n < dt.Rows.Count; n++)
        //            {
        //                cmd.Parameters.Add(new SQLiteParameter("@Enable", DbType.Boolean)); //MySql 使用MySqlDbType.String  
        //                cmd.Parameters.Add(new SQLiteParameter("@调用名", DbType.Boolean)); //MySql 使用MySqlDbType.String  
        //                cmd.Parameters.Add(new SQLiteParameter("@Name", DbType.Boolean)); //MySql 使用MySqlDbType.String  
        //                cmd.Parameters.Add(new SQLiteParameter("@配置", DbType.String)); //MySql 使用MySqlDbType.String  
        //                cmd.Parameters.Add(new SQLiteParameter("@路径", DbType.String)); //MySql 引用MySql.Data.dll  

        //                cmd.Parameters["@Enable"].Value = dt.Rows[n].ItemArray[0];
        //                cmd.Parameters["@调用名"].Value = dt.Rows[n].ItemArray[1];
        //                cmd.Parameters["@Name"].Value = dt.Rows[n].ItemArray[2];
        //                cmd.Parameters["@配置"].Value = dt.Rows[n].ItemArray[3];
        //                cmd.Parameters["@路径"].Value = dt.Rows[n].ItemArray[4];
        //                cmd.ExecuteNonQuery();
        //            }
        //            trans.Commit();//提交事务    
        //            return 1;

        //        }
        //        catch (Exception ex)
        //        {
        //            //MessageBox.Show(ex.Message);


        //        }
        //        return 0;
        //    }
        //}

        public int UpdateVariableTable(string dbPath, string tableName, DataTable dt)
        {

            //Sqlite使用事务批量操作 极大的提高速度  
            //DateTime starttime = DateTime.Now;
            using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
            {
                con.Open();
                DbTransaction trans = con.BeginTransaction();//开始事务       
                SQLiteCommand cmd = new SQLiteCommand(con);

                cmd.CommandText = "delete from " + tableName;  //清空数据
                cmd.ExecuteNonQuery();

                try
                {
                    cmd.CommandText = "INSERT INTO " + tableName + " (显示,判定,数值,步骤,名称,最小值,最大值,默认值,测量值,结果)VALUES(@显示,@判定,@数值,@步骤,@名称,@最小值,@最大值,@默认值,@测量值,@结果)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";
                    for (int n = 0; n < dt.Rows.Count; n++)
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@显示", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@判定", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@数值", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@步骤", DbType.String)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@名称", DbType.String)); //MySql 引用MySql.Data.dll  
                        cmd.Parameters.Add(new SQLiteParameter("@最小值", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@最大值", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@默认值", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@测量值", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@结果", DbType.String)); //MySql 引用MySql.Data.dll  

                        cmd.Parameters["@显示"].Value = dt.Rows[n].ItemArray[0];
                        cmd.Parameters["@判定"].Value = dt.Rows[n].ItemArray[1];
                        cmd.Parameters["@数值"].Value = dt.Rows[n].ItemArray[2];
                        cmd.Parameters["@步骤"].Value = dt.Rows[n].ItemArray[3];
                        cmd.Parameters["@名称"].Value = dt.Rows[n].ItemArray[4];
                        cmd.Parameters["@最小值"].Value = dt.Rows[n].ItemArray[5];
                        cmd.Parameters["@最大值"].Value = dt.Rows[n].ItemArray[6];
                        cmd.Parameters["@默认值"].Value = dt.Rows[n].ItemArray[7];
                        cmd.Parameters["@测量值"].Value = dt.Rows[n].ItemArray[8];
                        cmd.Parameters["@结果"].Value = dt.Rows[n].ItemArray[9];
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();//提交事务    
                    // DateTime endtime = DateTime.Now;
                    // MessageBox.Show("插入成功，用时" + (endtime - starttime).TotalMilliseconds);

                    return 1;

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);


                }
                return 0;
            }
        }


        public int UpdateDeviceSettingTable(string dbPath, string tableName, DataTable dt)
        {

            //Sqlite使用事务批量操作 极大的提高速度  
            //DateTime starttime = DateTime.Now;
            using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
            {
                con.Open();
                DbTransaction trans = con.BeginTransaction();//开始事务       
                SQLiteCommand cmd = new SQLiteCommand(con);

                cmd.CommandText = "delete from " + tableName;  //清空数据
                cmd.ExecuteNonQuery();

                try
                {
                    cmd.CommandText = "INSERT INTO " + tableName + " (Enable,调用名,Name,配置,路径)VALUES(@Enable, @调用名,@Name,@配置,@路径)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";
                    for (int n = 0; n < dt.Rows.Count; n++)
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@Enable", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                        cmd.Parameters.Add(new SQLiteParameter("@调用名", DbType.String)); //MySql 引用MySql.Data.dll  
                        cmd.Parameters.Add(new SQLiteParameter("@Name", DbType.String));
                        cmd.Parameters.Add(new SQLiteParameter("@配置", DbType.String)); //MySql 引用MySql.Data.dll  
                        cmd.Parameters.Add(new SQLiteParameter("@路径", DbType.String)); //MySql 引用MySql.Data.dll  

                        cmd.Parameters["@Enable"].Value = dt.Rows[n].ItemArray[0];
                        cmd.Parameters["@调用名"].Value = dt.Rows[n].ItemArray[1];
                        cmd.Parameters["@Name"].Value = dt.Rows[n].ItemArray[2];
                        cmd.Parameters["@配置"].Value = dt.Rows[n].ItemArray[3];
                        cmd.Parameters["@路径"].Value = dt.Rows[n].ItemArray[4];

                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();//提交事务    
                    return 1;

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);


                }
                return 0;
            }
        }


        public int CheckTableExistStatus(string dbPath, string tableName)
        {
            using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(con);
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master where type='table' and name='"+tableName+"'";
                if (0 == Convert.ToInt32(cmd.ExecuteScalar()))
                {
                    //table - Student does not exist.  
                    return 0;
                }
                else
                {
                    //table - Student does exist.  
                    return 1;
                }
            }
        }


      public int  UpdateTestProjectTable(string dbPath, string tableName,DataTable dt)
      {

          //Sqlite使用事务批量操作 极大的提高速度  
          //DateTime starttime = DateTime.Now;
          using (SQLiteConnection con = new SQLiteConnection("Data Source=" + dbPath))
          {
              con.Open();
              DbTransaction trans = con.BeginTransaction();//开始事务       
              SQLiteCommand cmd = new SQLiteCommand(con);

              cmd.CommandText = "delete from " + tableName;  //清空数据
              cmd.ExecuteNonQuery();

              try
              {
                  cmd.CommandText = "INSERT INTO " + tableName + " (Enable,功能,参数,标记,备注,路径)VALUES(@Enable, @功能,@参数,@标记,@备注,@路径)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";

                  cmd.CommandText = "INSERT INTO " + tableName + " (Name,Enable)VALUES(@Name, @Enable)";// "INSERT INTO MyTable(username,useraddr,userage) VALUES(@a,@b,@c)";
                  for (int n = 0; n < dt.Rows.Count; n++)
                  {
                      cmd.Parameters.Add(new SQLiteParameter("@Name", DbType.String)); //MySql 引用MySql.Data.dll  
                      cmd.Parameters.Add(new SQLiteParameter("@Enable", DbType.Boolean)); //MySql 使用MySqlDbType.String  
                      cmd.Parameters["@Name"].Value = dt.Rows[n].ItemArray[0];
                      cmd.Parameters["@Enable"].Value = dt.Rows[n].ItemArray[1];
                      cmd.ExecuteNonQuery();
                  }
                  trans.Commit();//提交事务    
                 // DateTime endtime = DateTime.Now;
                 // MessageBox.Show("插入成功，用时" + (endtime - starttime).TotalMilliseconds);

                  return 1;

              }
              catch (Exception ex)
              {
                  //MessageBox.Show(ex.Message);

                  
              }
              return 0;
          }  

      } 


    }
}
