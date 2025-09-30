namespace WorkMood.MauiApp.Shims;

public interface IFolderShim
{
    string GetApplicationFolder();
    string GetArchiveFolder();
    string GetDesktopFolder();
    string CombinePaths(params string[] paths);
    void CreateDirectory(string path);
    string GetFileName(string path);
    bool DirectoryExists(string path);
    string[] GetFiles(string path, string searchPattern);
}
