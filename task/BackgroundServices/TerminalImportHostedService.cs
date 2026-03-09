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
    /// <summary>
    /// Фоновый сервис, выполняющий периодический импорт терминалов.
    /// </summary>
    /// <remarks>
    /// Сервис работает на протяжении всего времени жизни приложения.
    /// Он рассчитывает задержку до следующего запуска и выполняет импорт
    /// данных терминалов с помощью <see cref="ITerminalImportService"/>.
    /// </remarks>

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

        /// <summary>
        /// Основной цикл выполнения фонового сервиса.
        /// </summary>
        /// <param name="stoppingToken">
        /// Токен отмены, сигнализирующий об остановке приложения.
        /// </param>
        /// <returns>Асинхронная задача выполнения сервиса.</returns>
        /// <remarks>
        /// Сервис работает в бесконечном цикле до остановки приложения.
        /// Перед каждым запуском импорта рассчитывается задержка до следующего
        /// запланированного времени выполнения.
        /// </remarks>
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

        /// <summary>
        /// Выполнить импорт терминалов из файла.
        /// </summary>
        /// <param name="stoppingToken">
        /// Токен отмены для корректной остановки операции.
        /// </param>
        /// <returns>Асинхронная задача выполнения импорта.</returns>
        private async Task RunImport(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var importService = scope.ServiceProvider.GetRequiredService<ITerminalImportService>();

            var filePath = Path.Combine(AppContext.BaseDirectory, _options.FilePath);

            _logger.LogInformation("Запуск импорта терминалов из файла {Path}", filePath);

            await importService.ImportAsync(filePath, stoppingToken);
        }

        /// <summary>
        /// Вычислить задержку до следующего запуска импорта.
        /// </summary>
        /// <returns>Временной интервал до следующего запуска задачи.</returns>
        /// <remarks>
        /// Импорт выполняется один раз в сутки в час, указанный в настройках
        /// <see cref="TerminalImportOptions.RunHourUtc"/>. Для вычислений используется UTC-время.
        /// </remarks>
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
