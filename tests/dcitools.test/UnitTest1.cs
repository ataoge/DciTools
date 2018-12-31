using System;
using DCI.Tools;
using McMaster.Extensions.CommandLineUtils;
using Xunit;

namespace dcitools.test
{
    public class UnitTest1
    {
        [Fact]
        public void TestDescribe()
        {
            var app =  CommandLineApplication.Execute<DescribeCommand>("-s","file://D:/BaiduYunDownload/aa/grid.shp");
        }
    }
}
