using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkAnalytics.Infrastructure.Persistence.Configurations;

public class LinkCountryClickConfiguration : IEntityTypeConfiguration<LinkCountryClick>
{
    public void Configure(EntityTypeBuilder<LinkCountryClick> builder)
    {
        builder.Property(c => c.Country).IsRequired().HasMaxLength(2).IsFixedLength();

        builder.HasIndex(c => new { c.LinkId, c.Country }).IsUnique();

        builder.HasOne(c => c.Link)
        .WithMany(l => l.CountryClicks)
        .HasForeignKey(c => c.LinkId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}