using System.Collections.Generic;
using System.Threading.Tasks;

namespace FIOpipeline.Domain.Interfaces
{
    public interface IDeduplicationProvider
    {
        Task<List<PersonGroup>> FindPotentialDuplicatesAsync(Person newPerson);
        Task<MergeResult> MergeWithExistingAsync(Domain.Person newPerson, List<PersonGroup> duplicateGroups);
        Task<bool> IsExactDuplicateAsync(Domain.Person newPerson, PersonGroup duplicateGroup);
    }

    public class MergeResult
    {
        public bool Success { get; set; }
        public int PersonId { get; set; }
        public string Message { get; set; }
    }

    public class PersonGroup
    {
        public int PrimaryPersonId { get; set; }
        public List<Person> Persons { get; set; } = new List<Person>(); // Используем Entity.Person
        public int MatchScore { get; set; }
    }
}
