using KalolCommunity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {   
    }
}
