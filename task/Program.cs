using Microsoft.EntityFrameworkCore;
using task;
using task.BackgroundServices;
using task.Data;
using task.Options;
using task.Repositories;
using task.Services;
using task.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DellinDictionaryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

builder.Services.Configure<TerminalImportOptions>(
    builder.Configuration.GetSection("TerminalImport"));

builder.Services.AddScoped<ITerminalImportService, TerminalImportService>();
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();

builder.Services.AddHostedService<TerminalImportHostedService>();

var host = builder.Build();

await host.RunAsync();
