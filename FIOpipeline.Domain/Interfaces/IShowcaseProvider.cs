// FIOpipeline.Domain/Interfaces/IShowcaseProvider.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FIOpipeline.Domain.Interfaces
{
    public interface IShowcaseProvider
    {
        Task<List<ShowcaseDto>> SearchPersonsAsync(ShowcaseSearchRequest request);
    }
}