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
        public IEnumerable<string> Validate(Person person)
        {
            var errors = new List<string>();

            // Валидация ФИО
            var nameRegex = new Regex(@"^[А-ЯЁ][а-яё\-]{1,}$");
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
            var emailRegex = new Regex(@"^[a-zA-Z0-9]{3,}@[a-zA-Z0-9]{3,}$");
            if (string.IsNullOrWhiteSpace(person.Email?.Value) || !emailRegex.IsMatch(person.Email.Value))
                errors.Add("Email некорректен.");

            return errors;
        }
    }

}
