using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.API.Configuration;
using Identity.API.Database;
using Identity.API.Models;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.API
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(connectionString)
            );

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOperationalDbContext(
                options =>
                {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(
                            connectionString,
                            sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName)
                        );
                }
            );
            services.AddConfigurationDbContext(
                options =>
                {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(
                            connectionString,
                            sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName)
                        );
                }
            );

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();

            EnsureSeedData(context);

            var ctx = scope.ServiceProvider.GetService<ApplicationDbContext>();
            ctx.Database.Migrate();
            EnsureUsers(scope);
        }

        private static void EnsureUsers(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var hieubui = userMgr.FindByNameAsync("HieuBui").Result;
            if (hieubui == null)
            {
                hieubui = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "HieuBui",
                    LastName = "Hieu",
                    FirstName = "Bui",
                    Email = "hieubui.chuy@email.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(hieubui, "Hieu123!").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result =
                    userMgr.AddClaimsAsync(
                        hieubui,
                        new Claim[]
                        {
                            new Claim(JwtClaimTypes.Name, "BuiMinhHieu"),
                            new Claim(JwtClaimTypes.GivenName, "HieuBui"),
                            new Claim(JwtClaimTypes.FamilyName, "Bui"),
                            new Claim(JwtClaimTypes.WebSite, "http://angellafreeman.com"),
                            new Claim("location", "somewhere")
                        }
                    ).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            // Khởi tạo configuration builder
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Đặt đường dẫn cơ bản cho file secret.json
                .AddUserSecrets<SeedData>(); // Thêm User Secrets cho SeedData

            var configuration = builder.Build(); // Xây dựng IConfiguration

            // Đọc các giá trị từ secret.json
            var clientUrls = new Dictionary<string, string>
            {
                { "ExamWebApp", configuration["ExamWebAppClient"] },
                { "ExamWebAdmin", configuration["ExamWebAdminClient"] },
                { "ExamWebApi", configuration["ExamWebApiClient"] }
            };

            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients(clientUrls).ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.GetIdentityResources().ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.GetApiScopes().ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.GetApis().ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
        }
    }
}