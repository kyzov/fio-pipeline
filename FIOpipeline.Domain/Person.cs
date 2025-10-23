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

        public required Address Address { get; set; }

        public required Phone Phone { get; set; }

        public required Email Email { get; set; }
    }
}
