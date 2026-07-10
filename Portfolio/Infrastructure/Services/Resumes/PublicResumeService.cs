using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Resume.DTOs;
using Portfolio.Application.Resume.Interfaces;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Resumes;

public sealed class PublicResumeService
    : IPublicResumeService
{
    private readonly ApplicationDbContext _dbContext;

    public PublicResumeService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PublicExperienceDto>>
        GetExperiencesAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Experiences
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .ThenBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.Id)
            .Select(x => new PublicExperienceDto
            {
                Id = x.Id,
                Position = x.Position,
                Company = x.Company,
                CompanyLogoUrl =
                    x.CompanyLogoUrl,
                Location = x.Location,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                Description = x.Description,
                Technologies = x.Technologies,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PublicEducationDto>>
        GetEducationAsync(
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.Education
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.EndYear)
            .ThenByDescending(x => x.StartYear)
            .ThenByDescending(x => x.Id)
            .Select(x => new PublicEducationDto
            {
                Id = x.Id,
                SchoolName = x.SchoolName,
                Major = x.Major,
                Degree = x.Degree,
                StartYear = x.StartYear,
                EndYear = x.EndYear,
                GPA = x.GPA,
                Description = x.Description,
                LogoUrl = x.LogoUrl
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PublicCertificateDto>>
        GetCertificatesAsync(
            PublicCertificateFilterRequest filter,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Certificates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.Organization.Contains(keyword) ||
                (x.CredentialId != null &&
                 x.CredentialId.Contains(keyword)) ||
                (x.Description != null &&
                 x.Description.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(
                filter.Organization))
        {
            var organization =
                filter.Organization.Trim();

            query = query.Where(x =>
                x.Organization.Contains(organization));
        }

        return await query
            .OrderByDescending(x => x.IssueDate)
            .ThenBy(x => x.Name)
            .Select(x => new PublicCertificateDto
            {
                Id = x.Id,
                Name = x.Name,
                Organization = x.Organization,
                IssueDate = x.IssueDate,
                ExpiryDate = x.ExpiryDate,
                CredentialId = x.CredentialId,
                CredentialUrl = x.CredentialUrl,
                ImageUrl = x.ImageUrl,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
    }
}
