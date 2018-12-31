using System;
using System.Collections.Generic;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace DCI.Tools
{
    [HelpOption("-?|-h|--help")]
    
    public abstract class CommandBase
    {
        public abstract List<string> CreateArgs();

        protected virtual int OnExecute(CommandLineApplication app)
        {
            var args = CreateArgs();

            Console.WriteLine("Result = git " + ArgumentEscaper.EscapeAndConcatenate(args));
            return 0;
        }
       
    }

    //[Command("a")]
    [VersionOptionFromMember("-v|--version", MemberName = nameof(GetVersion))]
    //[Subcommand("test",typeof(TestCommand))]
    [Subcommand("transform", typeof(TransfromCommand))]
    //[Subcommand("describe",typeof(DescribeCommand))]
    public class AtaogeCommand : CommandBase
    {
        public override List<string> CreateArgs()
        {
            var args = new List<string>();
            return args;
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(AtaogeCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }

    [Command(Description = "For Test Only")]
    public class TestCommand : CommandBase
    {

        public override List<string> CreateArgs()
        {
            var args = new List<string>();
            return args;
        }

        private AtaogeCommand Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        
            return base.OnExecute(app);
        }
    }

    public class DataSourceOptions
    {
        [Option("-s|--source", "{file|sqlite|postgresql}://{username}:{password}@{server}:{port}/{database}", CommandOptionType.SingleValue)] 
        public string Connect {get; set;}

         [Option("-u|--username", "-u=用户名", CommandOptionType.SingleOrNoValue)] 
        public string UserName {get; set;}

        [Option("-p|--password", "-p密码", CommandOptionType.SingleOrNoValue)] 
        public string Password {get; set;}
    }
}