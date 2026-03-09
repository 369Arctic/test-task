using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using task.Data;
using task.Models;

namespace task.Repositories
{
    internal class OfficeRepository : IOfficeRepository
    {
        private readonly DellinDictionaryDbContext _dbContext;
        private readonly ILogger<OfficeRepository> _logger;

        public OfficeRepository(DellinDictionaryDbContext dbContext, ILogger<OfficeRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Заменить все данные офисов и телефонов в БД.
        /// </summary>
        /// <param name="offices">Список офисов для импорта.</param>
        /// <param name="ct">Токен отмены для асинхронных операций.</param>
        /// <remarks>
        /// Логика работы метода:
        /// 1. Создать транзакцию для обеспечения атомарности операций.
        /// 2. Полностью очистить таблицы "Phone" и "Offices", сбросив идентификаторы.
        /// 3. Выполнить BulkInsert для офисов.
        /// 4. Получить актуальные идентификаторы офисов из БД.
        /// 5. Привязать телефоны к соответствующим офисам и выполняет BulkInsert телефонов.
        /// 6. Зафиксировать транзакцию.
        /// </remarks>
        public async Task ReplaceAllAsync(List<Office> offices, CancellationToken ct)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Phone\" RESTART IDENTITY CASCADE", ct);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Offices\" RESTART IDENTITY CASCADE", ct);

            await _dbContext.BulkInsertAsync(offices, cancellationToken: ct);

            var officeIds = await _dbContext.Offices
                .Select(o => new { o.Id, o.Code })
                .ToDictionaryAsync(o => o.Code!, o => o.Id, ct);

            var phones = new List<Phone>();

            foreach (var office in offices)
            {
                if (!officeIds.TryGetValue(office.Code!, out var officeId))
                    continue;

                foreach (var phone in office.Phones)
                {
                    phone.OfficeId = officeId;
                    phones.Add(phone);
                }
            }

            if (phones.Count > 0)
                await _dbContext.BulkInsertAsync(phones, cancellationToken: ct);

            await transaction.CommitAsync(ct);
        }
    }
}
