using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ILogger<MasterDataService> _logger;

        public MasterDataService(
            ICountryRepository countryRepository,
            IStateRepository stateRepository,
            ILogger<MasterDataService> logger)
        {
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<CountryDTO>>> GetAllCountriesAsync()
        {
            _logger.LogInformation("Fetching all countries");

            var countries = await _countryRepository.GetAllCountriesAsync();
            
            var countryDTOs = countries.Select(c => new CountryDTO
            {
                CountryId = c.CountryId,
                CountryName = c.CountryName,
                CountryCode = c.CountryCode
            }).ToList();

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

            var states = await _stateRepository.GetStatesByCountryIdAsync(countryId);
            
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
    }
}