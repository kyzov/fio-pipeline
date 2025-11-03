using FIOpipeline.Core.DataAccess;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIOpipeline.Core.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;        
using System.Linq;                   
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

            return persons.Select(p => MapToShowcaseDto(p)).ToList();
        }

        public async Task<List<PersonHistoryDto>> GetPersonHistoryAsync(int personId)
        {
            var history = new List<PersonHistoryDto>();

            var initialPerson = await _context.Persons
                .Where(p => p.Id == personId)
                .OrderBy(p => p.ValidFrom)
                .FirstOrDefaultAsync();

            if (initialPerson == null)
                return history;

            var initialAddresses = await _context.Addresses
                .Where(a => a.PersonId == personId && a.ValidFrom == initialPerson.ValidFrom)
                .ToListAsync();

            var initialPhones = await _context.Phones
                .Where(p => p.PersonId == personId && p.ValidFrom == initialPerson.ValidFrom)
                .ToListAsync();

            var initialEmails = await _context.Emails
                .Where(e => e.PersonId == personId && e.ValidFrom == initialPerson.ValidFrom)
                .ToListAsync();


            if (initialPerson == null)
                return history;

            history.Add(new PersonHistoryDto
            {
                Timestamp = initialPerson.ValidFrom,
                Action = "Создание пользователя",
                Details = "Добавлена новая запись",
                PersonData = new PersonSnapshotDto
                {
                    LastName = initialPerson.LastName,
                    FirstName = initialPerson.FirstName,
                    SecondName = initialPerson.SecondName,
                    BirthdayDate = initialPerson.BirthdayDate,
                    Sex = initialPerson.Sex,
                    Addresses = initialPerson.Addresses.Select(a => a.Value).ToList(),
                    Phones = initialPerson.Phones.Select(p => p.Value).ToList(),
                    Emails = initialPerson.Emails.Select(e => e.Value).ToList()
                }
            });

            var addedAddresses = await _context.Addresses
                .Where(a => a.PersonId == personId && a.ValidFrom > initialPerson.ValidFrom)
                .OrderBy(a => a.ValidFrom)
                .ToListAsync();

            foreach (var address in addedAddresses)
            {
                history.Add(new PersonHistoryDto
                {
                    Timestamp = address.ValidFrom,
                    Action = "Добавлен адрес",
                    Details = $"Новый адрес: {address.Value}",
                    FieldChanges = new List<FieldChangeDto>
                    {
                        new FieldChangeDto { FieldName = "Адрес", OldValue = "", NewValue = address.Value }
                    }
                });
            }

            var addedPhones = await _context.Phones
                .Where(p => p.PersonId == personId && p.ValidFrom > initialPerson.ValidFrom)
                .OrderBy(p => p.ValidFrom)
                .ToListAsync();

            foreach (var phone in addedPhones)
            {
                history.Add(new PersonHistoryDto
                {
                    Timestamp = phone.ValidFrom,
                    Action = "Добавлен телефон",
                    Details = $"Новый телефон: {phone.Value}",
                    FieldChanges = new List<FieldChangeDto>
            {
                new FieldChangeDto { FieldName = "Телефон", OldValue = "", NewValue = phone.Value }
            }
                });
            }

            var addedEmails = await _context.Emails
                .Where(e => e.PersonId == personId && e.ValidFrom > initialPerson.ValidFrom)
                .OrderBy(e => e.ValidFrom)
                .ToListAsync();

            foreach (var email in addedEmails)
            {
                history.Add(new PersonHistoryDto
                {
                    Timestamp = email.ValidFrom,
                    Action = "Добавлен email",
                    Details = $"Новый email: {email.Value}",
                    FieldChanges = new List<FieldChangeDto>
            {
                new FieldChangeDto { FieldName = "Email", OldValue = "", NewValue = email.Value }
            }
                });
            }

            return history.OrderBy(h => h.Timestamp).ToList();
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
    }
}
