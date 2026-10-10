using LinkAnalytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders ;

namespace LinkAnalytics.Infrastructure.Persistence.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.Property(p => p.Title).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Bio).HasMaxLength(500);

        builder.HasIndex(p => p.Slug).IsUnique();

        builder.HasOne(p => p.User)
        .WithMany(u => u.Profiles)
        .HasForeignKey(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}