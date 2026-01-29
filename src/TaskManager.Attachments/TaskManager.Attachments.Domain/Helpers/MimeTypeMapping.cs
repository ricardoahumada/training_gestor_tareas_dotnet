namespace TaskManager.Attachments.Domain.Helpers;

/// <summary>
/// Maps between AllowedFileType enum, MIME types, and file extensions.
/// </summary>
public static class MimeTypeMapping
{
    /// <summary>
    /// Map AllowedFileType to MIME type string.
    /// </summary>
    public static readonly Dictionary<Enums.AllowedFileType, string> ToMimeType = new()
    {
        { Enums.AllowedFileType.Jpeg, "image/jpeg" },
        { Enums.AllowedFileType.Png, "image/png" },
        { Enums.AllowedFileType.Gif, "image/gif" },
        { Enums.AllowedFileType.Pdf, "application/pdf" },
        { Enums.AllowedFileType.Doc, "application/msword" },
        { Enums.AllowedFileType.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { Enums.AllowedFileType.Xls, "application/vnd.ms-excel" },
        { Enums.AllowedFileType.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };

    /// <summary>
    /// Map MIME type to allowed file extensions.
    /// </summary>
    public static readonly Dictionary<string, string[]> FileExtensions = new()
    {
        { "image/jpeg", new[] { ".jpg", ".jpeg" } },
        { "image/png", new[] { ".png" } },
        { "image/gif", new[] { ".gif" } },
        { "application/pdf", new[] { ".pdf" } },
        { "application/msword", new[] { ".doc" } },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new[] { ".docx" } },
        { "application/vnd.ms-excel", new[] { ".xls" } },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new[] { ".xlsx" } }
    };

    /// <summary>
    /// All allowed MIME types as a flat collection.
    /// </summary>
    public static readonly HashSet<string> AllowedMimeTypes = new(FileExtensions.Keys, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// All allowed file extensions (lowercase).
    /// </summary>
    public static readonly HashSet<string> AllowedExtensions = new(
        FileExtensions.Values.SelectMany(x => x),
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// Gets the primary file extension for a MIME type.
    /// </summary>
    public static string? GetExtensionForMimeType(string mimeType)
    {
        return FileExtensions.TryGetValue(mimeType, out var extensions) ? extensions[0] : null;
    }

    /// <summary>
    /// Checks if a MIME type is allowed.
    /// </summary>
    public static bool IsAllowedMimeType(string mimeType)
    {
        return AllowedMimeTypes.Contains(mimeType);
    }

    /// <summary>
    /// Checks if a file extension is allowed.
    /// </summary>
    public static bool IsAllowedExtension(string extension)
    {
        return AllowedExtensions.Contains(extension);
    }
}
