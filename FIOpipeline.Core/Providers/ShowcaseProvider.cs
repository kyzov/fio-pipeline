using FIOpipeline.Core.DataAccess;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using FIOpipeline.Core.Entity;


namespace FIOpipeline.Core.Providers
{
    public class ShowcaseProvider : IShowcaseProvider
    {
        private readonly AppDbContext _context;

        public ShowcaseProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShowcaseDto>> SearchPersonsAsync(ShowcaseSearchRequest request)
        {
            var persons = await SearchByAnyCriteriaAsync(request);
            return persons.Select(MapToShowcaseDto).ToList();
        }

        private async Task<List<Entity.Person>> SearchByAnyCriteriaAsync(ShowcaseSearchRequest request)
        {
            var query = _context.Persons
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .AsQueryable();

            var hasSearchCriteria = false;

            if (!string.IsNullOrEmpty(request.LastName))
            {
                query = query.Where(p => p.LastName.Contains(request.LastName));
                hasSearchCriteria = true;
            }

            if (!string.IsNullOrEmpty(request.LastName) && !string.IsNullOrEmpty(request.FirstName))
            {
                query = query.Where(p => p.LastName.Contains(request.LastName) &&
                                        p.FirstName.Contains(request.FirstName));
                hasSearchCriteria = true;
            }

            if (!string.IsNullOrEmpty(request.LastName) &&
                !string.IsNullOrEmpty(request.FirstName) &&
                !string.IsNullOrEmpty(request.SecondName))
            {
                query = query.Where(p => p.LastName.Contains(request.LastName) &&
                                        p.FirstName.Contains(request.FirstName) &&
                                        p.SecondName.Contains(request.SecondName));
                hasSearchCriteria = true;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                query = query.Where(p => p.Addresses.Any(a => a.Value.Contains(request.Address)));
                hasSearchCriteria = true;
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                query = query.Where(p => p.Phones.Any(ph => ph.Value.Contains(request.Phone)));
                hasSearchCriteria = true;
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                query = query.Where(p => p.Emails.Any(e => e.Value.Contains(request.Email)));
                hasSearchCriteria = true;
            }

            if (!hasSearchCriteria)
                return new List<Entity.Person>();

            return await query.ToListAsync();
        }

        private ShowcaseDto MapToShowcaseDto(Entity.Person person)
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
                MatchScore = CalculateMatchScore(person)
            };
        }

        private int CalculateMatchScore(Entity.Person person)
        {
            var score = 0;
            if (!string.IsNullOrEmpty(person.LastName)) score += 10;
            if (!string.IsNullOrEmpty(person.FirstName)) score += 10;
            if (!string.IsNullOrEmpty(person.SecondName)) score += 5;
            if (person.Addresses?.Any() == true) score += person.Addresses.Count * 5;
            if (person.Phones?.Any() == true) score += person.Phones.Count * 3;
            if (person.Emails?.Any() == true) score += person.Emails.Count * 2;
            return score;
        }
    }
}
