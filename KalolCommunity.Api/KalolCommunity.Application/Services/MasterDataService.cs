using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Application.Interfaces.Infrastructure;
using KalolCommunity.Application.Interfaces.Services;


namespace KalolCommunity.Application.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ILogger<MasterDataService> _logger;
        private readonly ICachingService _cacheService;

        public MasterDataService(
            ICountryRepository countryRepository,
            IStateRepository stateRepository,
            ILogger<MasterDataService> logger,
            ICachingService cacheService)
        {
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<ApiResponse<IEnumerable<CountryDTO>>> GetAllCountriesAsync()
        {
            _logger.LogInformation("Fetching all countries");

            const string cacheKey = "countries";

            var countryDTOs = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("Countries not in cache. Fetching from DB");

                    var countries = await _countryRepository.GetAllCountriesAsync();

                    return countries.Select(c => new CountryDTO
                    {
                        CountryId = c.CountryId,
                        CountryName = c.CountryName,
                        CountryCode = c.CountryCode
                    }).ToList();
                },
                    TimeSpan.FromHours(12)
                );

            return new ApiResponse<IEnumerable<CountryDTO>>
            {
                Success = true,
                Message = ResponseMessages.CountriesRetrievedSuccessfully,
                StatusCode = (int)HttpStatusCode.OK,
                Data = countryDTOs
            };
        }

        public async Task<ApiResponse<IEnumerable<StateDTO>>> GetAllStatesAsync()
        {
            _logger.LogInformation("Fetching all states");

            var states = await _stateRepository.GetAllStatesAsync();

            var stateDTOs = states.Select(s => new StateDTO
            {
                StateId = s.StateId,
                StateName = s.StateName,
                StateCode = s.StateCode
            }).ToList();

            return new ApiResponse<IEnumerable<StateDTO>>
            {
                Success = true,
                Message = ResponseMessages.StatesRetrievedSuccessfully,
                StatusCode = (int)HttpStatusCode.OK,
                Data = stateDTOs
            };
        }

        public async Task<ApiResponse<IEnumerable<StateDTO>>> GetStatesByCountryIdAsync(int countryId)
        {
            _logger.LogInformation("Fetching states for CountryId: {CountryId}", countryId);

            string cacheKey = $"states_country_{countryId}";

            var stateDTOs = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () =>
                {
                    _logger.LogInformation("States for CountryId {CountryId} not in cache. Fetching from DB", countryId);
                    var states = await _stateRepository.GetStatesByCountryIdAsync(countryId);

                    return states.Select(s => new StateDTO
                    {
                        StateId = s.StateId,
                        StateName = s.StateName,
                        StateCode = s.StateCode
                    }).ToList();
                },
                    TimeSpan.FromHours(12)
                );

            return new ApiResponse<IEnumerable<StateDTO>>
            {
                Success = true,
                Message = ResponseMessages.StatesRetrievedSuccessfully,
                StatusCode = (int)HttpStatusCode.OK,
                Data = stateDTOs
            };
        }
    }
}