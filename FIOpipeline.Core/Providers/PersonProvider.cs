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
            if (string.IsNullOrWhiteSpace(person.Address?.Value))
                errors.Add("Адрес пустой.");

            // Телефон
            var phoneRegex = new Regex(@"^\+7\(\d{3}\)\d{3}-\d{2}-\d{2}$");
            if (string.IsNullOrWhiteSpace(person.Phone?.Value) || !phoneRegex.IsMatch(person.Phone.Value))
                errors.Add("Номер телефона некорректен.");

            // Email:
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (string.IsNullOrWhiteSpace(person.Email?.Value) || !emailRegex.IsMatch(person.Email.Value))
                errors.Add("Email некорректен.");

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
                Address = new Entity.Address { Value = person.Address.Value },
                Phone = new Entity.Phone { Value = person.Phone.Value },
                Email = new Entity.Email { Value = person.Email.Value }
            };

            _dbContext.Persons.Add(efPerson);
            await _dbContext.SaveChangesAsync();
        }



    }

}
