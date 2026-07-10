using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Position).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Company).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompanyLogoUrl).HasMaxLength(500);
        builder.Property(x => x.Location).HasMaxLength(200);
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.IsCurrent).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.Description).HasColumnType("text");
        builder.Property(x => x.Technologies).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("IX_Experiences_IsActive_DisplayOrder");
    }
}

