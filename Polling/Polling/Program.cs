using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Polling
{
    public class Program
    {
        static IConfigurationRoot config;
        public static void Main(string[] args)
        {
            config = new ConfigurationBuilder()
           .AddCommandLine(args)
           .AddJsonFile("hosting.json", optional: true)
           .Build();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    //所有controller都不限制post的body大小
                    options.Limits.MaxRequestBodySize = null;
                })
                .UseConfiguration(config)
                .UseStartup<Startup>();
    }
}
