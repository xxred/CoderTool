using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NewLife;

namespace Web码神工具
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = BuildWebHost(args);

            //var serverAddresses = webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
            //if (serverAddresses != null)
            //{
            //    var url = serverAddresses.FirstOrDefault() ?? "";
            //    if (Runtime.OSX)
            //    {
            //        Process.Start("open", url);
            //    }
            //    else if(Runtime.Linux)
            //    {
            //        Process.Start("xdg-open", url);
            //    }
            //    else
            //    {
            //        //Process.Start($"cmd /c start {url}");

            //        Process.Start(new ProcessStartInfo("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", $"{url}"));
            //    }
            //}

            webHost.Run();

        }

        public static IWebHost BuildWebHost(string[] args) =>
           WebHost.CreateDefaultBuilder(args)
               .UseKestrel()
               .UseStartup<Startup>()
               .Build();
    }
}
