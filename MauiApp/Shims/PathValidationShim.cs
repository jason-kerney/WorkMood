namespace WorkMood.MauiApp.Shims;

public class PathValidationShim : IPathValidationShim
{
    public bool IsAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        try { return Path.IsPathRooted(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0; }
        catch { return false; }
    }

    public bool HasWritePermission(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $".workmood_write_test_{Guid.NewGuid()}");
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
