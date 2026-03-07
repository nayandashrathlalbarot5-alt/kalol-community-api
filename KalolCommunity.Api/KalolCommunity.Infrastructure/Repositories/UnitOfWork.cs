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
        public ICountryRepository Countries { get; }
        public IStateRepository States { get; }
        public ICommunityDetailRepository CommunityDetails { get; }
        public IChildrenDetailRepository ChildrenDetails { get; }

        public UnitOfWork(KalolCommunityDbContext dbContext)
        {
            _dbContext = dbContext;
            Users = new UserRepository(_dbContext);
            RefreshTokens = new RefreshTokenRepository(_dbContext);
            Countries = new CountryRepository(_dbContext);
            States = new StateRepository(_dbContext);
            CommunityDetails = new CommunityDetailRepository(_dbContext);
            ChildrenDetails = new ChildrenDetailRepository(_dbContext);
        }

        public Task<int> SaveChangesAsync()
            => _dbContext.SaveChangesAsync();
    }
}
