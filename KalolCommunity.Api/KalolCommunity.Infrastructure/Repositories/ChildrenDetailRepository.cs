using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class ChildrenDetailRepository(KalolCommunityDbContext context)
        : Repository<ChildrenDetail>(context), IChildrenDetailRepository
    {
    }
}
