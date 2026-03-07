using Microsoft.EntityFrameworkCore;
using task;
using task.Data;
using task.Services;
using task.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DellinDictionaryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<ITerminalImportService, TerminalImportService>();


var host = builder.Build();
host.Run();
