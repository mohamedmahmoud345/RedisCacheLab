using Microsoft.EntityFrameworkCore;
using RedisCacheLab.Context;
using RedisCacheLab.Repositories;
using RedisCacheLab.Repositories.Interfaces;
using RedisCacheLab.Services;
using RedisCacheLab.Services.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConfiguration = builder.Configuration.GetConnectionString("Redis");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(ops =>
{
    ops.UseSqlServer(builder.Configuration.GetConnectionString("conStr"));
});

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConfiguration!));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfiguration;
    options.InstanceName = "MyAppCache";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
