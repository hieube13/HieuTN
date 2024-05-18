using System.Reflection;
using Identity.API;
using Identity.API.Database;
using Identity.API.Extensions;
using Identity.API.Models;
using Identity.API.Services;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var seed = args.Contains("/seed");
if (seed)
{
    args = args.Except(new[] { "/seed" }).ToArray();
}
string migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (seed)
{
    SeedData.EnsureSeedData(connectionString);
}

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString,
sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(migrationsAssembly);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    })
);

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(x =>
            {
                x.IssuerUri = "https://tedu.com.vn"; //
                x.Authentication.CookieLifetime = TimeSpan.FromHours(2);
            })
            .AddDeveloperSigningCredential()
            .AddAspNetIdentity<ApplicationUser>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            })
            .Services.AddTransient<IProfileService, ProfileService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();

app.Run();


