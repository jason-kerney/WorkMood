#if WINDOWS
using Windows.Storage.Pickers;
#endif

namespace WorkMood.MauiApp.Shims;

/// <summary>
/// Folder picker shim using platform-specific APIs.
/// </summary>
public class FolderPickerShim : IFolderPickerShim
{
    public async Task<string?> PickFolderAsync()
    {
#if WINDOWS
        try
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window platformWindow)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var folder = await picker.PickSingleFolderAsync();
            return folder?.Path;
        }
        catch
        {
            return null;
        }
#else
        // macOS and other platforms: not yet implemented
        return null;
#endif
    }
}
