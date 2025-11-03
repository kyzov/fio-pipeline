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
            if (IsRequestEmpty(request))
                return new List<ShowcaseDto>();

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

            var conditions = new List<System.Linq.Expressions.Expression<Func<Entity.Person, bool>>>();

            if (!string.IsNullOrEmpty(request.LastName))
            {
                conditions.Add(p => p.LastName != null && p.LastName.Contains(request.LastName));
            }

            if (!string.IsNullOrEmpty(request.FirstName))
            {
                conditions.Add(p => p.FirstName != null && p.FirstName.Contains(request.FirstName));
            }

            if (!string.IsNullOrEmpty(request.SecondName))
            {
                conditions.Add(p => p.SecondName != null && p.SecondName.Contains(request.SecondName));
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                conditions.Add(p => p.Addresses != null &&
                                   p.Addresses.Any(a => a.Value != null &&
                                                       a.Value.Contains(request.Address)));
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                var searchPhone = request.Phone.Trim();

                query = query.Where(p => p.Phones != null && p.Phones.Any(ph =>
                    ph.Value != null && ph.Value.Contains(searchPhone)));

                Console.WriteLine($"Searching for phone substring: '{searchPhone}'");
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                conditions.Add(p => p.Emails != null &&
                                   p.Emails.Any(e => e.Value != null &&
                                                    e.Value.Contains(request.Email)));
            }

            if (conditions.Any())
            {
                var combinedCondition = conditions[0];
                for (int i = 1; i < conditions.Count; i++)
                {
                    var currentCondition = conditions[i];
                    combinedCondition = Or(combinedCondition, currentCondition);
                }
                query = query.Where(combinedCondition);
            }

            return await query.ToListAsync();
        }

        private System.Linq.Expressions.Expression<Func<Entity.Person, bool>> Or(
            System.Linq.Expressions.Expression<Func<Entity.Person, bool>> expr1,
            System.Linq.Expressions.Expression<Func<Entity.Person, bool>> expr2)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Entity.Person));

            var body = System.Linq.Expressions.Expression.OrElse(
                System.Linq.Expressions.Expression.Invoke(expr1, parameter),
                System.Linq.Expressions.Expression.Invoke(expr2, parameter));

            return System.Linq.Expressions.Expression.Lambda<Func<Entity.Person, bool>>(body, parameter);
        }

        private bool IsRequestEmpty(ShowcaseSearchRequest request)
        {
            return string.IsNullOrEmpty(request.LastName) &&
                   string.IsNullOrEmpty(request.FirstName) &&
                   string.IsNullOrEmpty(request.SecondName) &&
                   string.IsNullOrEmpty(request.Address) &&
                   string.IsNullOrEmpty(request.Phone) &&
                   string.IsNullOrEmpty(request.Email);
        }

        private ShowcaseDto MapToShowcaseDto(Entity.Person person)
        {
            return new ShowcaseDto
            {
                PersonId = person.Id,
                FullName = $"{person.LastName} {person.FirstName} {person.SecondName}",
                BirthdayDate = person.BirthdayDate,
                Sex = person.Sex,
                Addresses = person.Addresses?.Where(a => a.Value != null).Select(a => a.Value).ToList() ?? new List<string>(),
                Phones = person.Phones?.Where(p => p.Value != null).Select(p => p.Value).ToList() ?? new List<string>(),
                Emails = person.Emails?.Where(e => e.Value != null).Select(e => e.Value).ToList() ?? new List<string>(),
                MatchScore = CalculateMatchScore(person)
            };
        }

        private int CalculateMatchScore(Entity.Person person)
        {
            var score = 0;
            if (!string.IsNullOrEmpty(person.LastName)) score += 10;
            if (!string.IsNullOrEmpty(person.FirstName)) score += 10;
            if (!string.IsNullOrEmpty(person.SecondName)) score += 5;
            if (person.Addresses?.Any(a => a.Value != null) == true) score += person.Addresses.Count(a => a.Value != null) * 5;
            if (person.Phones?.Any(p => p.Value != null) == true) score += person.Phones.Count(p => p.Value != null) * 3;
            if (person.Emails?.Any(e => e.Value != null) == true) score += person.Emails.Count(e => e.Value != null) * 2;
            return score;
        }
    }
}