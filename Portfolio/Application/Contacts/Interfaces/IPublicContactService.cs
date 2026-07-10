using Portfolio.Application.Contacts.DTOs;

namespace Portfolio.Application.Contacts.Interfaces;

public interface IPublicContactService
{
    Task<PublicContactMessageDto> CreateAsync(
        PublicContactMessageCreateRequest request,
        CancellationToken cancellationToken = default);
}

