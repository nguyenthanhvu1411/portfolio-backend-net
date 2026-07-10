using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class ViewStatisticConfiguration : IEntityTypeConfiguration<ViewStatistic>
{
    public void Configure(EntityTypeBuilder<ViewStatistic> builder)
    {
        builder.ToTable("ViewStatistics");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // ViewStatistics chưa có chi tiết cột trong file Excel.
        // Các độ dài dưới đây khớp entity đề xuất hiện tại.
        builder.Property(x => x.PagePath).HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.ViewedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(x => x.Blog)
            .WithMany(x => x.ViewStatistics)
            .HasForeignKey(x => x.BlogId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ViewStatistics_Blogs_BlogId");

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ViewStatistics)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ViewStatistics_Projects_ProjectId");

        builder.HasIndex(x => new { x.BlogId, x.ViewedAt })
            .HasDatabaseName("IX_ViewStatistics_BlogId_ViewedAt");

        builder.HasIndex(x => new { x.ProjectId, x.ViewedAt })
            .HasDatabaseName("IX_ViewStatistics_ProjectId_ViewedAt");

        builder.HasIndex(x => new { x.PagePath, x.ViewedAt })
            .HasDatabaseName("IX_ViewStatistics_PagePath_ViewedAt");
    }
}
