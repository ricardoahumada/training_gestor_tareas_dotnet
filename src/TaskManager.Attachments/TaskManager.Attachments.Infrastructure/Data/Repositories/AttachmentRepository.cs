using Microsoft.EntityFrameworkCore;
using TaskManager.Attachments.Domain.Entities;
using TaskManager.Attachments.Domain.Interfaces;

namespace TaskManager.Attachments.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of IAttachmentRepository.
/// </summary>
public class AttachmentRepository : IAttachmentRepository
{
    private readonly AttachmentsDbContext _context;

    public AttachmentRepository(AttachmentsDbContext context)
    {
        _context = context;
    }

    public async Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);
        return attachment;
    }

    public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Attachment attachment, CancellationToken cancellationToken = default)
    {
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Attachments
            .CountAsync(a => a.TaskId == taskId, cancellationToken);
    }
}
