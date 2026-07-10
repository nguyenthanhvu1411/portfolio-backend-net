using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Projects.DTOs;
using Portfolio.Application.Projects.Interfaces;
using Portfolio.Application.Projects.Validators;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Projects;

public sealed class AdminProjectService : IAdminProjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminProjectService(
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResult<ProjectDto>> GetPagedAsync(
        ProjectFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.ProjectName.Contains(keyword) ||
                x.Slug.Contains(keyword) ||
                (x.ShortDescription != null && x.ShortDescription.Contains(keyword)) ||
                (x.ProjectType != null && x.ProjectType.Contains(keyword)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ProjectDto
            {
                Id = x.Id,
                ProjectName = x.ProjectName,
                Slug = x.Slug,
                ShortDescription = x.ShortDescription,
                FullDescription = x.FullDescription,
                Role = x.Role,
                ProjectType = x.ProjectType,
                ThumbnailUrl = x.ThumbnailUrl,
                GithubUrl = x.GithubUrl,
                DemoUrl = x.DemoUrl,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                IsFeatured = x.IsFeatured,
                IsActive = x.IsActive,
                SkillCount = x.ProjectSkills.Count,
                ImageCount = x.ProjectImages.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.StatusName = item.Status.GetDisplayName();
        }

        return PagedResult<ProjectDto>.Create(
            items,
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<ProjectDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.ProjectSkills)
                .ThenInclude(x => x.Skill)
                    .ThenInclude(x => x.Category)
            .Include(x => x.ProjectImages)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {id}.");

        return MapDetail(project);
    }

    public async Task<ProjectDto> CreateAsync(
        ProjectCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skillIds = request.SkillIds.Distinct().ToArray();
        await EnsureSkillsExistAsync(skillIds, cancellationToken);

        var projectName = request.ProjectName.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await GenerateUniqueSlugAsync(projectName, null, cancellationToken)
            : NormalizeSlug(request.Slug);

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            await EnsureSlugIsUniqueAsync(slug, null, cancellationToken);
        }

        var project = new Project
        {
            ProjectName = projectName,
            Slug = slug,
            ShortDescription = TrimToNull(request.ShortDescription),
            FullDescription = TrimToNull(request.FullDescription),
            Role = TrimToNull(request.Role),
            ProjectType = TrimToNull(request.ProjectType),
            GithubUrl = TrimToNull(request.GithubUrl),
            DemoUrl = TrimToNull(request.DemoUrl),
            StartDate = request.StartDate?.Date,
            EndDate = request.EndDate?.Date,
            Status = request.Status,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive
        };

        foreach (var skillId in skillIds)
        {
            project.ProjectSkills.Add(new ProjectSkill
            {
                SkillId = skillId
            });
        }

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetSummaryByIdAsync(project.Id, cancellationToken);

        AddAuditLog(
            currentUserId,
            AuditAction.Create,
            nameof(Project),
            project.Id,
            null,
            result);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<ProjectDto> UpdateAsync(
        int id,
        ProjectUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await _dbContext.Projects
            .Include(x => x.ProjectSkills)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {id}.");

        var skillIds = request.SkillIds.Distinct().ToArray();
        await EnsureSkillsExistAsync(skillIds, cancellationToken);

        var oldValue = await GetSummaryByIdAsync(id, cancellationToken);

        var slug = project.Slug;
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            slug = NormalizeSlug(request.Slug);
            await EnsureSlugIsUniqueAsync(slug, id, cancellationToken);
        }

        project.ProjectName = request.ProjectName.Trim();
        project.Slug = slug;
        project.ShortDescription = TrimToNull(request.ShortDescription);
        project.FullDescription = TrimToNull(request.FullDescription);
        project.Role = TrimToNull(request.Role);
        project.ProjectType = TrimToNull(request.ProjectType);
        project.GithubUrl = TrimToNull(request.GithubUrl);
        project.DemoUrl = TrimToNull(request.DemoUrl);
        project.StartDate = request.StartDate?.Date;
        project.EndDate = request.EndDate?.Date;
        project.Status = request.Status;
        project.IsFeatured = request.IsFeatured;
        project.IsActive = request.IsActive;

        var requestedSet = skillIds.ToHashSet();
        var removed = project.ProjectSkills
            .Where(x => !requestedSet.Contains(x.SkillId))
            .ToArray();

        _dbContext.ProjectSkills.RemoveRange(removed);

        var existingSet = project.ProjectSkills
            .Select(x => x.SkillId)
            .ToHashSet();

        foreach (var skillId in skillIds.Where(x => !existingSet.Contains(x)))
        {
            project.ProjectSkills.Add(new ProjectSkill
            {
                ProjectId = project.Id,
                SkillId = skillId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var newValue = await GetSummaryByIdAsync(id, cancellationToken);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(Project),
            id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetTrackedProjectAsync(id, cancellationToken);
        var oldValue = MapSummary(project);

        project.IsActive = false;
        project.IsFeatured = false;

        var newValue = MapSummary(project);

        AddAuditLog(
            currentUserId,
            AuditAction.Delete,
            nameof(Project),
            id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Dự án đã được ẩn thay vì xóa vật lý để giữ dữ liệu liên quan."
        };
    }

    public async Task<ProjectDto> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetTrackedProjectAsync(id, cancellationToken);
        var oldValue = MapSummary(project);

        project.IsActive = !project.IsActive;
        if (!project.IsActive)
        {
            project.IsFeatured = false;
        }

        var newValue = MapSummary(project);
        AddAuditLog(currentUserId, AuditAction.Update, nameof(Project), id, oldValue, newValue);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<ProjectDto> ToggleFeaturedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await GetTrackedProjectAsync(id, cancellationToken);

        if (!project.IsActive && !project.IsFeatured)
        {
            throw new ConflictException(
                "Không thể đặt dự án ngưng hoạt động thành dự án nổi bật.");
        }

        var oldValue = MapSummary(project);
        project.IsFeatured = !project.IsFeatured;
        var newValue = MapSummary(project);

        AddAuditLog(currentUserId, AuditAction.Update, nameof(Project), id, oldValue, newValue);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<ProjectFileUploadResponse> UploadThumbnailAsync(
        int projectId,
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await ProjectImageFileValidator.ValidateAsync(file, cancellationToken);

        var project = await _dbContext.Projects
            .Include(x => x.ProjectImages)
            .SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {projectId}.");

        var oldUrl = project.ThumbnailUrl;
        var storedFile = await _fileStorageService.SaveAsync(
            file,
            $"uploads/projects/{projectId}/thumbnail",
            cancellationToken);

        try
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            foreach (var image in project.ProjectImages)
            {
                image.IsThumbnail = false;
            }

            project.ThumbnailUrl = storedFile.FileUrl;

            _dbContext.UploadedFiles.Add(ToUploadedFile(storedFile, currentUserId));

            var canDeleteOld = !string.IsNullOrWhiteSpace(oldUrl) &&
                               !project.ProjectImages.Any(x => x.ImageUrl == oldUrl);

            if (canDeleteOld)
            {
                await RemoveUploadedFileRecordAsync(oldUrl!, cancellationToken);
            }

            AddAuditLog(
                currentUserId,
                AuditAction.Update,
                nameof(Project),
                projectId,
                oldUrl,
                storedFile.FileUrl);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (canDeleteOld)
            {
                await _fileStorageService.DeleteAsync(oldUrl, cancellationToken);
            }
        }
        catch
        {
            await _fileStorageService.DeleteAsync(storedFile.FileUrl, cancellationToken);
            throw;
        }

        return new ProjectFileUploadResponse
        {
            FileUrl = storedFile.FileUrl,
            OriginalFileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize
        };
    }

    public async Task<IReadOnlyCollection<ProjectImageDto>> GetImagesAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProjectExistsAsync(projectId, cancellationToken);

        return await _dbContext.ProjectImages
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.IsThumbnail)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new ProjectImageDto
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

    public async Task<ProjectImageDto> UploadImageAsync(
        int projectId,
        ProjectImageUploadRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await ProjectImageFileValidator.ValidateAsync(request.File, cancellationToken);
        await EnsureProjectExistsAsync(projectId, cancellationToken);

        var storedFile = await _fileStorageService.SaveAsync(
            request.File,
            $"uploads/projects/{projectId}/gallery",
            cancellationToken);

        var image = new ProjectImage
        {
            ProjectId = projectId,
            ImageUrl = storedFile.FileUrl,
            Caption = TrimToNull(request.Caption),
            DisplayOrder = request.DisplayOrder,
            IsThumbnail = false
        };

        try
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            _dbContext.ProjectImages.Add(image);
            _dbContext.UploadedFiles.Add(ToUploadedFile(storedFile, currentUserId));

            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = MapImage(image);
            AddAuditLog(
                currentUserId,
                AuditAction.Create,
                nameof(ProjectImage),
                image.Id,
                null,
                result);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await _fileStorageService.DeleteAsync(storedFile.FileUrl, cancellationToken);
            throw;
        }
    }

    public async Task<ProjectImageDto> UpdateImageAsync(
        int imageId,
        ProjectImageUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var image = await _dbContext.ProjectImages
            .SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy ảnh dự án có Id = {imageId}.");

        var oldValue = MapImage(image);
        image.Caption = TrimToNull(request.Caption);
        image.DisplayOrder = request.DisplayOrder;
        var newValue = MapImage(image);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(ProjectImage),
            imageId,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<OperationResult> DeleteImageAsync(
        int imageId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var image = await _dbContext.ProjectImages
            .Include(x => x.Project)
            .SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy ảnh dự án có Id = {imageId}.");

        var oldValue = MapImage(image);
        var imageUrl = image.ImageUrl;

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        if (image.IsThumbnail || image.Project.ThumbnailUrl == image.ImageUrl)
        {
            image.Project.ThumbnailUrl = null;
        }

        _dbContext.ProjectImages.Remove(image);
        await RemoveUploadedFileRecordAsync(imageUrl, cancellationToken);

        AddAuditLog(
            currentUserId,
            AuditAction.Delete,
            nameof(ProjectImage),
            imageId,
            oldValue,
            null);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _fileStorageService.DeleteAsync(imageUrl, cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa ảnh dự án thành công."
        };
    }

    public async Task<OperationResult> SetThumbnailAsync(
        int imageId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var selectedImage = await _dbContext.ProjectImages
            .Include(x => x.Project)
                .ThenInclude(x => x.ProjectImages)
            .SingleOrDefaultAsync(x => x.Id == imageId, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy ảnh dự án có Id = {imageId}.");

        var project = selectedImage.Project;
        var oldThumbnailUrl = project.ThumbnailUrl;

        foreach (var image in project.ProjectImages)
        {
            image.IsThumbnail = image.Id == imageId;
        }

        project.ThumbnailUrl = selectedImage.ImageUrl;

        var oldStandaloneThumbnail = !string.IsNullOrWhiteSpace(oldThumbnailUrl) &&
                                     oldThumbnailUrl != selectedImage.ImageUrl &&
                                     !project.ProjectImages.Any(x => x.ImageUrl == oldThumbnailUrl);

        if (oldStandaloneThumbnail)
        {
            await RemoveUploadedFileRecordAsync(oldThumbnailUrl!, cancellationToken);
        }

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(Project),
            project.Id,
            oldThumbnailUrl,
            selectedImage.ImageUrl);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (oldStandaloneThumbnail)
        {
            await _fileStorageService.DeleteAsync(oldThumbnailUrl, cancellationToken);
        }

        return new OperationResult
        {
            Success = true,
            Message = "Đã đặt ảnh làm thumbnail của dự án."
        };
    }

    private async Task<Project> GetTrackedProjectAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .Include(x => x.ProjectSkills)
            .Include(x => x.ProjectImages)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {id}.");
    }

    private async Task<ProjectDto> GetSummaryByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProjectDto
            {
                Id = x.Id,
                ProjectName = x.ProjectName,
                Slug = x.Slug,
                ShortDescription = x.ShortDescription,
                FullDescription = x.FullDescription,
                Role = x.Role,
                ProjectType = x.ProjectType,
                ThumbnailUrl = x.ThumbnailUrl,
                GithubUrl = x.GithubUrl,
                DemoUrl = x.DemoUrl,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                IsFeatured = x.IsFeatured,
                IsActive = x.IsActive,
                SkillCount = x.ProjectSkills.Count,
                ImageCount = x.ProjectImages.Count
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {id}.");

        result.StatusName = result.Status.GetDisplayName();
        return result;
    }

    private async Task EnsureProjectExistsAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken))
        {
            throw new NotFoundException(
                $"Không tìm thấy dự án có Id = {projectId}.");
        }
    }

    private async Task EnsureSkillsExistAsync(
        IReadOnlyCollection<int> skillIds,
        CancellationToken cancellationToken)
    {
        if (skillIds.Count == 0)
        {
            return;
        }

        var foundIds = await _dbContext.Skills
            .Where(x => skillIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var foundSet = foundIds.ToHashSet();
        var missingIds = skillIds
            .Where(x => !foundSet.Contains(x))
            .OrderBy(x => x)
            .ToArray();

        if (missingIds.Length > 0)
        {
            throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id: {string.Join(", ", missingIds)}.");
        }
    }

    private async Task EnsureSlugIsUniqueAsync(
        string slug,
        int? excludedProjectId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Projects.AnyAsync(
            x => x.Slug == slug &&
                 (!excludedProjectId.HasValue || x.Id != excludedProjectId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"Slug '{slug}' đã được sử dụng bởi dự án khác.");
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string projectName,
        int? excludedProjectId,
        CancellationToken cancellationToken)
    {
        var baseSlug = NormalizeSlug(projectName);
        var candidate = baseSlug;
        var suffix = 2;

        while (await _dbContext.Projects.AnyAsync(
                   x => x.Slug == candidate &&
                        (!excludedProjectId.HasValue || x.Id != excludedProjectId.Value),
                   cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        return candidate;
    }

    private async Task RemoveUploadedFileRecordAsync(
        string fileUrl,
        CancellationToken cancellationToken)
    {
        var uploadedFile = await _dbContext.UploadedFiles
            .SingleOrDefaultAsync(x => x.FileUrl == fileUrl, cancellationToken);

        if (uploadedFile is not null)
        {
            _dbContext.UploadedFiles.Remove(uploadedFile);
        }
    }

    private static UploadedFile ToUploadedFile(
        Portfolio.Application.Profiles.Models.StoredFileResult storedFile,
        int currentUserId)
    {
        return new UploadedFile
        {
            OriginalFileName = storedFile.OriginalFileName,
            StoredFileName = storedFile.StoredFileName,
            FileUrl = storedFile.FileUrl,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize,
            UploadedBy = currentUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void AddAuditLog(
        int currentUserId,
        AuditAction action,
        string entityName,
        int entityId,
        object? oldValue,
        object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static ProjectDto MapSummary(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Slug = project.Slug,
            ShortDescription = project.ShortDescription,
            FullDescription = project.FullDescription,
            Role = project.Role,
            ProjectType = project.ProjectType,
            ThumbnailUrl = project.ThumbnailUrl,
            GithubUrl = project.GithubUrl,
            DemoUrl = project.DemoUrl,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            StatusName = project.Status.GetDisplayName(),
            IsFeatured = project.IsFeatured,
            IsActive = project.IsActive,
            SkillCount = project.ProjectSkills.Count,
            ImageCount = project.ProjectImages.Count
        };
    }

    private static ProjectDetailDto MapDetail(Project project)
    {
        return new ProjectDetailDto
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            Slug = project.Slug,
            ShortDescription = project.ShortDescription,
            FullDescription = project.FullDescription,
            Role = project.Role,
            ProjectType = project.ProjectType,
            ThumbnailUrl = project.ThumbnailUrl,
            GithubUrl = project.GithubUrl,
            DemoUrl = project.DemoUrl,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            StatusName = project.Status.GetDisplayName(),
            IsFeatured = project.IsFeatured,
            IsActive = project.IsActive,
            SkillCount = project.ProjectSkills.Count,
            ImageCount = project.ProjectImages.Count,
            Skills = project.ProjectSkills
                .OrderBy(x => x.Skill.Category.DisplayOrder)
                .ThenBy(x => x.Skill.DisplayOrder)
                .ThenBy(x => x.Skill.Name)
                .Select(x => new ProjectSkillDto
                {
                    Id = x.Skill.Id,
                    Name = x.Skill.Name,
                    CategoryId = x.Skill.CategoryId,
                    CategoryName = x.Skill.Category.Name,
                    Level = x.Skill.Level,
                    LevelName = x.Skill.Level?.GetDisplayName(),
                    IconUrl = x.Skill.IconUrl,
                    IsActive = x.Skill.IsActive
                })
                .ToArray(),
            Images = project.ProjectImages
                .OrderByDescending(x => x.IsThumbnail)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .Select(MapImage)
                .ToArray()
        };
    }

    private static ProjectImageDto MapImage(ProjectImage image)
    {
        return new ProjectImageDto
        {
            Id = image.Id,
            ProjectId = image.ProjectId,
            ImageUrl = image.ImageUrl,
            Caption = image.Caption,
            DisplayOrder = image.DisplayOrder,
            IsThumbnail = image.IsThumbnail
        };
    }

    private static string NormalizeSlug(string value)
    {
        var normalized = value.Trim()
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        slug = Regex.Replace(slug, "[^a-z0-9]+", "-");
        slug = slug.Trim('-');

        if (!string.IsNullOrWhiteSpace(slug))
        {
            return slug;
        }

        var fallback = $"project-{Guid.NewGuid():N}";
        return fallback[..16];
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
