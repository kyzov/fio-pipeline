using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIOpipeline.Domain
{
    public class PersonHistoryDto
    {
        public int PersonId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int Version { get; set; }
        public bool IsCurrent { get; set; }
        public List<string> Addresses { get; set; }
        public List<string> Phones { get; set; }
        public List<string> Emails { get; set; }
    }

    // FIOpipeline.Domain/SystemSnapshotDto.cs
    public class SystemSnapshotDto
    {
        public DateTime SnapshotMoment { get; set; }
        public int TotalPersons { get; set; }
        public List<ChangeStatisticDto> RecentChanges { get; set; }
    }

    // FIOpipeline.Domain/ChangeStatisticDto.cs
    public class ChangeStatisticDto
    {
        public DateTime Date { get; set; }
        public int ChangesCount { get; set; }
    }
}
