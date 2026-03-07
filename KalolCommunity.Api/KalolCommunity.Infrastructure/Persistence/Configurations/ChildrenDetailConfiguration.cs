using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KalolCommunity.Domain.Entities;

namespace KalolCommunity.Infrastructure.Persistence.Configurations
{
    public class ChildrenDetailConfiguration : IEntityTypeConfiguration<ChildrenDetail>
    {
        public void Configure(EntityTypeBuilder<ChildrenDetail> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}