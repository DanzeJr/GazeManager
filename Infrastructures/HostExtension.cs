using System.Threading.Tasks;
using GazeManager.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GazeManager.Infrastructures
{
    public static class HostExtension
    {
        public static async Task<IHost> SeedDataAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetService<AppDbContext>();
                var configuration = services.GetService<IConfiguration>();

                if (configuration.GetSection("AppConfig:Database:DeleteOnDeploy").Get<bool>())
                {
                    context.Database.EnsureDeleted();
                }

                await DatabaseSeeder.InitializeAsync(context, configuration);
            }

            return host;
        }
    }
}