using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace DCI.Tools
{
    class Program
    {
        static int Main(string[] args)
        {
                   
            var services = new ServiceCollection();
            ConfigureServices(services);
            var sp = services.BuildServiceProvider();
            
            var app = new CommandLineApplication<AtaogeCommand>();
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(sp);
            //app.HelpOption(inherited: true);
          
            app.FullName = $"城信所数据处理工具集";
            
            app.Command("shape", shpCmd => {
                shpCmd.Description = "Shape文件处理";
                var operate = shpCmd.Option("-o|--operate","操作:describe或compress",CommandOptionType.SingleValue)
                        .IsRequired().Accepts(v => v.Values("describe", "compress"));
                var filePath = shpCmd.Option("-f|--file", "Input Shapefile", CommandOptionType.SingleValue)
                        .IsRequired().Accepts(v => v.ExistingFile());
                var index = shpCmd.Option<int>("-i|--index", "Grid Code Index", CommandOptionType.SingleValue)
                        .Accepts(o => o.Range(0,30));
                var output = shpCmd.Option("-t|--output","output shapefile name", CommandOptionType.SingleValue);
                var encoding = shpCmd.Option("-e|--encoding","dbf file encoding", CommandOptionType.SingleValue);


                shpCmd.OnExecute(() => {
                    if (operate.Value() == "describe")
                    {
                        ShapeFileHelper.Describe(filePath.Value(), encoding.Value());
                        return 1;
                    }
                    else if (operate.Value() == "compress")
                    {
                        ShapeFileHelper.Compress(filePath.Value(), index.ParsedValue, encoding.Value(), output.Value());
                        return 1;
                    }
                    shpCmd.ShowHelp();
                    return 1;
                });
            });

            var transfomrcmd = app.Commands.Find(c => c.Name == "transform");
            transfomrcmd?.Option<int>("--outSrid", "要转换成的srid", CommandOptionType.SingleValue);
           
        
       
            app.Command<StatsCommand>("stats", cmd => {
                //cmd.Options.Find(opt => opt.LongName == "xy").IsRequired();
            });

            app.Command<DescribeCommand>("describe", cmd => {
                cmd.Description = "Describe shape file or database";
            });
            
            return app.Execute(args);
        }

        public static void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
