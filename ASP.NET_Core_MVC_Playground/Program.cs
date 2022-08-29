using ASP.NET_Core_MVC_Playground.Areas.Identity.Data;
using ASP.NET_Core_MVC_Playground.Controllers;
using ASP.NET_Core_MVC_Playground.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_Playground
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // Serilog Stuff - Required
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();


            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Attempting to seed DB");

                    SeedData seedData = new(services.GetRequiredService<DataDbContext>(),
                                            services.GetRequiredService<ILogger<SeedData>>(),
                                            new Helpers(services.GetRequiredService<DataDbContext>(),
                                                        services.GetRequiredService<ILogger<Helpers>>(),
                                                        services.GetRequiredService<IOptions<StripeOptions>>(),
                                                        services.GetRequiredService<IOptions<AppOptions>>(),
                                                        services.GetRequiredService<UserManager<ApplicationUser>>()));
                    seedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error has occured seeding the DB");
                }
                
                try
                {
                    logger.LogInformation("Attempting to start application!");
                    host.Run();
                    
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Application could not start!");
                    throw;
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                //.ConfigureLogging((context, logging) =>
                //{
                //    logging.ClearProviders();
                //    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                //    logging.AddDebug();
                //    logging.AddConsole();
                //})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
