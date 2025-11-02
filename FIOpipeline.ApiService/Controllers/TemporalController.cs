using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FIOpipeline.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemporalController : ControllerBase
    {
        private readonly ITemporalDataService _temporalDataService;

        public TemporalController(ITemporalDataService temporalDataService)
        {
            _temporalDataService = temporalDataService;
        }

        [HttpGet("search-at-moment")]
        public async Task<IActionResult> SearchAtMoment(
            [FromQuery] ShowcaseSearchRequest request,
            [FromQuery] DateTime moment)
        {
            try
            {
                var results = await _temporalDataService.SearchPersonsAtMomentAsync(request, moment);

                return Ok(new
                {
                    Message = $"Поиск выполнен на момент {moment:yyyy-MM-dd HH:mm:ss}",
                    SnapshotMoment = moment,
                    TotalCount = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Ошибка при выполнении временного поиска",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("person/{personId}/history")]
        public async Task<IActionResult> GetPersonHistory(int personId)
        {
            try
            {
                var history = await _temporalDataService.GetPersonHistoryAsync(personId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("system-snapshot")]
        public async Task<IActionResult> GetSystemSnapshot([FromQuery] DateTime? moment = null)
        {
            try
            {
                var snapshotMoment = moment ?? DateTime.UtcNow;
                var snapshot = await _temporalDataService.GetSystemSnapshotAsync(snapshotMoment);
                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
