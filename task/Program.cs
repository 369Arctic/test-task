using Microsoft.EntityFrameworkCore;
using task;
using task.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DellinDictionaryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
