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

        public PersonProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ValidatePerson(Person person)
        {
            var errors = Validate(person).ToList();

            if (errors.Any())
            {
                return (false, errors);
            }

            try
            {
                await SavePersonAsync(person);
                return (true, Enumerable.Empty<string>());
            }
            catch (Exception ex)
            {
                return (false, new List<string> { $"Ошибка при сохранении: {ex.Message}" });
            }
        }

        public IEnumerable<string> Validate(Person person)
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

        public async Task SavePersonAsync(Person person)
        {
            var efPerson = new Entity.Person
            {
                LastName = person.LastName,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                BirthdayDate = DateTime.SpecifyKind(person.BirthdayDate, DateTimeKind.Utc),
                Sex = person.Sex.ToString(),
                Addresses = person.Addresses.Select(a => new Entity.Address
                {
                    Value = a.Value
                }).ToList(),

                // Коллекции телефонов
                Phones = person.Phones.Select(p => new Entity.Phone
                {
                    Value = p.Value
                }).ToList(),

                // Коллекции email
                Emails = person.Emails.Select(e => new Entity.Email
                {
                    Value = e.Value
                }).ToList()
            };

            _dbContext.Persons.Add(efPerson);
            await _dbContext.SaveChangesAsync();
        }
 
    }

}
