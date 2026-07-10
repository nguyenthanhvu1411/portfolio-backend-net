using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Uploads.DTOs;
using Portfolio.Application.Uploads.Interfaces;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/uploads")]
[Produces("application/json")]
public sealed class AdminUploadsController : AdminControllerBase
{
    private readonly IAdminUploadService _service;

    public AdminUploadsController(IAdminUploadService service) => _service = service;

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadedFileDto>> UploadImage(
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadImageAsync(file, GetCurrentUserId(), cancellationToken));

    [HttpPost("file")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadedFileDto>> UploadFile(
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadFileAsync(file, GetCurrentUserId(), cancellationToken));

    [HttpPost("cv")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadedFileDto>> UploadCv(
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadCvAsync(file, GetCurrentUserId(), cancellationToken));

    [HttpDelete("{fileId:int}")]
    public async Task<ActionResult<OperationResult>> Delete(
        int fileId,
        CancellationToken cancellationToken) =>
        Ok(await _service.DeleteAsync(fileId, GetCurrentUserId(), cancellationToken));
}

