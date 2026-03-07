using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task.Services.Interfaces;

namespace task.BackgroundServices
{
    internal class TerminalImportHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TerminalImportHostedService> _logger;

        public TerminalImportHostedService(IServiceProvider serviceProvider, ILogger<TerminalImportHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Terminal import worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // TODO проверить корректность формирования даты.
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1).AddHours(2);

                    if (now.Hour < 2)
                        nextRun = now.Date.AddHours(2);

                    var delay = nextRun - now;

                    _logger.LogInformation("Следующий запуск импорта через {Delay}", delay);

                    await Task.Delay(delay, stoppingToken);

                    using var scope = _serviceProvider.CreateScope();

                    var importService = scope.ServiceProvider.GetRequiredService<ITerminalImportService>();

                    var filePath = Path.Combine(
                        AppContext.BaseDirectory,
                        "files",
                        "terminals.json");

                    await importService.ImportAsync(filePath, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка импорта: {Exception}", ex.Message);
                }
            }
        }
    }
}
