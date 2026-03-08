using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task.Data;
using task.Models;
using task.Services;

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

        public async Task ReplaceAllAsync(List<Office> offices, CancellationToken token)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);

            var oldCount = await _dbContext.Offices.CountAsync(token);

            await _dbContext.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"Offices\" CASCADE",
                token);

            _logger.LogInformation("Удалено {OldCount} старых записей", oldCount);

            await _dbContext.Offices.AddRangeAsync(offices, token);
            await _dbContext.SaveChangesAsync(token);

            await transaction.CommitAsync(token);
        }
    }
}
