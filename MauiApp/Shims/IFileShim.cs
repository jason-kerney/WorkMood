namespace WorkMood.MauiApp.Shims;

public interface IFileShim
{
    Task WriteAllTextAsync(string path, string contents);
    Task<string> ReadAllTextAsync(string path);
    void AppendAllText(string path, string contents);
    Task WriteAllBytesAsync(string path, byte[] bytes);
    bool Exists(string path);
}
