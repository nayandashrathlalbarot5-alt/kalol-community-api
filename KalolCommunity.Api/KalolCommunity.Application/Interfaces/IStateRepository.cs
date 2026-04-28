using KalolCommunity.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IStateRepository : IRepository<State>
    {
        Task<IEnumerable<State>> GetAllStatesAsync();
        Task<IEnumerable<State>> GetStatesByCountryIdAsync(int countryId);
    }
}