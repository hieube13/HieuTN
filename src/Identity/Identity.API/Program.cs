using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using HealthChecks.UI.Client;
using Identity.API;
using Identity.API.Database;
using Identity.API.Extensions;
using Identity.API.Models;
using Identity.API.Services;
using IdentityServer4.AspNetIdentity;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;


var seed = args.Contains("/seed");
if (seed)
{
    args = args.Except(new[] { "/seed" }).ToArray();
}

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddSqlServer(
        connectionString,
        name: "IdentityDB-check",
        tags: new string[] { "identitydb" });


builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
    opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
    opt.SetApiMaxActiveRequests(1); //api requests concurrency

    opt.AddHealthCheckEndpoint("Identity API", "/hc"); //map health check api
})
.AddInMemoryStorage();

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

app.UseStaticFiles();

app.UseIdentityServer();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI(options => options.UIPath = "/hc-ui");
    endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
    {
        Predicate = r => r.Name.Contains("self")
    });
    endpoints.MapHealthChecks("/hc-details",
        new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var result = JsonSerializer.Serialize(
                    new
                    {
                        status = report.Status.ToString(),
                        monitors = report.Entries.Select(e => new { key = e.Key, value = Enum.GetName(typeof(HealthStatus), e.Value.Status) })
                    });
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            }
        }
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();


