using KalolCommunity.Application.Common;
using KalolCommunity.Contracts.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IMasterDataService
    {
        Task<ApiResponse<IEnumerable<CountryDTO>>> GetAllCountriesAsync();
        Task<ApiResponse<IEnumerable<StateDTO>>> GetAllStatesAsync();
        Task<ApiResponse<IEnumerable<StateDTO>>> GetStatesByCountryIdAsync(int countryId);
    }
}