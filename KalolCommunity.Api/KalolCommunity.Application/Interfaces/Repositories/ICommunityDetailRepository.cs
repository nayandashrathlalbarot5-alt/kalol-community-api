using KalolCommunity.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces.Repositories
{
    public interface ICommunityDetailRepository : IRepository<CommunityDetail>
    {
        /// <summary>
        /// Get community detail by user ID with children details eagerly loaded
        /// </summary>
        Task<CommunityDetail?> GetByUserIdWithChildrenAsync(Guid userId);
    }
}