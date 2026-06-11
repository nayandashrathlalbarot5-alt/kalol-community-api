using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class StateRepository(KalolCommunityDbContext context) : Repository<State>(context), IStateRepository
    {
        private readonly KalolCommunityDbContext _context = context;

        public async Task<IEnumerable<State>> GetAllStatesAsync()
        {
            return await _context.States
                .OrderBy(s => s.StateName)
                .ToListAsync();
        }

        public async Task<IEnumerable<State>> GetStatesByCountryIdAsync(int countryId)
        {
            return await _context.States
                .Where(s => s.CountryId == countryId)
                .OrderBy(s => s.StateName)
                .ToListAsync();
        }
    }
}