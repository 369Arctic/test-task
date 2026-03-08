using Microsoft.EntityFrameworkCore;
using task;
using task.BackgroundServices;
using task.Data;
using task.Services;
using task.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DellinDictionaryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<ITerminalImportService, TerminalImportService>();
builder.Services.AddHostedService<TerminalImportHostedService>();


var host = builder.Build();

// TODO тестовый запуск. удалить после успешного тестирования.
using (var scope = host.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<ITerminalImportService>();

    var filePath = Path.Combine(
        AppContext.BaseDirectory,
        "files",
        "terminals.json");

    await service.ImportAsync(filePath, CancellationToken.None);
}

await host.RunAsync();
