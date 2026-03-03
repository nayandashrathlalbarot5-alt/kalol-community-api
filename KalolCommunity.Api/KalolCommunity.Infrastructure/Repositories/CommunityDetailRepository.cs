using KalolCommunity.Application.Interfaces;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class CommunityDetailRepository(KalolCommunityDbContext context)
        : Repository<CommunityDetail>(context), ICommunityDetailRepository
    {
    }
}