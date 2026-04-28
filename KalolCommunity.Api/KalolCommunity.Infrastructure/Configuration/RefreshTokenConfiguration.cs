using KalolCommunity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Infrastructure.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("TblRefreshToken");
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TokenHash).IsRequired();
            builder.Property(rt => rt.ExpiresAt).IsRequired();

            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens) // uncomment collection on User
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
