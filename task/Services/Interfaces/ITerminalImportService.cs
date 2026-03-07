using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task.Services.Interfaces
{
    public interface ITerminalImportService
    {
        Task ImportAsync(string filePath, CancellationToken cancellationToken);
    }
}
