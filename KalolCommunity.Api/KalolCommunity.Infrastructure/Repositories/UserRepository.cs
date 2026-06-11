using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Application.Interfaces.Repositories;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class UserRepository(KalolCommunityDbContext context) : Repository<User>(context), IUserRepository
    {
    }
}
