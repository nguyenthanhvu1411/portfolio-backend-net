using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Settings.DTOs;
using Portfolio.Application.Settings.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Settings;

public sealed class PublicSettingService : IPublicSettingService
{
    private readonly ApplicationDbContext _dbContext;

    public PublicSettingService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublicSettingDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.Settings
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                "Chưa có cấu hình website.");

        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                x => x.IsActive,
                cancellationToken);

        return new PublicSettingDto
        {
            SiteName = setting.SiteName,
            LogoUrl = setting.LogoUrl,
            FaviconUrl = setting.FaviconUrl,
            ThemeColor = setting.ThemeColor,
            SeoTitle = setting.SeoTitle,
            SeoDescription = setting.SeoDescription,
            ContactEmail = setting.ContactEmail,
            FooterText = setting.FooterText,
            GithubUrl = profile?.GithubUrl,
            LinkedinUrl = profile?.LinkedinUrl,
            FacebookUrl = profile?.FacebookUrl
        };
    }
}

