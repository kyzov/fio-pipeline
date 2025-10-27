
namespace FIOpipeline.Domain.Interfaces
{
    public interface IShowcaseProvider
    {
        Task<List<ShowcaseDto>> SearchPersonsAsync(ShowcaseSearchRequest request);
    }
}