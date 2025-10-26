using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Domain
{
    public enum Sex
    {
        M,
        F
    }

    public class Address
    {
        public required string Value { get; set; }
    }

    public class Phone
    {
        public required string Value { get; set; }
    }

    public class Email
    {
        public required string Value { get; set; }
    }

    public class Person
    {
        public required string LastName { get; set; }

        public required string FirstName { get; set; }

        public required string SecondName { get; set; }

        public required DateTime BirthdayDate { get; set; }

        public required Sex Sex { get; set; }

        public required List<Address> Addresses { get; set; } = new List<Address>();

        public required List<Phone> Phones { get; set; } = new List<Phone>();

        public required List<Email> Emails { get; set; } = new List<Email>();
    }
}
