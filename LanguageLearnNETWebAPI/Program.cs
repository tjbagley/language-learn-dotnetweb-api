using LanguageLearnNETWebAPI.Mapping;
using LanguageLearnNETWebAPI.Middleware;
using LanguageLearnNETWebAPI.Models.AppSettings;
using LanguageLearnNETWebAPI.Repositories;
using LanguageLearnNETWebAPI.Services;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<APISettings>(
    builder.Configuration.GetSection("API"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IWordRepository, WordRepository>();
builder.Services.AddScoped<IWordService, WordService>();

builder.Services.AddAutoMapper(cfg => {}, typeof(MappingProfile));

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
