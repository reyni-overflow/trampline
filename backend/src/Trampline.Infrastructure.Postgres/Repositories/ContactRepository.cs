using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class ContactRepository(ILogger<ContactRepository> logger, AppDbContext context) : IContactRepository
{
    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contact?> GetByPairAsync(Guid requesterId, Guid receiverId, CancellationToken cancellationToken)
    {
        return await context.Contacts.FirstOrDefaultAsync(
            c => (c.RequesterId == requesterId && c.ReceiverId == receiverId) ||
                 (c.RequesterId == receiverId && c.ReceiverId == requesterId),
            cancellationToken);
    }

    public async Task<IEnumerable<Contact>> GetContactsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Contacts
            .Where(c => (c.RequesterId == userId || c.ReceiverId == userId) && c.Status == ContactStatus.Accepted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contact>> GetIncomingAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Contacts
            .Where(c => c.ReceiverId == userId && c.Status == ContactStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contact> AddAsync(Contact contact, CancellationToken cancellationToken)
    {
        await context.Contacts.AddAsync(contact, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return contact;
    }

    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken)
    {
        context.Contacts.Update(contact);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await context.Contacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contact != null)
        {
            context.Contacts.Remove(contact);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
