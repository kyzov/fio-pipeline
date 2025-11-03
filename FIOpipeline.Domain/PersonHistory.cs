using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Domain
{
    public class PersonHistoryDto
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public PersonSnapshotDto PersonData { get; set; }
        public List<FieldChangeDto> FieldChanges { get; set; }
    }

    public class PersonSnapshotDto
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string Sex { get; set; }
        public List<string> Addresses { get; set; }
        public List<string> Phones { get; set; }
        public List<string> Emails { get; set; }
    }

    public class FieldChangeDto
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class SystemSnapshotDto
    {
        public DateTime SnapshotMoment { get; set; }
        public int TotalPersons { get; set; }
        public List<ChangeStatisticDto> RecentChanges { get; set; }
    }

    public class ChangeStatisticDto
    {
        public DateTime Date { get; set; }
        public int ChangesCount { get; set; }
    }
}
