using Examination.Application.Commands.V1.StartExam;
using Examination.Application.Mapping;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Domain.AggregateModels.ExamResultAggregate;
using Examination.Domain.AggregateModels.UserAggregate;
using Examination.Infrastructure.Repositories;
using Examination.Infrastructure.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Reflection;

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

    builder.Host.UseSerilog();
    builder.Services.AddEndpointsApiExplorer();

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
        var user = builder.Configuration.GetValue<string>("DatabaseSettings:User");
        var password = builder.Configuration.GetValue<string>("DatabaseSettings:Password");
        var server = builder.Configuration.GetValue<string>("DatabaseSettings:Server");
        var databaseName = builder.Configuration.GetValue<string>("DatabaseSettings:DatabaseName");
        return new MongoClient(
            "mongodb://" + user + ":" + password + "@" + server + "/" + databaseName + "?authSource=admin");
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
    });
    builder.Services.Configure<ExamSettings>(builder.Configuration);

    builder.Services.AddTransient<IExamRepository, ExamRepository>();
    builder.Services.AddTransient<IExamResultRepository, ExamResultRepository>();
    builder.Services.AddTransient<IUserRepository, UserRepository>();


    var app = builder.Build();

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

    app.MapControllers();

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


