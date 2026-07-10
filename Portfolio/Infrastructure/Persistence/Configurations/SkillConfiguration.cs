using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.IconUrl).HasMaxLength(500);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.IsFeatured).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Skills_SkillCategories_CategoryId");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_Skills_CategoryId");

        builder.HasIndex(x => new { x.IsActive, x.IsFeatured, x.DisplayOrder })
            .HasDatabaseName("IX_Skills_PublicDisplay");
    }
}
