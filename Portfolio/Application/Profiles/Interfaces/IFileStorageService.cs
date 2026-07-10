using Microsoft.AspNetCore.Http;
using Portfolio.Application.Profiles.Models;

namespace Portfolio.Application.Profiles.Interfaces;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string? fileUrl,
        CancellationToken cancellationToken = default);
}
