using Portfolio.Application.Common.Models;
using Portfolio.Application.Contacts.DTOs;

namespace Portfolio.Application.Contacts.Interfaces;

public interface IAdminContactMessageService
{
    Task<PagedResult<ContactMessageDto>> GetPagedAsync(
        ContactMessageFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> GetByIdAsync(
        int id,
        int currentUserId,
        bool autoMarkAsRead = true,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkAsReadAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkAsRepliedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> ArchiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
