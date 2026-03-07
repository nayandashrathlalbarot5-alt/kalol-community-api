using KalolCommunity.Application.Interfaces;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class CommunityDetailRepository(KalolCommunityDbContext context)
        : Repository<CommunityDetail>(context), ICommunityDetailRepository
    {
        /// <summary>
        /// Get community detail by user ID with children details eagerly loaded
        /// </summary>
        public async Task<CommunityDetail?> GetByUserIdWithChildrenAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .Include(c => c.ChildrenDetails)
                .FirstOrDefaultAsync();
        }
    }
}