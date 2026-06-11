using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;
using KalolCommunity.Application.Interfaces.Repositories;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class RefreshTokenRepository(KalolCommunityDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
    {
    }
}
