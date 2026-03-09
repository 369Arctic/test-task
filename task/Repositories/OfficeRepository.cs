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
