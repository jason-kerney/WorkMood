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
}
