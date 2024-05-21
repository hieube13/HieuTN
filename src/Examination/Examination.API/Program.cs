using Examination.API.Filter;
using Examination.Application.Commands.V1.StartExam;
using Examination.Application.Mapping;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Domain.AggregateModels.ExamResultAggregate;
using Examination.Domain.AggregateModels.UserAggregate;
using Examination.Infrastructure;
using Examination.Infrastructure.Repositories;
using Examination.Infrastructure.SeedWork;
using HealthChecks.UI.Client;
using Humanizer.Configuration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;

string appName = typeof(Program).Namespace;
var configuration = GetConfiguration();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("ApplicationContext", appName)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true)
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

IConfiguration GetConfiguration()
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets(typeof(Program).Assembly);

    var config = configBuilder.Build();
    return config;
}

Log.Information("Hello Hieu Starting up!");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddConfiguration(configuration);

    builder.Host.UseSerilog();
    builder.Services.AddEndpointsApiExplorer();

    var user = builder.Configuration.GetValue<string>("DatabaseSettings:User");
    var password = builder.Configuration.GetValue<string>("DatabaseSettings:Password");
    var server = builder.Configuration.GetValue<string>("DatabaseSettings:Server");
    var databaseName = builder.Configuration.GetValue<string>("DatabaseSettings:DatabaseName");
    var mongodbConnectionString = "mongodb://" + user + ":" + password + "@" + server + "/" + databaseName + "?authSource=admin";

    builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
    });
    builder.Services.AddVersionedApiExplorer(
        options =>
        {
            // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            options.GroupNameFormat = "'v'VVV";

            // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            // can also be used to control the format of the API version in route templates
            options.SubstituteApiVersionInUrl = true;
        }
    );

    builder.Services.AddSingleton<IMongoClient>(c =>
    {
        return new MongoClient(mongodbConnectionString);
    });

    builder.Services.AddScoped(c => c.GetService<IMongoClient>()?.StartSession());

    builder.Services.AddAutoMapper(cfg => { cfg.AddProfile(new MappingProfile()); });
    builder.Services.AddMediatR(typeof(StartExamCommandHandler).Assembly);
    //builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));
    builder.Services.AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Examination.API V1", Version = "v1" });
        c.SwaggerDoc("v2", new OpenApiInfo { Title = "Examination.API V2", Version = "v2" });

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows()
            {
                Implicit = new OpenApiOAuthFlow()
                {
                    AuthorizationUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrl")}/connect/authorize"),
                    TokenUrl = new Uri($"{builder.Configuration.GetValue<string>("IdentityUrl")}/connect/token"),
                    Scopes = new Dictionary<string, string>()
                            {
                                {"full_access", "Full Access"},
                            }
                }
            }
        });
        c.OperationFilter<AuthorizeCheckOperationFilter>();
    });

    var identityUrl = builder.Configuration.GetValue<string>("IdentityUrl");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults
            .AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults
            .AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.Authority = identityUrl;
        options.RequireHttpsMetadata = false;
        options.Audience = "exam_api";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

    builder.Services.Configure<ExamSettings>(builder.Configuration);

    builder.Services.AddHealthChecks()
                    .AddCheck("self", () => HealthCheckResult.Healthy())
                    .AddMongoDb(mongodbConnectionString: mongodbConnectionString,
                                name: "mongo",
                                failureStatus: HealthStatus.Unhealthy);

    builder.Services.AddHealthChecksUI(opt =>
    {
        opt.SetEvaluationTimeInSeconds(15); //time in seconds between check
        opt.MaximumHistoryEntriesPerEndpoint(60); //maximum history of checks
        opt.SetApiMaxActiveRequests(1); //api requests concurrency 

        opt.AddHealthCheckEndpoint("Exam API", "/hc"); //map health check api
    })
    .AddInMemoryStorage();

    builder.Services.AddTransient<IExamRepository, ExamRepository>();
    builder.Services.AddTransient<IExamResultRepository, ExamResultRepository>();
    builder.Services.AddTransient<IUserRepository, UserRepository>();


    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ExamMongoDbSeeding>>();
        var settings = services.GetRequiredService<IOptions<ExamSettings>>();
        var mongoClient = services.GetRequiredService<IMongoClient>();
        new ExamMongoDbSeeding()
            .SeedAsync(mongoClient, settings, logger)
            .Wait();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Examination.API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "Examination.API v2");
        });
    }

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseCors("CorsPolicy");

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

        endpoints.MapControllers();
    });

    app.Run();

    Log.Information("Stopped cleanly");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", appName);

    return 1;
}
finally
{
    Log.CloseAndFlush();
}


