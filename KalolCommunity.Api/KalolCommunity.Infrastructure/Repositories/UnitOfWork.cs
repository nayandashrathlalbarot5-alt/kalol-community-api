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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KalolCommunityDbContext _dbContext;
        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        public UnitOfWork(KalolCommunityDbContext dbContext)
        {
            _dbContext = dbContext;
            Users = new UserRepository(_dbContext);
            RefreshTokens = new RefreshTokenRepository(_dbContext);
        }

        public Task<int> SaveChangesAsync()
            => _dbContext.SaveChangesAsync();
    }
}
