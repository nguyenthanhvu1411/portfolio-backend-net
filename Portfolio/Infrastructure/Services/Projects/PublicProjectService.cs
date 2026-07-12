using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Projects.DTOs;
using Portfolio.Application.Projects.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Projects;

public sealed class PublicProjectService
    : IPublicProjectService
{
    private readonly ApplicationDbContext _dbContext;

    public PublicProjectService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicProjectCardDto>>
        GetFeaturedAsync(
            int limit,
            CancellationToken cancellationToken = default)
    {
        var projects = await CardQuery()
            .Where(x => x.IsFeatured)
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return projects.Select(MapCard).ToArray();
    }

    public async Task<PagedResult<PublicProjectCardDto>>
        GetPagedAsync(
            PublicProjectFilterRequest filter,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.ProjectName.Contains(keyword) ||
                x.Slug.Contains(keyword) ||
                (x.ShortDescription != null &&
                 x.ShortDescription.Contains(keyword)) ||
                (x.ProjectType != null &&
                 x.ProjectType.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Technology))
        {
            var technology = filter.Technology.Trim();

            query = query.Where(x =>
                x.ProjectSkills.Any(projectSkill =>
                    projectSkill.Skill.IsActive &&
                    projectSkill.Skill.Category.IsActive &&
                    projectSkill.Skill.Name.Contains(technology)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(
                x => x.Status == filter.Status.Value);
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var projects = await query
            .Include(x => x.ProjectSkills)
                .ThenInclude(x => x.Skill)
                    .ThenInclude(x => x.Category)
            .Include(x => x.ProjectImages)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PublicProjectCardDto>.Create(
            projects.Select(MapCard).ToArray(),
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<PublicProjectDetailDto> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim();

        var project = await _dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.ProjectSkills)
                .ThenInclude(x => x.Skill)
                    .ThenInclude(x => x.Category)
            .Include(x => x.ProjectImages)
            .SingleOrDefaultAsync(
                x =>
                    x.IsActive &&
                    x.Slug == normalizedSlug,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án public có slug '{normalizedSlug}'.");

        return MapDetail(project);
    }

    public async Task<IReadOnlyList<PublicProjectImageDto>>
        GetImagesAsync(
            int projectId,
            CancellationToken cancellationToken = default)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == projectId && x.IsActive,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException(
                $"Không tìm thấy dự án public có Id = {projectId}.");
        }

        return await _dbContext.ProjectImages
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.IsThumbnail)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new PublicProjectImageDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ImageUrl = x.ImageUrl,
                Caption = x.Caption,
                DisplayOrder = x.DisplayOrder,
                IsThumbnail = x.IsThumbnail
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Project> CardQuery() =>
        _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.ProjectSkills)
                .ThenInclude(x => x.Skill)
                    .ThenInclude(x => x.Category)
            .Include(x => x.ProjectImages)
            .Where(x => x.IsActive);

    private static PublicProjectCardDto MapCard(
        Project project) =>
        new()
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Slug = project.Slug,
            ShortDescription =
                project.ShortDescription,
            ProjectType = project.ProjectType,
            ThumbnailUrl = project.ThumbnailUrl ?? project.ProjectImages
                .OrderByDescending(x => x.IsThumbnail)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(x => x.ImageUrl)
                .FirstOrDefault(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            StatusName =
                project.Status.GetDisplayName(),
            IsFeatured = project.IsFeatured,
            Skills = project.ProjectSkills
                .Where(x =>
                    x.Skill.IsActive &&
                    x.Skill.Category.IsActive)
                .OrderBy(x =>
                    x.Skill.Category.DisplayOrder)
                .ThenBy(x => x.Skill.DisplayOrder)
                .Select(x => MapSkill(x.Skill))
                .ToArray()
        };

    private static PublicProjectDetailDto MapDetail(
        Project project) =>
        new()
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Slug = project.Slug,
            ShortDescription =
                project.ShortDescription,
            FullDescription =
                project.FullDescription,
            Role = project.Role,
            ProjectType = project.ProjectType,
            ThumbnailUrl = project.ThumbnailUrl ?? project.ProjectImages
                .OrderByDescending(x => x.IsThumbnail)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(x => x.ImageUrl)
                .FirstOrDefault(),
            GithubUrl = project.GithubUrl,
            DemoUrl = project.DemoUrl,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            StatusName =
                project.Status.GetDisplayName(),
            IsFeatured = project.IsFeatured,
            Skills = project.ProjectSkills
                .Where(x =>
                    x.Skill.IsActive &&
                    x.Skill.Category.IsActive)
                .OrderBy(x =>
                    x.Skill.Category.DisplayOrder)
                .ThenBy(x => x.Skill.DisplayOrder)
                .Select(x => MapSkill(x.Skill))
                .ToArray(),
            Images = project.ProjectImages
                .OrderByDescending(x =>
                    x.IsThumbnail)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(x =>
                    new PublicProjectImageDto
                    {
                        Id = x.Id,
                        ProjectId = x.ProjectId,
                        ImageUrl = x.ImageUrl,
                        Caption = x.Caption,
                        DisplayOrder =
                            x.DisplayOrder,
                        IsThumbnail =
                            x.IsThumbnail
                    })
                .ToArray()
        };

    private static PublicProjectSkillDto MapSkill(
        Skill skill) =>
        new()
        {
            Id = skill.Id,
            Name = skill.Name,
            CategoryId = skill.CategoryId,
            CategoryName = skill.Category.Name,
            Level = skill.Level,
            LevelName = skill.Level.HasValue
                ? skill.Level.Value.GetDisplayName()
                : null,
            IconUrl = skill.IconUrl
        };
}

