namespace WorkMood.MauiApp.Shims;

public class FileShimFactory : IFileShimFactory
{
    public IFileShim Create()
    {
        return new FileShim();
    }
}