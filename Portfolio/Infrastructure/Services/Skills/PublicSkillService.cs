using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Skills;

public sealed class PublicSkillService : IPublicSkillService
{
    private readonly ApplicationDbContext _dbContext;

    public PublicSkillService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicSkillDto>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var skills = await BaseQuery()
            .Where(x => x.IsFeatured)
            .OrderBy(x => x.Category.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return skills.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<PublicSkillDto>> GetAllAsync(
        PublicSkillFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = BaseQuery();

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(
                x => x.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                (x.Description != null &&
                 x.Description.Contains(keyword)) ||
                x.Category.Name.Contains(keyword));
        }

        var skills = await query
            .OrderBy(x => x.Category.DisplayOrder)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return skills.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<PublicSkillCategoryDto>>
        GetCategoriesAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.SkillCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new PublicSkillCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                SkillCount = x.Skills.Count(skill =>
                    skill.IsActive)
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Skill> BaseQuery() =>
        _dbContext.Skills
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x =>
                x.IsActive &&
                x.Category.IsActive);

    private static PublicSkillDto Map(Skill x) =>
        new()
        {
            Id = x.Id,
            CategoryId = x.CategoryId,
            CategoryName = x.Category.Name,
            CategoryDisplayOrder =
                x.Category.DisplayOrder,
            Name = x.Name,
            Level = x.Level,
            LevelName = x.Level.HasValue
                ? x.Level.Value.GetDisplayName()
                : null,
            IconUrl = x.IconUrl,
            Description = x.Description,
            DisplayOrder = x.DisplayOrder,
            IsFeatured = x.IsFeatured
        };
}

