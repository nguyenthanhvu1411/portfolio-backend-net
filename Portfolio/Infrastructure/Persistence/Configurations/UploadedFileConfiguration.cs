using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.ToTable("UploadedFiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UploadedFiles)
            .HasForeignKey(x => x.UploadedBy)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_UploadedFiles_Users_UploadedBy");

        builder.HasIndex(x => x.UploadedBy)
            .HasDatabaseName("IX_UploadedFiles_UploadedBy");

        builder.HasIndex(x => x.StoredFileName)
            .HasDatabaseName("IX_UploadedFiles_StoredFileName");
    }
}


