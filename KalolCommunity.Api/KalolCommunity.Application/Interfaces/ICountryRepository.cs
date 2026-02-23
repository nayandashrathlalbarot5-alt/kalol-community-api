using KalolCommunity.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface ICountryRepository : IRepository<Country>
    {
        Task<IEnumerable<Country>> GetAllCountriesAsync();
    }
}