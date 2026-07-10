using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class BlogTagMappingConfiguration : IEntityTypeConfiguration<BlogTagMapping>
{
    public void Configure(EntityTypeBuilder<BlogTagMapping> builder)
    {
        builder.ToTable("BlogTagMappings");

        builder.HasKey(x => new { x.BlogId, x.TagId });

        builder.HasOne(x => x.Blog)
            .WithMany(x => x.BlogTagMappings)
            .HasForeignKey(x => x.BlogId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_BlogTagMappings_Blogs_BlogId");

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.BlogTagMappings)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_BlogTagMappings_BlogTags_TagId");

        builder.HasIndex(x => x.TagId)
            .HasDatabaseName("IX_BlogTagMappings_TagId");
    }
}
