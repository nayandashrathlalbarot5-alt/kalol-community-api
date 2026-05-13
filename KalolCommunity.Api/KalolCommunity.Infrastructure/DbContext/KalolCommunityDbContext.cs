using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KalolCommunity.Domain.Entities;

namespace KalolCommunity.Infrastructure.Persistence
{
    public class KalolCommunityDbContext : DbContext
    {
        public KalolCommunityDbContext(DbContextOptions<KalolCommunityDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<CommunityDetail> CommunityDetails { get; set; }
        public DbSet<ChildrenDetail> ChildrenDetails { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(KalolCommunityDbContext).Assembly
            );
        }
    }

}
