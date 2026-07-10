using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class EducationConfiguration : IEntityTypeConfiguration<Education>
{
    public void Configure(EntityTypeBuilder<Education> builder)
    {
        builder.ToTable("Education");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.SchoolName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Major).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Degree).HasMaxLength(100);
        builder.Property(x => x.GPA).HasMaxLength(50);
        builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Education_IsActive");
    }
}
