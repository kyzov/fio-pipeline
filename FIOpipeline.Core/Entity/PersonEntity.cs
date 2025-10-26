using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Core.Entity
{
    public class Address
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Phone
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Email
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string Sex { get; set; }
        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<Phone> Phones { get; set; } = new List<Phone>();
        public List<Email> Emails { get; set; } = new List<Email>();
    }

}
