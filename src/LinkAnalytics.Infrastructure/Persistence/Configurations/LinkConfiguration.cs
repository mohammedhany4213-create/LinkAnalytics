using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkAnalytics.Infrastructure.Persistence.Configuration;

public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.Property(l => l.Title).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Url).IsRequired().HasMaxLength(2048);

        builder.HasOne(l => l.Profile)
        .WithMany(p => p.Links)
        .HasForeignKey(l => l.ProfileId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}