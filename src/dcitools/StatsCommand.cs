using System;
using System.Collections.Generic;
using System.IO;
using DCI.Data;
using MathNet.Numerics.Statistics;
using McMaster.Extensions.CommandLineUtils;

namespace DCI.Tools
{
    [Command(Description = "打印CSV文件统计信息")]
    public class StatsCommand: DataSourceOptions
    {
       

        //[Option("-d|--connect", "{db}://{username}:{password}@server:port/database", CommandOptionType.SingleOrNoValue)] 
        //public string Connect {get; set;}

        //[Option("-f|--file", "file path (shp file Or csv file)", CommandOptionType.SingleValue)]
        //[FileExists()]
        public string FilePath {get; set;}

        [Option("-i|--index", "file field index", CommandOptionType.SingleValue)]

        public int Index {get; set;}

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
                        
                        var port = uri.Port >0 ? uri.Port : 5432;
                        var database = uri.AbsolutePath.Trim('/');
                        var connectString = $"Host={uri.Host}; Port={port}; User Id={userName}; Password={password}; Database={database}";
                        return 1;
                    case "file":
                        this.FilePath = uri.LocalPath;
                        break;
                    default:
                        return 0;
                }
            }
            
            if (!string.IsNullOrEmpty(this.FilePath))
            {
                IEnumerable<double> data;
                var ext = Path.GetExtension(this.FilePath);

                if (ext.Equals(".shp",StringComparison.InvariantCultureIgnoreCase))
                {
                    var shpDataSource = new ShapeDataSource(this.FilePath, this.Index);
                    data = shpDataSource.GetData();
                }
                else
                {
                    var dataSource = new CsvDataSource(this.FilePath, this.Index);
                    data = dataSource.GetData();
                }
                var stats = new  RunningStatistics(data);
                Console.WriteLine($"总数：{stats.Count}");
                Console.WriteLine($"最大值：{stats.Maximum}");
                Console.WriteLine($"最小值：{stats.Minimum}");
                Console.WriteLine($"平均值Mean：：{stats.Mean}");
                Console.WriteLine($"均方差StandardDeviation： {stats.StandardDeviation}");
                Console.WriteLine($"{stats.ToString()}");
                Console.WriteLine($"中位数： {data.Median()}");
                Console.WriteLine(@"{0} - 有偏方差", data.PopulationVariance().ToString(" #0.00000;-#0.00000"));
                Console.WriteLine(@"{0} - 无偏方差", data.Variance().ToString(" #0.00000;-#0.00000"));
                Console.WriteLine(@"{0} - 标准偏差", data.StandardDeviation().ToString(" #0.00000;-#0.00000"));
                Console.WriteLine(@"{0} - 标准有偏偏差", data.PopulationStandardDeviation().ToString(" #0.00000;-#0.00000"));
                Console.WriteLine($"25% 中位数：{data.LowerQuartile()}");
                Console.WriteLine($"75% 中位数：{data.UpperQuartile()}");
                //new DescriptiveStatistics()

                var histogram = new Histogram(data, 100, stats.Minimum, stats.Maximum);
                //Console.WriteLine($"{histogram.ToString()}");
                for(var i =0; i< 100; i++)
                {
                    var bucket = histogram[i];
                    Console.WriteLine($"({bucket.LowerBound}, {bucket.UpperBound}] {bucket.Count}");
                }
                
                return 1;

            }
            app.ShowHelp();
            return 1;
        }
    }
}