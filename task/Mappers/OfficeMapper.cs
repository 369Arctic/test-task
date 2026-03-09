using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using task.Models;
using task.Models.Dto;

namespace task.Mappers
{
    internal class OfficeMapper
    {
        /// <summary>
        /// Преобразовать данные из TerminalDto и CityDto в сущность Office.
        /// </summary>
        /// <param name="terminalDto">DTO терминала.</param>
        /// <param name="cityDto">DTO города.</param>
        /// <returns>Экземпляр <see cref="Office"/> с заполненными свойствами, включая координаты и телефоны.</returns>

        public static Office Map(TerminalDto terminalDto, CityDto cityDto)
        {
            double.TryParse(terminalDto.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude);
            double.TryParse(terminalDto.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude);

            var (street, house) = ParseAddress(terminalDto.Address);

            return new Office
            {
                Code = terminalDto.Id,
                Uuid = terminalDto.Uuid,
                CityCode = cityDto.CityID,

                AddressStreet = street,
                AddressHouseNumber = house,
                AddressCity = cityDto.Name,

                Type = ParseOfficeType(terminalDto),

                Coordinates = new Coordinates
                {
                    Latitude = latitude,
                    Longitude = longitude,
                },

                Phones = terminalDto.Phones?.Select(p => new Phone
                {
                    PhoneNumber = p.Number ?? "",
                    Additional = p.Comment
                }).ToList() ?? new(),

                WorkTime = terminalDto.CalcSchedule != null
                    ? $"{terminalDto.CalcSchedule.Derival}; {terminalDto.CalcSchedule.Arrival}"
                    : ""
            };
        }

        /// <summary>
        /// Разбить строку адреса на отдельные части: улицу и номер дома.
        /// </summary>
        /// <param name="address">Строка адреса в формате "улица, дом".</param>
        /// <returns>
        /// Кортеж, где <c>Item1</c> — улица (street), 
        /// <c>Item2</c> — номер дома (house). 
        /// Если адрес отсутствует, возвращается (null, null).
        /// </returns>
        private static (string?, string?) ParseAddress(string? address)
        {
            if (string.IsNullOrEmpty(address))
                return (null, null);

            var parts = address.Split(',');

            return (
                parts.Length > 0 ? parts[0].Trim() : null,
                parts.Length > 1 ? parts[1].Trim() : null
            );
        }

        /// <summary>
        /// Определить тип офиса на основе свойств TerminalDto.
        /// </summary>
        /// <param name="terminalDto">DTO терминала.</param>
        /// <returns>Тип офиса <see cref="OfficeType"/></returns>
        private static OfficeType ParseOfficeType(TerminalDto terminalDto)
        {
            if (terminalDto.IsPVZ == true)
                return OfficeType.PVZ;

            if (terminalDto.Storage == true && terminalDto.ReceiveCargo == true && terminalDto.GiveoutCargo == true)
                return OfficeType.WAREHOUSE;

            return OfficeType.POSTAMAT;
        }
    }
}
