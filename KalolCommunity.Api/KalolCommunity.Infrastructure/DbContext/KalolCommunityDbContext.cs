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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(KalolCommunityDbContext).Assembly
            );
        }
    }

}
