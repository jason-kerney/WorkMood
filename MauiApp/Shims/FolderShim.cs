namespace WorkMood.MauiApp.Shims;

public class FolderShim : IFolderShim
{
    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public string GetApplicationFolder() =>
        CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkMood");

    public string GetArchiveFolder() =>
        CombinePaths(GetApplicationFolder(), "archives");

    public string GetDesktopFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public string GetFileName(string path) => Path.GetFileName(path);

    public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
}
