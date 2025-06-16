using DuplicateCheckerService.Configuration;
using DuplicateCheckerService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DuplicateCheckerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServiceSettings>(
                        hostContext.Configuration.GetSection("ServiceSettings"));
                    
                    services.AddSingleton<ServiceSettings>();
                    services.AddHostedService<DirectoryMonitorService>();
                });
    }
}
