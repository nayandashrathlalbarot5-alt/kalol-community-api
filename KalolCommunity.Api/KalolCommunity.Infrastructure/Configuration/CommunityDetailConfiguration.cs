using KalolCommunity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KalolCommunity.Infrastructure.Configuration
{
    public class CommunityDetailConfiguration : IEntityTypeConfiguration<CommunityDetail>
    {
        public void Configure(EntityTypeBuilder<CommunityDetail> builder)
        {
            builder.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.State)
                .WithMany()
                .HasForeignKey(x => x.StateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<CommunityDetail>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.PrimatyContactNumber).IsUnique();
            builder.HasIndex(x => x.AlternateContactNumber).IsUnique();
            builder.HasIndex(x => x.UserId).IsUnique();
        }
    }
}