namespace WorkMood.MauiApp.Shims;

public interface IPathValidationShim
{
    /// <summary>
    /// Returns true if path is absolute (rooted) and contains no invalid characters.
    /// Returns false if path is null, empty, relative, or contains invalid path characters.
    /// Does NOT check whether the path exists or is writable.
    /// </summary>
    bool IsAbsolutePath(string? path);

    /// <summary>
    /// Attempts to write and delete a temporary file inside path.
    /// Returns true if write succeeded and cleanup succeeded.
    /// Returns false if the write attempt throws any exception.
    /// Does NOT throw on permission failure — returns false instead.
    /// </summary>
    bool HasWritePermission(string path);
}
