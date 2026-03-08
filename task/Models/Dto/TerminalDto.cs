using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task.Models.Dto
{
    internal class TerminalDto
    {
        public string? Id { get; set; }
        public string? Uuid { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public bool? IsPVZ { get; set; }
        public bool? Storage { get; set; }
        public bool? ReceiveCargo { get; set; }
        public bool? GiveoutCargo { get; set; }
        public bool? IsOffice { get; set; }

        public List<PhoneDto>? Phones { get; set; }
        public CalcScheduleDto? CalcSchedule { get; set; }
    }
}
