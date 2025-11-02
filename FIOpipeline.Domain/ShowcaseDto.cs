namespace FIOpipeline.Domain
{
    // FIOpipeline.Domain/ShowcaseDto.cs
    public class ShowcaseDto
    {
        public int PersonId { get; set; }
        public string FullName { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string Sex { get; set; }
        public List<string> Addresses { get; set; } = new List<string>();
        public List<string> Phones { get; set; } = new List<string>();
        public List<string> Emails { get; set; } = new List<string>();
        public int MatchScore { get; set; }

        // Добавьте эти свойства для временных данных
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }

    public class ShowcaseSearchRequest
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
    public class SearchResponse
    {
        public string Message { get; set; }
        public int TotalCount { get; set; }
        public List<ShowcaseDto> Results { get; set; }
    }
}