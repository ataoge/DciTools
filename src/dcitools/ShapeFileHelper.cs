using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI;
using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace DCI.Tools
{
    public static class ShapeFileHelper
    {
        static ShapeFileHelper()
        {
            GeometryServiceProvider.Instance = NtsGeometryServices.Instance;
        }
        public static void Describe(string shapeFilePath, string encoding = null)
        {
            var factory = new GeometryFactory();

            ShapefileDataReader reader;
            if (string.IsNullOrEmpty(encoding))
                reader = new ShapefileDataReader(shapeFilePath, factory);
            else 
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                reader = new ShapefileDataReader(shapeFilePath, factory, Encoding.GetEncoding(encoding));
            }
            
            reader.Read();
            int length = reader.DbaseHeader.NumFields;
            for(var i = 0; i< length; i++)
            {
                Console.WriteLine($"{i+1} {reader.DbaseHeader.Fields[i].Name} {reader.DbaseHeader.Fields[i].DbaseType} {reader.DbaseHeader.Fields[i].Length},  {reader.GetString(i)}");
                if (i>1)
                {
                    //Encoding ee = Encoding.GetEncoding("ISO-8859-1");
                    
                    //var ss = Encoding.GetEncoding("GB2312").GetString(ee.GetBytes(reader.GetString(i)));
                    //Console.WriteLine($"{ss}");
                }
            }
        }
        public static void Compress(string shapeFilePath, int gridFieldIndex,  string encoding = null, string outputShapeFileName = "")
        {
            
            var factory = new GeometryFactory();

            if (!File.Exists(shapeFilePath))
            {
                Console.WriteLine("文件不存在。");
                return;
            }
            string filePath = Path.GetDirectoryName(shapeFilePath);
            string outputShapeFilePath = "";
            var outputFilePath = Path.ChangeExtension(shapeFilePath, ".text");
            if (!string.IsNullOrEmpty(outputShapeFileName))
            {
                outputShapeFilePath = Path.Combine(filePath, outputShapeFileName);
                outputFilePath = Path.ChangeExtension(outputShapeFileName, ".txt");
            }
              
            var nFile = new FileStream(outputFilePath, FileMode.Create);
            Encoding fileEncoding = Encoding.UTF8;
            nFile.Position = nFile.Length;
            
            var fts = new List<IFeature>();
            ShapefileDataReader reader;
            if (string.IsNullOrEmpty(encoding))
                reader = new ShapefileDataReader(shapeFilePath, factory);
            else 
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                reader = new ShapefileDataReader(shapeFilePath, factory, Encoding.GetEncoding(encoding));
            }

            using(reader)
            {
                var dict = new Dictionary<string, IGeometry>();
                int count = 0;
                long recordCount = 0;
                try {
                    while( reader.Read())
                    {
                        recordCount++;
                        var gridId = reader.GetString(gridFieldIndex);
                        if (dict.ContainsKey(gridId))
                            continue;

                        count++;
                        if (count % 1000 == 0)
                        {
                            Console.WriteLine($"{count}");
                            nFile.Flush();
                        }
                    

                        dict.Add(gridId, reader.Geometry); 
                        string text = $"{gridId},{reader.Geometry.ToString()}\n";
                        var bytes = fileEncoding.GetBytes(text);
                        
                        nFile.Write(bytes, 0, bytes.Length); 

                        var attrs = new AttributesTable();
                        attrs.Add("GRIDID", gridId); 
                        var feature = new Feature(reader.Geometry, attrs);
                        fts.Add(feature);
                        
                    }
                }
                catch(Exception)
                {
                    Console.WriteLine($"第{recordCount}条记录读取错误！");
                    throw;
                }
                Console.WriteLine($"共处理{reader.RecordCount}条数据，压缩生成{count}条数据");

                    
                nFile.Close();
            };
                

            if (!string.IsNullOrEmpty(outputShapeFilePath))
            {
                var writer = new ShapefileDataWriter(outputShapeFilePath, factory);
                writer.Header = ShapefileDataWriter.GetHeader(fts[0], fts.Count);
                writer.Write(fts);
            }

            Console.WriteLine("成功结束！");
            
        }
    }
}