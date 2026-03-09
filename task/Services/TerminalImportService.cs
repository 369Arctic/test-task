using System.Diagnostics;
using System.Text.Json;
using task.Mappers;
using task.Models;
using task.Models.Dto;
using task.Repositories;
using task.Services.Interfaces;

namespace task.Services
{
    internal class TerminalImportService : ITerminalImportService
    {
        private readonly IOfficeRepository _officeRepository;
        private readonly ILogger<TerminalImportService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TerminalImportService(
            IOfficeRepository officeRepository,
            ILogger<TerminalImportService> logger)
        {
            _officeRepository = officeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Выполнить импорт терминалов из JSON-файла.
        /// </summary>
        /// <param name="filePath">Путь к JSON-файлу с данными терминалов.</param>
        /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
        /// <returns>Асинхронная задача импорта терминалов.</returns>
        public async Task ImportAsync(string filePath, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Файл с терминалами не найден. Путь: {Path}", filePath);
                    return;
                }

                var root = await DeserializeAsync(filePath, cancellationToken);

                var terminals = ExtractTerminals(root);

                var offices = terminals
                    .Select(u => OfficeMapper.Map(u.Terminal, u.City))
                    .ToList();

                _logger.LogInformation("Загружено {Count} терминалов из JSON", offices.Count);

                await _officeRepository.ReplaceAllAsync(offices, cancellationToken);

                _logger.LogInformation("Сохранено {Count} новых терминалов", offices.Count);
                _logger.LogInformation("Время импорта: {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка импорта: {Exception}", ex);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Десериализация JSON-файла в объект <see cref="RootDto"/>.
        /// </summary>
        /// <param name="filePath">Путь к JSON-файлу.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Объект <see cref="RootDto"/> с данными терминалов и городов.</returns>
        private async Task<RootDto?> DeserializeAsync(string filePath, CancellationToken cancellationToken)
        {
            using var stream = File.OpenRead(filePath);

            return await JsonSerializer.DeserializeAsync<RootDto>(
                stream,
                JsonOptions,
                cancellationToken);
        }

        /// <summary>
        /// Извлечь список терминалов с привязкой к городам из DTO корневого объекта.
        /// </summary>
        /// <param name="root">Корневой DTO объект <see cref="RootDto"/>.</param>
        /// <returns>Список кортежей (терминал, город) для дальнейшего маппинга.</returns>
        private List<(TerminalDto Terminal, CityDto City)> ExtractTerminals(RootDto? root)
        {
            if (root?.City == null)
                return new();

            return root.City
                .Where(c => c.Terminals?.Terminal != null)
                .SelectMany(c => c.Terminals!.Terminal!
                     .Select(t => (Terminal: t, City: c)))
                .ToList();
        }
    }
}
