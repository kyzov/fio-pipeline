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
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
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
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .FirstOrDefaultAsync(p => p.Id == primaryPersonId);

            if (primaryPerson == null)
            {
                return new MergeResult { Success = false, Message = "Основная персона не найдена" };
            }

            await MergePersonDataAsync(primaryPerson, newPerson);

            await _context.SaveChangesAsync();

            return new MergeResult
            {
                Success = true,
                PersonId = primaryPersonId,
                Message = "Данные успешно объединены с существующей записью"
            };
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

        public async Task<bool> IsExactDuplicateAsync(Domain.Person newPerson, PersonGroup duplicateGroup)
        {
            var existingPerson = await _context.Persons
                .Include(p => p.Addresses)
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .FirstOrDefaultAsync(p => p.Id == duplicateGroup.PrimaryPersonId);

            if (existingPerson == null) return false;

            var allAddressesExist = newPerson.Addresses.All(newAddr =>
                existingPerson.Addresses.Any(existingAddr => existingAddr.Value == newAddr.Value));

            var allPhonesExist = newPerson.Phones.All(newPhone =>
                existingPerson.Phones.Any(existingPhone => existingPhone.Value == newPhone.Value));

            var allEmailsExist = newPerson.Emails.All(newEmail =>
                existingPerson.Emails.Any(existingEmail => existingEmail.Value == newEmail.Value));

            var basicDataMatch = existingPerson.LastName == newPerson.LastName &&
                               existingPerson.FirstName == newPerson.FirstName &&
                               existingPerson.SecondName == newPerson.SecondName &&
                               existingPerson.Sex == newPerson.Sex.ToString() &&
                               existingPerson.BirthdayDate == DateTime.SpecifyKind(newPerson.BirthdayDate, DateTimeKind.Utc);

            return basicDataMatch && allAddressesExist && allPhonesExist && allEmailsExist;
        }

        private async Task MergePersonDataAsync(Core.Entity.Person primaryPerson, Domain.Person newPerson)
        {
            if (newPerson.Addresses != null)
            {
                foreach (var newAddress in newPerson.Addresses)
                {
                    if (!string.IsNullOrWhiteSpace(newAddress.Value) &&
                        !primaryPerson.Addresses.Any(a => a.Value == newAddress.Value))
                    {
                        primaryPerson.Addresses.Add(new Core.Entity.Address { Value = newAddress.Value });
                    }
                }
            }

            if (newPerson.Phones != null)
            {
                foreach (var newPhone in newPerson.Phones)
                {
                    if (!string.IsNullOrWhiteSpace(newPhone.Value) &&
                        !primaryPerson.Phones.Any(p => p.Value == newPhone.Value))
                    {
                        primaryPerson.Phones.Add(new Core.Entity.Phone { Value = newPhone.Value });
                    }
                }
            }

            if (newPerson.Emails != null)
            {
                foreach (var newEmail in newPerson.Emails)
                {
                    if (!string.IsNullOrWhiteSpace(newEmail.Value) &&
                        !primaryPerson.Emails.Any(e => e.Value == newEmail.Value))
                    {
                        primaryPerson.Emails.Add(new Core.Entity.Email { Value = newEmail.Value });
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(primaryPerson.SecondName) && !string.IsNullOrWhiteSpace(newPerson.SecondName))
            {
                primaryPerson.SecondName = newPerson.SecondName;
            }

        }

        private Entity.Person MapToEntityPerson(Domain.Person domainPerson)
        {
            return new Entity.Person
            {
                LastName = domainPerson.LastName,
                FirstName = domainPerson.FirstName,
                SecondName = domainPerson.SecondName,
                Sex = domainPerson.Sex.ToString(),
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