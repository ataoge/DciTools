using System;
using System.Collections.Generic;
using System.IO;
using DCI.Data;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Data.Sqlite;

namespace DCI.Tools
{
    public class DescribeCommand : DataSourceOptions //: CommandBase
    {
        

        //[Option("-s|--source", "{file|sqlite|postgresql}://{username}:{password}@{server}:{port}/{database}", CommandOptionType.SingleValue)] 
        //public string Connect {get; set;}

        //[Option("-u|--username", "-u=用户名", CommandOptionType.SingleOrNoValue)] 
        //public string UserName {get; set;}

        //[Option("-p|--password", "-p密码", CommandOptionType.SingleOrNoValue)] 
        //public string Password {get; set;}

        [Option("-t|--table", "-t=表名|文件名", CommandOptionType.SingleOrNoValue)] 
        public string TableName {get; set;}

        public int OnExecute(CommandLineApplication app)
        {
            if (!string.IsNullOrEmpty(this.Connect))
            {
                var uri = new Uri(this.Connect);
                switch(uri.Scheme)
                {
                    case "postgresql":
                        string userName = "";
                        string password = "";
                        if (!string.IsNullOrEmpty(uri.UserInfo))
                        {
                            var ss = uri.UserInfo.Split(":");
                            userName = ss[0];
                            password = ss.Length > 1 ? ss[1] : "";
                        }

                        if (!string.IsNullOrEmpty(this.UserName))
                            userName = this.UserName;
                        if (!string.IsNullOrEmpty(this.Password))
                            password = this.Password;
                        
                        var port = uri.Port >0 ? uri.Port : 5432;
                        var database = uri.AbsolutePath.Trim('/');
                        var connectString = $"Host={uri.Host};Port={port};Username={userName};Password={password};Database={database}";
                         
                        var npgConn = DbHelper.OpenConnection(Npgsql.NpgsqlFactory.Instance, connectString);
                       
                        
                        if (string.IsNullOrEmpty(this.TableName))
                        {
                            var sql = @"SELECT  tablename  FROM  pg_tables WHERE  tablename   NOT   LIKE   'pg%'  AND tablename NOT LIKE 'sql_%' ORDER   BY   tablename";
                            using (var read = DbHelper.ExecuteCommandText(npgConn, sql))
                            {
                                for (var i = 0; i < read.FieldCount; i++)
                                {
                                    if (i == 0)
                                        Console.Write("编号");
                                    Console.Write("  ");
                                    Console.Write($"{read.GetName(i)}");
                                }
                                Console.WriteLine();
                                var count = 0;
                                while (read.Read())
                                {
                                    count++;
                                    for (var i = 0; i < read.FieldCount; i++)
                                    {
                                        if (i == 0)
                                            Console.Write($"{count}");
                                        Console.Write("  ");
                                        if (read.IsDBNull(i))
                                            Console.Write("<NULL>");
                                        else
                                            Console.Write($"{read.GetString(i)}");
                                    }
                                    Console.WriteLine();
                                }
                                Console.WriteLine(string.Format("总共{0}个表！", count));
                            }
                        }
                        else
                        {
                            var sql = @"SELECT a.attname as name, col_description(a.attrelid,a.attnum) as comment,pg_type.typname as typename,a.attnotnull as notnull
                                        FROM pg_class as c,pg_attribute as a inner join pg_type on pg_type.oid = a.atttypid
                                        where c.relname = @P1 and a.attrelid = c.oid and a.attnum>0";
                            using (var read = DbHelper.ExecuteCommandText(npgConn, sql, this.TableName))
                            {
                                for (var i = 0; i < read.FieldCount; i++)
                                {
                                    if (i == 0)
                                        Console.Write("编号");
                                    Console.Write("  ");
                                    Console.Write($"{read.GetName(i)}");
                                }
                                Console.WriteLine();
                                var count = 0;
                                while (read.Read())
                                {
                                    count++;
                                    for (var i = 0; i < read.FieldCount; i++)
                                    {
                                        if (i == 0)
                                            Console.Write($"{count}");
                                        Console.Write("  ");
                                        if (read.IsDBNull(i))
                                            Console.Write("<NULL>");
                                        else
                                            Console.Write($"{read.GetValue(i)}");
                                    }
                                    Console.WriteLine();
                                }
                                Console.WriteLine(string.Format("总共{0}个字段！", count));
                            }
                        }
                        
                        
                        return 1;
                    case "sqlite":
                        var connString = $"Data Source={uri.LocalPath}";
                        var dbFactory = SqliteFactory.Instance;
                        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                        var conn = dbFactory.CreateConnection();
                        conn.ConnectionString = connString;
                        //var sqlConn = new SqliteConnection(connString);

                        conn.Open();
                        var command = dbFactory.CreateCommand();
                        command.Connection = conn;
                        var template = "{0}";
                        if (string.IsNullOrEmpty(this.TableName))
                        {
                            command.CommandText = @"SELECT name FROM sqlite_master WHERE type=@type ORDER BY name;";
                            var parameter = command.CreateParameter();
                            parameter.ParameterName ="type";
                            parameter.Value = "table";
                            command.Parameters.Add(parameter);
                            template = "总共{0}个表！";
                        }
                        else
                        {
                            command.CommandText = $"PRAGMA table_info([{this.TableName}])";
                            template = "总共{0}个字段！";
                        }

                        using (var read = command.ExecuteReader())
                        {
                            
                            for(var i=0; i < read.FieldCount; i++)
                            {
                                if (i==0)
                                    Console.Write("编号");
                                Console.Write("  ");
                                Console.Write($"{read.GetName(i)}");
                            }
                            Console.WriteLine();
                            var count = 0;
                            while (read.Read())
                            {
                                count++;
                                for(var i = 0; i < read.FieldCount; i++)
                                {
                                    if (i == 0)
                                        Console.Write($"{count}");
                                    Console.Write("  ");
                                    if (read.IsDBNull(i))
                                        Console.Write("<NULL>");
                                    else
                                        Console.Write($"{read.GetString(i)}");
                                }
                                Console.WriteLine();
                            }
                            Console.WriteLine(string.Format(template, count));
                        }
                        conn.Close();
                        return 1;
                    case "file":
                        if (HandleFile(uri.LocalPath, this.TableName))
                            return 1;
                        break;
                    default:
                        return 0;
                }
            }

            app.ShowHelp();
            return 1;
        }

        public bool HandleFile(string filePath, string fileName = null)
        {
            if (Directory.Exists(filePath))
            {
                if (string.IsNullOrEmpty(fileName) || fileName.StartsWith("."))
                {
                    var count=0;
                    foreach(var theFileName in Directory.GetFiles(filePath))
                    {
                        
                        if (string.IsNullOrEmpty(fileName))
                        {
                            count++;
                            Console.WriteLine($"{count} {Path.GetFileName(theFileName)}");
                        }
                        else
                        {
                            if (Path.GetExtension(theFileName).Equals(fileName,StringComparison.InvariantCultureIgnoreCase))
                            {
                                count++;
                                Console.WriteLine($"{count} {Path.GetFileName(theFileName)}");
                            }
                            else
                                continue;
                        }
                    }
                    return true;
                }
            }
            
            var fileFullPath = filePath;
            if (!string.IsNullOrEmpty(fileName))
                fileFullPath = Path.Combine(filePath, fileName);
            if (File.Exists(fileFullPath))
            {
                var ext = Path.GetExtension(fileFullPath);
                if (ext.Equals(".shp", StringComparison.InvariantCultureIgnoreCase))
                {
                    ShapeFileHelper.Describe(fileFullPath);
                }
            }
            return true;
        }

    }
}