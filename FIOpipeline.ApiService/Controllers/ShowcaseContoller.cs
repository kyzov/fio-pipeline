using FIOpipeline.Domain;
using FIOpipeline.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FIOpipeline.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowcaseController : ControllerBase
    {
        private readonly IShowcaseProvider _showcaseProvider;

        public ShowcaseController(IShowcaseProvider showcaseProvider)
        {
            _showcaseProvider = showcaseProvider;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ShowcaseSearchRequest request)
        {
            if (IsRequestEmpty(request))
            {
                return BadRequest(new
                {
                    Message = "Хотя бы один параметр поиска должен быть указан"
                });
            }

            try
            {
                var results = await _showcaseProvider.SearchPersonsAsync(request);

                return Ok(new
                {
                    Message = "Поиск выполнен успешно",
                    TotalCount = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Ошибка при выполнении поиска",
                    Error = ex.Message
                });
            }
        }

        private bool IsRequestEmpty(ShowcaseSearchRequest request)
        {
            return string.IsNullOrEmpty(request.LastName) &&
                   string.IsNullOrEmpty(request.FirstName) &&
                   string.IsNullOrEmpty(request.SecondName) &&
                   string.IsNullOrEmpty(request.Address) &&
                   string.IsNullOrEmpty(request.Phone) &&
                   string.IsNullOrEmpty(request.Email);
        }
    }
}