using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.SiteName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.FaviconUrl).HasMaxLength(500);
        builder.Property(x => x.ThemeColor).HasMaxLength(30);
        builder.Property(x => x.SeoTitle).HasMaxLength(250);
        builder.Property(x => x.SeoDescription).HasMaxLength(500);
        builder.Property(x => x.ContactEmail).HasMaxLength(255);
        builder.Property(x => x.FooterText).HasMaxLength(500);
    }
}
