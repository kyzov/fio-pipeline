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
        public Sex Sex { get; set; }
        public Address Address { get; set; }
        public Phone Phone { get; set; }
        public Email Email { get; set; }
    }
}
