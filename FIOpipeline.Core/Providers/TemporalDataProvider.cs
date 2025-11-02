using FIOpipeline.Core.DataAccess;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIOpipeline.Core.Entity;
using Microsoft.EntityFrameworkCore; // Для CountAsync, ToListAsync и других async методов
using System.Threading.Tasks;        // Для Task
using System.Linq;                   // Для LINQ методов
using System.Collections.Generic;

namespace FIOpipeline.Core.Providers
{
    public class TemporalDataService : ITemporalDataService
    {
        private readonly AppDbContext _context;

        public TemporalDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShowcaseDto>> SearchPersonsAtMomentAsync(ShowcaseSearchRequest request, DateTime moment)
        {
            var query = _context.Persons
                .Where(p => p.ValidFrom <= moment && p.ValidTo > moment && p.IsCurrent)
                .Include(p => p.Addresses.Where(a => a.ValidFrom <= moment && a.ValidTo > moment))
                .Include(p => p.Phones.Where(ph => ph.ValidFrom <= moment && ph.ValidTo > moment))
                .Include(p => p.Emails.Where(e => e.ValidFrom <= moment && e.ValidTo > moment))
                .AsQueryable();

            // Применяем фильтры поиска
            if (!string.IsNullOrEmpty(request.LastName))
                query = query.Where(p => p.LastName.Contains(request.LastName));

            if (!string.IsNullOrEmpty(request.FirstName))
                query = query.Where(p => p.FirstName.Contains(request.FirstName));

            if (!string.IsNullOrEmpty(request.Phone))
                query = query.Where(p => p.Phones.Any(ph => ph.Value.Contains(request.Phone)));

            if (!string.IsNullOrEmpty(request.Email))
                query = query.Where(p => p.Emails.Any(e => e.Value.Contains(request.Email)));

            if (!string.IsNullOrEmpty(request.Address))
                query = query.Where(p => p.Addresses.Any(a => a.Value.Contains(request.Address)));

            var persons = await query.ToListAsync();

            // Явно указываем пространство имен
            return persons.Select(p => MapToShowcaseDto(p)).ToList();
        }

        public async Task<List<PersonHistoryDto>> GetPersonHistoryAsync(int personId)
        {
            var history = await _context.Persons
                .Where(p => p.Id == personId)
                .OrderByDescending(p => p.ValidFrom)
                .Select(p => new PersonHistoryDto
                {
                    PersonId = p.Id,
                    LastName = p.LastName,
                    FirstName = p.FirstName,
                    SecondName = p.SecondName,
                    ValidFrom = p.ValidFrom,
                    ValidTo = p.ValidTo,
                    Version = p.Version,
                    IsCurrent = p.IsCurrent,
                    Addresses = p.Addresses.Select(a => a.Value).ToList(),
                    Phones = p.Phones.Select(ph => ph.Value).ToList(),
                    Emails = p.Emails.Select(e => e.Value).ToList()
                })
                .ToListAsync();

            return history;
        }

        public async Task<SystemSnapshotDto> GetSystemSnapshotAsync(DateTime moment)
        {
            var totalPersons = await _context.Persons
                .CountAsync(p => p.ValidFrom <= moment && p.ValidTo > moment);

            var recentChanges = await _context.Persons
                .Where(p => p.ValidFrom <= moment && p.ValidFrom >= moment.AddDays(-7))
                .GroupBy(p => p.ValidFrom.Date)
                .Select(g => new ChangeStatisticDto
                {
                    Date = g.Key,
                    ChangesCount = g.Count()
                })
                .ToListAsync();

            return new SystemSnapshotDto
            {
                SnapshotMoment = moment,
                TotalPersons = totalPersons,
                RecentChanges = recentChanges
            };
        }

        private ShowcaseDto MapToShowcaseDto(FIOpipeline.Core.Entity.Person person)
        {
            return new ShowcaseDto
            {
                PersonId = person.Id,
                FullName = $"{person.LastName} {person.FirstName} {person.SecondName}",
                BirthdayDate = person.BirthdayDate,
                Sex = person.Sex,
                Addresses = person.Addresses?.Select(a => a.Value).ToList() ?? new List<string>(),
                Phones = person.Phones?.Select(p => p.Value).ToList() ?? new List<string>(),
                Emails = person.Emails?.Select(e => e.Value).ToList() ?? new List<string>(),
                ValidFrom = person.ValidFrom,
                ValidTo = person.ValidTo
            };
        }

        // Реализация RestoreToMomentAsync будет сложнее, зависит от требований
        public Task<bool> RestoreToMomentAsync(DateTime moment)
        {
            // Логика восстановления данных на определенный момент
            throw new NotImplementedException();
        }
    }
}
