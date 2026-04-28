using KalolCommunity.Application.Interfaces;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class CountryRepository(KalolCommunityDbContext context) : Repository<Country>(context), ICountryRepository
    {
        private readonly KalolCommunityDbContext _context = context;        

        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            return await _context.Countries
                .OrderBy(c => c.CountryName)
                .ToListAsync();
        }
    }
}