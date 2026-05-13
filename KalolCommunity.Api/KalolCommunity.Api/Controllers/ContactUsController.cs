using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace KalolCommunity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactUsController : ControllerBase
    {
        private readonly IContactUsService _contactUsService;

        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ContactUsRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(new ApiResponse<ContactUsResponseDTO>
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Data = null
                });
            }

            var result = await _contactUsService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
