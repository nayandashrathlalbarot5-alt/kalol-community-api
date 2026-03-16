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
    public class ChildrenDetailsConfiguration : IEntityTypeConfiguration<ChildrenDetail>
    {
        public void Configure(EntityTypeBuilder<ChildrenDetail> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.ChildrenDetails)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreign Key: CommunityDetail (parent relationship)
            builder.HasOne(x => x.CommunityDetail)
                .WithMany(x => x.ChildrenDetails)
                .HasForeignKey(x => x.CommunityDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // non-unique index (optional, for query performance)
            builder.HasIndex(x => x.UserId);
        }
    }
}
