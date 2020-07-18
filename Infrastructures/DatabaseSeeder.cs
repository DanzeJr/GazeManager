using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using GazeManager.Models;
using GazeManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GazeManager.Infrastructures
{
    public class DatabaseSeeder
    {
        public static async Task InitializeAsync(AppDbContext context, IConfiguration configuration)
        {
            // now we have the DbContext. Run migrations
            context.Database.Migrate();

            // now that the database is up to date. Let's seed
            var appConfig = await context.Configuration.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
            if (appConfig == null)
            {
                appConfig = configuration.GetSection("AppConfig:DefaultConfiguration").Get<Configuration>();
                appConfig.CreatedDate = DateTime.Now;

                await context.Configuration.AddAsync(appConfig);
                await context.SaveChangesAsync();
            }

            var adminInfo = configuration.GetSection("AppConfig:Firebase:Admin").Get<UserRecordArgs>();
            var user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(adminInfo.Email);
            if (user == null)
            {
                user = await FirebaseAuth.DefaultInstance.CreateUserAsync(adminInfo);
            }
            else
            {
                adminInfo.Uid = user.Uid;
                user = await FirebaseAuth.DefaultInstance.UpdateUserAsync(adminInfo);
            }

            var admin = await context.User.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (admin == null)
            {
                admin = new User();
                admin.Email = user.Email;
                admin.Name = user.DisplayName;
                admin.Avatar = user.PhotoUrl;
                admin.Phone = user.PhoneNumber;
                admin.Role = Role.Admin;
                admin.CreatedDate = DateTime.Now;

                await context.User.AddAsync(admin);
                await context.SaveChangesAsync();

                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.Uid, new Dictionary<string, object>
                    {
                        {"gazeId", admin.Id},
                        {ClaimTypes.Role, Role.Admin},
                        {ClaimTypes.Email, user.Email}
                    });
            }
        }
    }
}