namespace WorkMood.MauiApp.Shims;

public class FileShim : IFileShim
{
    public Task WriteAllTextAsync(string path, string contents)
    {
        return File.WriteAllTextAsync(path, contents);
    }

    public void AppendAllText(string path, string contents)
    {
        File.AppendAllText(path, contents);
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes)
    {
        return File.WriteAllBytesAsync(path, bytes);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }
}
