using DuplicateCheckerService.Configuration;
using DuplicateCheckerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DuplicateCheckerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Ensure log directory exists
            var logDirectory = "C:\\ServiceLogs";
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                    .Build())
                .CreateLogger();

            try
            {
                Log.Information("Starting up the service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServiceSettings>(
                        hostContext.Configuration.GetSection("ServiceSettings"));
                    
                    services.AddSingleton<ServiceSettings>();
                    services.AddHostedService<DirectoryMonitorService>();
                });
    }
}
