
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Topshelf;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4Net.config", Watch = true)]
namespace WS_scrapeHAP_NLTT
{
    public class Program 
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<getData>(s =>
                {
                    s.ConstructUsing(run => new getData());
                    s.WhenStarted(run => run.Start());
                    s.WhenStopped(run => run.Stop());
                });
                x.RunAsLocalSystem();
                x.SetServiceName("ExecuteService");
                x.SetDisplayName("Execute Service");
                x.SetDescription("Processing crawl from EVNNLDC!!!");
            });
            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetType());
            Environment.ExitCode = exitCodeValue;           
        }
        
    }

}