using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BuildMaster.Infrastructure;
using BuildMaster.Extensions;
using System.Threading.Tasks;
using System.Linq;
using BuildMaster.Model;

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
                ), ServiceLifetime.Scoped
            );
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting up application");

            AppConfigProvider.Instance.Configure(args);

            ConfigureServices(ServiceCollectionProvider.Instance.Collections);

            ServiceCollectionProvider.Instance.Provider.GetService<ApplicationDbContext>().Database.Migrate();

            var taskArray = new Task[100];
            100.Times(i =>
            {
                taskArray[i - 1] = Task.Factory.StartNew(() =>
                  {
                      Console.WriteLine(i);
                      var sqlConnectionString = AppConfigProvider.Instance.GetConnectionString();

                      var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                      optionsBuilder.UseSqlServer(
                            sqlConnectionString,
                            b => b.MigrationsAssembly("BuildMaster")
                        );

                      var context = new ApplicationDbContext(optionsBuilder.Options);
                      context.Database.BeginTransaction();
                      context.Add(new Configuration
                      {
                          Key = i.ToString(),
                          Value = $"Value{i}"
                      });
                      context.SaveChanges();
                      context.Database.CommitTransaction();
                  });
            });

            Task.WaitAll(taskArray);

            //  var context = ServiceCollectionProvider.Instance.Provider.GetService<DbContext>(); 
            //         context.Database.BeginTransaction();
            //          context.Add(new Configuration{
            //             Key = 1.ToString(),
            //             Value = $"Value{1}"
            //         });
            //         context.SaveChanges();
            //         context.Database.CommitTransaction();

            Console.WriteLine("Done");
        }
    }
}