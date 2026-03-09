using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task.Options
{
    internal class TerminalImportOptions
    {
        public string FilePath { get; set; } = "files/terminals.json";
        public int RunHourUtc { get; set; } = 2;
    }
}
