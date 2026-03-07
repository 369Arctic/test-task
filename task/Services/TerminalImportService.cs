using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using task.Data;
using task.Models;
using task.Services.Interfaces;

namespace task.Services
{
    internal class TerminalImportService : ITerminalImportService
    {
        private readonly DellinDictionaryDbContext _dbContext;
        private readonly ILogger<TerminalImportService> _logger;

        public TerminalImportService(DellinDictionaryDbContext dbContext, ILogger<TerminalImportService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task ImportAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Файл с терминалами не найден. Путь: {Path}", filePath);
                    return;
                }

                var stopwatch = Stopwatch.StartNew();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                await using var stream = File.OpenRead(filePath);

                var offices = await JsonSerializer.DeserializeAsync<List<Office>>(
                    stream,
                    options,
                    cancellationToken) ?? new();

                _logger.LogInformation("Загружено {Count} терминалов из JSON", offices.Count);

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                var oldCount = await _dbContext.Offices.CountAsync(cancellationToken);

                await _dbContext.Database.ExecuteSqlRawAsync(
                    "TRUNCATE TABLE \"Offices\" CASCADE",
                    cancellationToken);

                _logger.LogInformation("Удалено {OldCount} старых записей", oldCount);

                await _dbContext.Offices.AddRangeAsync(offices, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation("Сохранено {NewCount} новых терминалов", offices.Count);
                _logger.LogInformation("Время импорта: {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка импорта: {Exception}", ex.Message);
            }
        }
    }
}
