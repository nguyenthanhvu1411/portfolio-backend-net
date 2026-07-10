using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.ToTable("ProjectImages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Caption).HasMaxLength(255);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.IsThumbnail).HasDefaultValue(false).IsRequired();

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectImages)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ProjectImages_Projects_ProjectId");

        builder.HasIndex(x => new { x.ProjectId, x.DisplayOrder })
            .HasDatabaseName("IX_ProjectImages_ProjectId_DisplayOrder");
    }
}
