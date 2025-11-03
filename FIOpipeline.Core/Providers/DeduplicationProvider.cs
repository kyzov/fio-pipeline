using FIOpipeline.Core.DataAccess;
using FIOpipeline.Core.Entity;
using FIOpipeline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIOpipeline.Core.Providers
{
    public class DeduplicationProvider : IDeduplicationProvider
    {
        private readonly AppDbContext _context;

        public DeduplicationProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PersonGroup>> FindPotentialDuplicatesAsync(Domain.Person newPerson)
        {
            var existingPersons = await _context.Persons
                .Where(p => p.IsCurrent)
                .Include(p => p.Addresses.Where(a => a.IsCurrent))
                .Include(p => p.Phones.Where(ph => ph.IsCurrent))
                .Include(p => p.Emails.Where(e => e.IsCurrent))
                .ToListAsync();

            var potentialDuplicates = new List<PersonGroup>();

            foreach (var existingPerson in existingPersons)
            {
                if (ArePotentialDuplicates(existingPerson, MapToEntityPerson(newPerson)))
                {
                    var group = new PersonGroup { PrimaryPersonId = existingPerson.Id };
                    group.Persons.Add(MapToDomainPerson(existingPerson));
                    potentialDuplicates.Add(group);
                }
            }

            return potentialDuplicates;
        }

        public async Task<MergeResult> MergeWithExistingAsync(Domain.Person newPerson, List<PersonGroup> duplicateGroups)
        {
            if (duplicateGroups == null || !duplicateGroups.Any())
            {
                return new MergeResult { Success = false, Message = "Нет дубликатов для объединения" };
            }

            var primaryGroup = duplicateGroups.First();
            var primaryPersonId = primaryGroup.PrimaryPersonId;

            var primaryPerson = await _context.Persons
                .AsNoTracking()
                .Where(p => p.Id == primaryPersonId && p.IsCurrent)
                .Include(p => p.Addresses.Where(a => a.IsCurrent))
                .Include(p => p.Phones.Where(ph => ph.IsCurrent))
                .Include(p => p.Emails.Where(e => e.IsCurrent))
                .FirstOrDefaultAsync();

            if (primaryPerson == null)
            {
                return new MergeResult { Success = false, Message = "Основная персона не найдена" };
            }

            await MergePersonDataAsync(primaryPerson, newPerson);

            return new MergeResult
            {
                Success = true,
                PersonId = primaryPersonId,
                Message = "Данные успешно объединены с существующей записью"
            };
        }

        private async Task MergePersonDataAsync(Person existingPerson, Domain.Person newPerson)
        {
            var now = DateTime.Now;

            var trackedPerson = await _context.Persons
                .Where(p => p.Id == existingPerson.Id && p.IsCurrent)
                .Include(p => p.Addresses.Where(a => a.IsCurrent))
                .Include(p => p.Phones.Where(ph => ph.IsCurrent))
                .Include(p => p.Emails.Where(e => e.IsCurrent))
                .FirstOrDefaultAsync();

            if (trackedPerson == null) return;

            if (newPerson.Addresses != null)
            {
                foreach (var newAddress in newPerson.Addresses)
                {
                    if (!string.IsNullOrWhiteSpace(newAddress.Value) &&
                        !trackedPerson.Addresses.Any(a => a.Value == newAddress.Value))
                    {
                        trackedPerson.Addresses.Add(new Address
                        {
                            Value = newAddress.Value,
                            PersonId = trackedPerson.Id,
                            ValidFrom = now,
                            ValidTo = new DateTime(9999, 12, 31, 23, 59, 59),
                            IsCurrent = true,
                            Version = 1
                        });
                    }
                }
            }

            if (newPerson.Phones != null)
            {
                foreach (var newPhone in newPerson.Phones)
                {
                    if (!string.IsNullOrWhiteSpace(newPhone.Value) &&
                        !trackedPerson.Phones.Any(p => p.Value == newPhone.Value))
                    {
                        trackedPerson.Phones.Add(new Phone
                        {
                            Value = newPhone.Value,
                            PersonId = trackedPerson.Id,
                            ValidFrom = now,
                            ValidTo = new DateTime(9999, 12, 31, 23, 59, 59),
                            IsCurrent = true,
                            Version = 1
                        });
                    }
                }
            }

            if (newPerson.Emails != null)
            {
                foreach (var newEmail in newPerson.Emails)
                {
                    if (!string.IsNullOrWhiteSpace(newEmail.Value) &&
                        !trackedPerson.Emails.Any(e => e.Value == newEmail.Value))
                    {
                        trackedPerson.Emails.Add(new Email
                        {
                            Value = newEmail.Value,
                            PersonId = trackedPerson.Id,
                            ValidFrom = now,
                            ValidTo = new DateTime(9999, 12, 31, 23, 59, 59),
                            IsCurrent = true,
                            Version = 1
                        });
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(trackedPerson.SecondName) && !string.IsNullOrWhiteSpace(newPerson.SecondName))
            {
                trackedPerson.SecondName = newPerson.SecondName;
            }

            await _context.SaveChangesAsync();
        }

        private string GetEntityValue<T>(T entity) where T : BaseTemporalEntity
        {
            return entity switch
            {
                Address address => address.Value,
                Phone phone => phone.Value,
                Email email => email.Value,
                _ => string.Empty
            };
        }

        public async Task<bool> IsExactDuplicateAsync(Domain.Person newPerson, PersonGroup duplicateGroup)
        {
            var existingPerson = await _context.Persons
                .Where(p => p.Id == duplicateGroup.PrimaryPersonId && p.IsCurrent)
                .Include(p => p.Addresses.Where(a => a.IsCurrent))
                .Include(p => p.Phones.Where(ph => ph.IsCurrent))
                .Include(p => p.Emails.Where(e => e.IsCurrent))
                .FirstOrDefaultAsync();

            if (existingPerson == null) return false;

            return existingPerson.LastName == newPerson.LastName &&
               existingPerson.FirstName == newPerson.FirstName &&
               existingPerson.SecondName == newPerson.SecondName &&
               existingPerson.Sex == newPerson.Sex.ToString() &&
               existingPerson.BirthdayDate.Date == newPerson.BirthdayDate.Date &&
               CollectionsMatch<Address, Domain.Address>(existingPerson.Addresses, newPerson.Addresses) &&
               CollectionsMatch<Phone, Domain.Phone>(existingPerson.Phones, newPerson.Phones) &&
               CollectionsMatch<Email, Domain.Email>(existingPerson.Emails, newPerson.Emails);
        }

        private bool CollectionsMatch<TEntity, TDomain>(
            ICollection<TEntity> existing,
            List<TDomain> newOnes)
            where TEntity : BaseTemporalEntity
            where TDomain : class
        {
            if (existing == null && newOnes == null) return true;
            if (existing == null || newOnes == null) return false;

            var existingValues = existing.Select(GetEntityValue).OrderBy(v => v).ToList();
            var newValues = newOnes.Select(n =>
            {
                var property = n.GetType().GetProperty("Value");
                return property?.GetValue(n)?.ToString();
            }).OrderBy(v => v).ToList();

            return existingValues.SequenceEqual(newValues);
        }

        private bool ArePotentialDuplicates(Person person1, Person person2)
        {
            if (person1.Sex == "M" && person2.Sex == "M" &&
                person1.LastName != person2.LastName)
                return false;

            if (person1.FirstName != person2.FirstName)
                return false;

            if (!string.IsNullOrEmpty(person1.SecondName) &&
                !string.IsNullOrEmpty(person2.SecondName) &&
                person1.SecondName != person2.SecondName)
                return false;

            if (person1.Sex != person2.Sex)
                return false;

            var hasAddresses1 = person1.Addresses?.Any() == true;
            var hasAddresses2 = person2.Addresses?.Any() == true;
            var addressMatch = !hasAddresses1 && !hasAddresses2 ||
                              (hasAddresses1 && hasAddresses2 &&
                               person1.Addresses.Any(a1 => person2.Addresses.Any(a2 => a1.Value == a2.Value)));

            var hasPhones1 = person1.Phones?.Any() == true;
            var hasPhones2 = person2.Phones?.Any() == true;
            var phoneMatch = !hasPhones1 && !hasPhones2 ||
                            (hasPhones1 && hasPhones2 &&
                             person1.Phones.Any(p1 => person2.Phones.Any(p2 => p1.Value == p2.Value)));

            var hasEmails1 = person1.Emails?.Any() == true;
            var hasEmails2 = person2.Emails?.Any() == true;
            var emailMatch = !hasEmails1 && !hasEmails2 ||
                            (hasEmails1 && hasEmails2 &&
                             person1.Emails.Any(e1 => person2.Emails.Any(e2 => e1.Value == e2.Value)));

            return addressMatch && phoneMatch && emailMatch;
        }

        private Entity.Person MapToEntityPerson(Domain.Person domainPerson)
        {
            return new Entity.Person
            {
                LastName = domainPerson.LastName,
                FirstName = domainPerson.FirstName,
                SecondName = domainPerson.SecondName,
                Sex = domainPerson.Sex.ToString(),
                BirthdayDate = domainPerson.BirthdayDate,
                Addresses = domainPerson.Addresses?.Select(a => new Entity.Address { Value = a.Value }).ToList(),
                Phones = domainPerson.Phones?.Select(p => new Entity.Phone { Value = p.Value }).ToList(),
                Emails = domainPerson.Emails?.Select(e => new Entity.Email { Value = e.Value }).ToList()
            };
        }

        private Domain.Person MapToDomainPerson(Person entityPerson)
        {
            return new Domain.Person
            {
                LastName = entityPerson.LastName,
                FirstName = entityPerson.FirstName,
                SecondName = entityPerson.SecondName,
                BirthdayDate = entityPerson.BirthdayDate,
                Sex = Enum.Parse<FIOpipeline.Domain.Sex>(entityPerson.Sex),
                Addresses = entityPerson.Addresses?.Select(a => new Domain.Address { Value = a.Value }).ToList(),
                Phones = entityPerson.Phones?.Select(p => new Domain.Phone { Value = p.Value }).ToList(),
                Emails = entityPerson.Emails?.Select(e => new Domain.Email { Value = e.Value }).ToList()
            };
        }
    }
}