using KalolCommunity.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KalolCommunity.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;

        public MasterDataController(IMasterDataService masterDataService)
        {
            _masterDataService = masterDataService;
        }

        /// <summary>
        /// Get all countries for dropdown
        /// </summary>
        [HttpGet("countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            var result = await _masterDataService.GetAllCountriesAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get all states for dropdown
        /// </summary>
        [HttpGet("states")]
        public async Task<IActionResult> GetAllStates()
        {
            var result = await _masterDataService.GetAllStatesAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get states by country ID for dependent dropdown
        /// </summary>
        [HttpGet("states/country/{countryId}")]
        public async Task<IActionResult> GetStatesByCountryId(int countryId)
        {
            var result = await _masterDataService.GetStatesByCountryIdAsync(countryId);
            return StatusCode(result.StatusCode, result);
        }
    }
}