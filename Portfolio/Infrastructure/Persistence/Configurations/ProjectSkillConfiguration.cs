using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class ProjectSkillConfiguration : IEntityTypeConfiguration<ProjectSkill>
{
    public void Configure(EntityTypeBuilder<ProjectSkill> builder)
    {
        builder.ToTable("ProjectSkills");

        builder.HasKey(x => new { x.ProjectId, x.SkillId });

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectSkills)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ProjectSkills_Projects_ProjectId");

        builder.HasOne(x => x.Skill)
            .WithMany(x => x.ProjectSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ProjectSkills_Skills_SkillId");

        builder.HasIndex(x => x.SkillId)
            .HasDatabaseName("IX_ProjectSkills_SkillId");
    }
}
