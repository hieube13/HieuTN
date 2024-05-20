using Examination.Application.Commands.StartExam;
using Examination.Application.Mapping;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Domain.AggregateModels.ExamResultAggregate;
using Examination.Domain.AggregateModels.UserAggregate;
using Examination.Infrastructure.Repositories;
using Examination.Infrastructure.SeedWork;
using MediatR;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Examination.API", Version = "v1" });
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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
