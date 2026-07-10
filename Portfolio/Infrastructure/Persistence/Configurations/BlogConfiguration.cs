using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.Content).HasColumnType("text").IsRequired();
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ViewCount).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.IsFeatured).HasDefaultValue(false).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Blogs)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Blogs_BlogCategories_CategoryId");

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("UX_Blogs_Slug");

        builder.HasIndex(x => new { x.Status, x.PublishedAt })
            .HasDatabaseName("IX_Blogs_Status_PublishedAt");

        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("IX_Blogs_CategoryId");
    }
}

