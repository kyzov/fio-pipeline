using System.Net;
using System.Numerics;
using FIOpipeline.Domain;


namespace FIOpipeline.ApiService.Models
{
    public class PersonDto
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string Sex { get; set; }

        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        public List<PhoneDto> Phones { get; set; } = new List<PhoneDto>();
        public List<EmailDto> Emails { get; set; } = new List<EmailDto>();

        // Добавляем временные поля (опциональные)
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    // Обновленные DTO для связанных сущностей
    public class AddressDto
    {
        public string Value { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class PhoneDto
    {
        public string Value { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class EmailDto
    {
        public string Value { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
