using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Core.Entity
{
    public abstract class BaseTemporalEntity
    {
        public int Id { get; set; }
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime ValidTo { get; set; } = new DateTime(9999, 12, 31, 23, 59, 59);
        public bool IsCurrent { get; set; } = true;
        public int Version { get; set; } = 1;
    }

    public class Address : BaseTemporalEntity
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Phone : BaseTemporalEntity
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Email : BaseTemporalEntity
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
    }

    public class Person : BaseTemporalEntity
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
