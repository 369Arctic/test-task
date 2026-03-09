using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task.Models.Dto
{
    internal class CityDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? CityID { get; set; }
        public TerminalsDto? Terminals{ get; set; }
    }
}
