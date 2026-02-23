using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Infrastructure.Persistence;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class RefreshTokenRepository(KalolCommunityDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
    {
    }
}
