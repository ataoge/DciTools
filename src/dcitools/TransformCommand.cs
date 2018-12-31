using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace DCI.Tools
{
    [Command(Description = "坐标转换")]
    public class TransfromCommand : CommandBase
    {
        public override List<string> CreateArgs()
        {
            var args = new List<string>();
            return args;
        }

        private AtaogeCommand Parent { get; set; }

        [Option("-o", "转换操作（WGS84ToWeb3857 | Web3857ToWGS84 | WGS84ToCJ02 | CJ02ToWGS84） 或指定 srid", CommandOptionType.SingleValue)]
        public TransformOperator Operator {get; set;}

        [Option("--xy", "要转换的坐标(x,y)", CommandOptionType.SingleValue)]
        [Required]
        public string InputXY {get; set;}

        [Option("--inSrid", "input srid", CommandOptionType.SingleValue)]
        public int InSrid {get; set;}
        protected override int OnExecute(CommandLineApplication app)
        {
            var xy = InputXY.Split(',').Select(s =>  double.Parse(s)).ToArray();
            double ox;
            double oy;
            switch (Operator)
            {
                case TransformOperator.WGS84ToWeb3857:
                    Console.WriteLine($"纬度：{xy[1]}, 经度: {xy[0]}, 转换为:");
                    Ataoge.Utilities.CoordinateTransform.WGS84ToWebMercator(xy[1], xy[0], out ox, out oy);
                    Console.WriteLine($"x:{ox}, y:{oy}");
                    break;
                case TransformOperator.Web3857ToWGS84:
                    Console.WriteLine($"x：{xy[0]}, y: {xy[1]}, 转换为:");
                    Ataoge.Utilities.CoordinateTransform.WebMercatorToWGS84(xy[1], xy[0], out oy, out ox);
                    Console.WriteLine($"lon:{ox}, lat:{oy}");
                    break;

                case TransformOperator.WGS84ToCJ02:
                    Console.WriteLine($"纬度：{xy[1]}, 经度: {xy[0]}, 转换为:");
                    Ataoge.Utilities.CoordinateTransform.WGS84ToGCJ02(xy[1], xy[0], out oy, out ox);
                    Console.WriteLine($"lon:{ox}, lat:{oy}");
                    break;
                 case TransformOperator.CJ02ToWGS84:
                    Console.WriteLine($"lon：{xy[0]}, lat: {xy[1]}, 转换为:");
                    Ataoge.Utilities.CoordinateTransform.GCJ022ToWGS84(xy[1], xy[0], out oy, out ox);
                    Console.WriteLine($"lon:{ox}, lat:{oy}");
                    break;
                default:
                    //var args = CreateArgs();
                    int outSrid = ((CommandOption<int>)app.Options.Find(opt => opt.LongName == "outSrid")).ParsedValue;
                    
                    Console.WriteLine($"{InSrid} {xy[0]}, {xy[1]} {outSrid} ");
                    break;

            }
            
            //app.ShowHelp();
            return 0;
        }



        public ValidationResult OnValidate(ValidationContext cc)
        {
          
            return ValidationResult.Success;
        }

        
    }

    public enum TransformOperator
    {
        Custom,
        WGS84ToWeb3857,
        Web3857ToWGS84,

        WGS84ToCJ02,
        CJ02ToWGS84

        
    }
}