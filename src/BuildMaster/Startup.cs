using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BuildMaster.Infrastructure;
using BuildMaster;

namespace LibCloud.Core
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            //Use a MS SQL Server database
            var sqlConnectionString = AppConfigProvider.Instance.GetConnectionString();

            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            Console.WriteLine($"Connection string: {sqlConnectionString}");

            services.AddEntityFramework().AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    sqlConnectionString,
                    b => b.MigrationsAssembly("BuildMaster")
                ), ServiceLifetime.Transient
            );

            services.AddTransient<IRepository, Repository>();
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting up application");

            AppConfigProvider.Instance.Configure(args);

            ConfigureServices(ServiceCollectionProvider.Instance.Collections);

            var manager = new TaskManager();

            manager.Start();

            Console.WriteLine("Press ESC to stop");

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Stopping Manager");
                    manager.Stop();
                    break;
                }

                System.Threading.Thread.Sleep(1000);
                continue;
                
            }

            Console.WriteLine("Waiting for all running tasks to finish");

            manager.WaitForAll();

            Console.WriteLine("All tasks completed");
        }
    }
}