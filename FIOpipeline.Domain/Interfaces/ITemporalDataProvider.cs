using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FIOpipeline.Domain;

namespace FIOpipeline.Domain.Interfaces
{
    public interface ITemporalDataService
    {
        Task<List<ShowcaseDto>> SearchPersonsAtMomentAsync(ShowcaseSearchRequest request, DateTime moment);

        Task<List<PersonHistoryDto>> GetPersonHistoryAsync(int personId);

        Task<SystemSnapshotDto> GetSystemSnapshotAsync(DateTime moment);
    }
}
