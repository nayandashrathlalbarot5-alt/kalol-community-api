using KalolCommunity.Application.Common;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Contracts.DTO;
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

        public MasterDataService(ICountryRepository countryRepository, IStateRepository stateRepository)
        {
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
        }

        public async Task<ApiResponse<IEnumerable<CountryDTO>>> GetAllCountriesAsync()
        {
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
                Message = "Countries retrieved successfully",
                StatusCode = (int)HttpStatusCode.OK,
                Data = countryDTOs
            };
        }

        public async Task<ApiResponse<IEnumerable<StateDTO>>> GetAllStatesAsync()
        {
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
                Message = "States retrieved successfully",
                StatusCode = (int)HttpStatusCode.OK,
                Data = stateDTOs
            };
        }

        public async Task<ApiResponse<IEnumerable<StateDTO>>> GetStatesByCountryIdAsync(int countryId)
        {
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
                Message = $"States retrieved successfully for country ID {countryId}",
                StatusCode = (int)HttpStatusCode.OK,
                Data = stateDTOs
            };
        }
    }
}