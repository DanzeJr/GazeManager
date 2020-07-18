using System.Threading.Tasks;
using GazeManager.Infrastructures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GazeManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = await CreateHostBuilder(args).Build().SeedDataAsync();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
