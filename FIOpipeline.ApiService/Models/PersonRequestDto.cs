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

        public class AddressDto
        {
            public string Value { get; set; }
        }

        public class PhoneDto
        {
            public string Value { get; set; }
        }

        public class EmailDto
        {
            public string Value { get; set; }
        }
    }
}
