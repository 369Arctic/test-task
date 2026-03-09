using task.Models;

namespace task.Repositories
{
    public interface IOfficeRepository
    {
        public Task ReplaceAllAsync(List<Office> offices, CancellationToken token);
    }
}
