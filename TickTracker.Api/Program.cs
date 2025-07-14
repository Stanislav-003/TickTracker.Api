using Carter;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TickTracker.Api.Abstractions;
using TickTracker.Api.Configurations;
using TickTracker.Api.Database;
using TickTracker.Api.DependencyInjection;
using TickTracker.Api.Extensions;
using TickTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFintacharts(builder.Configuration);

builder.Services.AddCarter();

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Cache"));

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterConfig).Assembly);
MapsterConfig.Register(TypeAdapterConfig.GlobalSettings);
builder.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
builder.Services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.MapCarter();

app.UseHttpsRedirection();

app.Run();
