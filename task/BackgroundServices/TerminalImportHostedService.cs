using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task.Options;
using task.Services.Interfaces;

namespace task.BackgroundServices
{
    internal class TerminalImportHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TerminalImportHostedService> _logger;
        private readonly TerminalImportOptions _options;

        public TerminalImportHostedService(
            IServiceProvider serviceProvider,
            ILogger<TerminalImportHostedService> logger,
            IOptions<TerminalImportOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Terminal import worker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelay();

                    _logger.LogInformation("Следующий запуск импорта через {Delay}", delay);

                    await Task.Delay(delay, stoppingToken);

                    await RunImport(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка импорта: {Exception}", ex.Message);
                }
            }
        }

        private async Task RunImport(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var importService = scope.ServiceProvider.GetRequiredService<ITerminalImportService>();

            var filePath = Path.Combine(AppContext.BaseDirectory, _options.FilePath);

            _logger.LogInformation("Запуск импорта терминалов из файла {Path}", filePath);

            await importService.ImportAsync(filePath, stoppingToken);
        }

        private TimeSpan GetDelay()
        {
            var now = DateTime.UtcNow;

            var nextRun = now.Date.AddHours(_options.RunHourUtc);

            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            return nextRun - now;
            
        }
    }
}
