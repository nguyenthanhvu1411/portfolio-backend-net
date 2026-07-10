using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contacts.DTOs;
using Portfolio.Application.Contacts.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Contacts;

public sealed class AdminContactMessageService
    : IAdminContactMessageService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminContactMessageService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ContactMessageDto>> GetPagedAsync(
        ContactMessageFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ContactMessages
            .AsNoTracking()
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(
                x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.FullName.Contains(keyword) ||
                x.Email.Contains(keyword) ||
                x.Subject.Contains(keyword) ||
                x.Message.Contains(keyword) ||
                (x.Phone != null && x.Phone.Contains(keyword)) ||
                (x.Company != null && x.Company.Contains(keyword)));
        }

        if (filter.FromDate.HasValue)
        {
            var fromDate = filter.FromDate.Value.Date;

            query = query.Where(
                x => x.CreatedAt >= fromDate);
        }

        if (filter.ToDate.HasValue)
        {
            // Dùng mốc nhỏ hơn ngày kế tiếp để bao gồm toàn bộ ToDate.
            var toDateExclusive =
                filter.ToDate.Value.Date.AddDays(1);

            query = query.Where(
                x => x.CreatedAt < toDateExclusive);
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var items = await query
            // Tin nhắn New luôn được ưu tiên lên đầu.
            .OrderBy(x =>
                x.Status == ContactStatus.New ? 0 : 1)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ContactMessageDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                Company = x.Company,
                Subject = x.Subject,
                Message = x.Message,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.StatusName =
                item.Status.GetDisplayName();
        }

        return PagedResult<ContactMessageDto>.Create(
            items,
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<ContactMessageDto> GetByIdAsync(
        int id,
        int currentUserId,
        bool autoMarkAsRead = true,
        CancellationToken cancellationToken = default)
    {
        var contactMessage =
            await GetTrackedAsync(id, cancellationToken);

        if (autoMarkAsRead &&
            contactMessage.Status == ContactStatus.New)
        {
            var oldValue = Map(contactMessage);

            contactMessage.Status = ContactStatus.Read;

            var newValue = Map(contactMessage);

            AddAuditLog(
                currentUserId,
                AuditAction.Update,
                nameof(ContactMessage),
                contactMessage.Id,
                oldValue,
                newValue);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return Map(contactMessage);
    }

    public async Task<ContactMessageDto> MarkAsReadAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var contactMessage =
            await GetTrackedAsync(id, cancellationToken);

        // Replied và Archived không bị hạ ngược về Read.
        if (contactMessage.Status != ContactStatus.New)
        {
            return Map(contactMessage);
        }

        return await ChangeStatusAsync(
            contactMessage,
            ContactStatus.Read,
            currentUserId,
            cancellationToken);
    }

    public async Task<ContactMessageDto> MarkAsRepliedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var contactMessage =
            await GetTrackedAsync(id, cancellationToken);

        if (contactMessage.Status == ContactStatus.Archived)
        {
            throw new ConflictException(
                "Tin nhắn đã được lưu trữ nên không thể đánh dấu đã phản hồi.");
        }

        if (contactMessage.Status == ContactStatus.Replied)
        {
            return Map(contactMessage);
        }

        return await ChangeStatusAsync(
            contactMessage,
            ContactStatus.Replied,
            currentUserId,
            cancellationToken);
    }

    public async Task<ContactMessageDto> ArchiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var contactMessage =
            await GetTrackedAsync(id, cancellationToken);

        if (contactMessage.Status == ContactStatus.Archived)
        {
            return Map(contactMessage);
        }

        return await ChangeStatusAsync(
            contactMessage,
            ContactStatus.Archived,
            currentUserId,
            cancellationToken);
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var contactMessage =
            await GetTrackedAsync(id, cancellationToken);

        var oldValue = Map(contactMessage);

        _dbContext.ContactMessages.Remove(contactMessage);

        AddAuditLog(
            currentUserId,
            AuditAction.Delete,
            nameof(ContactMessage),
            contactMessage.Id,
            oldValue,
            newValue: null);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa tin nhắn liên hệ thành công."
        };
    }

    private async Task<ContactMessageDto> ChangeStatusAsync(
        ContactMessage contactMessage,
        ContactStatus newStatus,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var oldValue = Map(contactMessage);

        contactMessage.Status = newStatus;

        var newValue = Map(contactMessage);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(ContactMessage),
            contactMessage.Id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return newValue;
    }

    private async Task<ContactMessage> GetTrackedAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ContactMessages
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy tin nhắn liên hệ có Id = {id}.");
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
            OldValue = oldValue is null
                ? null
                : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null
                ? null
                : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static ContactMessageDto Map(
        ContactMessage contactMessage)
    {
        return new ContactMessageDto
        {
            Id = contactMessage.Id,
            FullName = contactMessage.FullName,
            Email = contactMessage.Email,
            Phone = contactMessage.Phone,
            Company = contactMessage.Company,
            Subject = contactMessage.Subject,
            Message = contactMessage.Message,
            Status = contactMessage.Status,
            StatusName =
                contactMessage.Status.GetDisplayName(),
            CreatedAt = contactMessage.CreatedAt
        };
    }
}
