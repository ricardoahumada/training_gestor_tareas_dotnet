namespace TaskManager.Attachments.Domain.Interfaces;

/// <summary>
/// Abstraction for file storage operations (local disk, Azure Blob, AWS S3, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage.
    /// </summary>
    /// <param name="relativePath">Relative path (e.g., "{taskId}/{attachmentId}.jpg")</param>
    /// <param name="fileContent">File bytes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveFileAsync(string relativePath, byte[] fileContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file from storage.
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File bytes</returns>
    Task<byte[]> GetFileAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="relativePath">Relative path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}
