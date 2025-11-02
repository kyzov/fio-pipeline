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
        // Поиск на определенный момент времени
        Task<List<ShowcaseDto>> SearchPersonsAtMomentAsync(ShowcaseSearchRequest request, DateTime moment);

        // Получение истории изменений персоны
        Task<List<PersonHistoryDto>> GetPersonHistoryAsync(int personId);

        // Получение состояния системы на определенный момент
        Task<SystemSnapshotDto> GetSystemSnapshotAsync(DateTime moment);

        // Восстановление данных на определенный момент
        Task<bool> RestoreToMomentAsync(DateTime moment);
    }
}
