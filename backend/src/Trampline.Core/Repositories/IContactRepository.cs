using Trampline.Core.Models.Worker;

namespace Trampline.Core.Repositories;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Contact?> GetByPairAsync(Guid requesterId, Guid receiverId, CancellationToken cancellationToken);

    Task<IEnumerable<Contact>> GetContactsAsync(Guid userId, CancellationToken cancellationToken);

    Task<IEnumerable<Contact>> GetIncomingAsync(Guid userId, CancellationToken cancellationToken);

    Task<Contact> AddAsync(Contact contact, CancellationToken cancellationToken);

    Task UpdateAsync(Contact contact, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
