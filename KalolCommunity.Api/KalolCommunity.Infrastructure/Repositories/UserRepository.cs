using KalolCommunity.Domain.Entities;
using KalolCommunity.Application.Interfaces;
using KalolCommunity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(KalolCommunityDbContext context)
            : base(context)
        {
        }        
    }
}
