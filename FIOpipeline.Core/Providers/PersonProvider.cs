using FIOpipeline.Core.DataAccess;
using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FIOpipeline.Core.Providers
{
    public class PersonProvider : IPersonProvider
    {
        private readonly AppDbContext _dbContext;
        private readonly IDeduplicationProvider _deduplicationProvider;

        public PersonProvider(AppDbContext dbContext, IDeduplicationProvider deduplicationProvider)
        {
            _dbContext = dbContext;
            _deduplicationProvider = deduplicationProvider;
        }

        public async Task<(bool Success, IEnumerable<string> Errors, int? PersonId)> ValidatePerson(Person person)
        {
            var errors = Validate(person).ToList();

            if (errors.Any())
            {
                return (false, errors, null);
            }

            try
            {
                var duplicateGroups = await _deduplicationProvider.FindPotentialDuplicatesAsync(person);

                if (duplicateGroups.Any())
                {
                    if (await _deduplicationProvider.IsExactDuplicateAsync(person, duplicateGroups.First()))
                    {
                        errors.Add("Пользователь с такими данными уже существует.");
                        return (false, errors, null);
                    }
                    else
                    {
                        var mergeResult = await _deduplicationProvider.MergeWithExistingAsync(person, duplicateGroups);
                        return (true, new List<string> { "Данные объединены с существующей записью." }, mergeResult.PersonId);
                    }
                }

                var personId = await SavePersonAsync(person);
                return (true, new List<string> { "Данные успешно сохранены." }, personId);
            }
            catch (Exception ex)
            {
                return (false, new List<string> { $"Ошибка при сохранении: {ex.Message}" }, null);
            }
        }

        private IEnumerable<string> Validate(Person person)
        {
            var errors = new List<string>();

            // Валидация ФИО
            var nameRegex = new Regex(@"^[А-ЯЁ][а-яё\-]+$");
            if (string.IsNullOrWhiteSpace(person.LastName) || !nameRegex.IsMatch(person.LastName))
                errors.Add("Фамилия некорректна.");

            if (string.IsNullOrWhiteSpace(person.FirstName) || !nameRegex.IsMatch(person.FirstName))
                errors.Add("Имя некорректно.");

            if (string.IsNullOrWhiteSpace(person.SecondName) || !nameRegex.IsMatch(person.SecondName))
                errors.Add("Отчество некорректно.");

            // Проверка даты рождения
            if (person.BirthdayDate == default)
                errors.Add("Дата рождения неправильного формата.");

            // Проверка пола
            if (!Enum.IsDefined(typeof(Sex), person.Sex))
                errors.Add("Пол должен быть 'M' или 'F'.");

            // Проверка адреса
            if (person.Addresses == null || !person.Addresses.Any() ||
                person.Addresses.All(a => string.IsNullOrWhiteSpace(a.Value)))
                errors.Add("Должен быть указан хотя бы один адрес.");
            else
            {
                foreach (var address in person.Addresses)
                {
                    if (string.IsNullOrWhiteSpace(address.Value))
                        errors.Add("Один из адресов пустой.");
                }
            }

            // Проверка телефонов
            var phoneRegex = new Regex(@"^\+7\(\d{3}\)\d{3}-\d{2}-\d{2}$");
            if (person.Phones == null || !person.Phones.Any() ||
                person.Phones.All(p => string.IsNullOrWhiteSpace(p.Value) || !phoneRegex.IsMatch(p.Value)))
                errors.Add("Должен быть указан хотя бы один корректный номер телефона.");
            else
            {
                foreach (var phone in person.Phones)
                {
                    if (!string.IsNullOrWhiteSpace(phone.Value) && !phoneRegex.IsMatch(phone.Value))
                        errors.Add($"Номер телефона '{phone.Value}' некорректен.");
                }
            }

            // Проверка email
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (person.Emails == null || !person.Emails.Any() ||
                person.Emails.All(e => string.IsNullOrWhiteSpace(e.Value) || !emailRegex.IsMatch(e.Value)))
                errors.Add("Должен быть указан хотя бы один корректный email.");
            else
            {
                foreach (var email in person.Emails)
                {
                    if (!string.IsNullOrWhiteSpace(email.Value) && !emailRegex.IsMatch(email.Value))
                        errors.Add($"Email '{email.Value}' некорректен.");
                }
            }

            return errors;
        }

        private async Task<int> SavePersonAsync(Person person)
        {
            var now = DateTime.Now; // Local time
            var maxDate = new DateTime(9999, 12, 31, 23, 59, 59);

            var efPerson = new Entity.Person
            {
                LastName = person.LastName,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                BirthdayDate = person.BirthdayDate, // Для даты рождения Kind обычно не важен
                Sex = person.Sex.ToString(),

                // Используем UTC время
                ValidFrom = now,
                ValidTo = maxDate,
                IsCurrent = true,
                Version = 1,

                Addresses = person.Addresses?.Select(a => new Entity.Address
                {
                    Value = a.Value,
                    ValidFrom = now,
                    ValidTo = maxDate,
                    IsCurrent = true,
                    Version = 1
                }).ToList() ?? new List<Entity.Address>(),

                Phones = person.Phones?.Select(p => new Entity.Phone
                {
                    Value = p.Value,
                    ValidFrom = now,
                    ValidTo = maxDate,
                    IsCurrent = true,
                    Version = 1
                }).ToList() ?? new List<Entity.Phone>(),

                Emails = person.Emails?.Select(e => new Entity.Email
                {
                    Value = e.Value,
                    ValidFrom = now,
                    ValidTo = maxDate,
                    IsCurrent = true,
                    Version = 1
                }).ToList() ?? new List<Entity.Email>()
            };

            _dbContext.Persons.Add(efPerson);
            await _dbContext.SaveChangesAsync();

            return efPerson.Id;
         }

    }

}
