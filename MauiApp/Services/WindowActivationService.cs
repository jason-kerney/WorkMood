using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Platform-specific implementation for window activation
/// </summary>
public class WindowActivationService : IWindowActivationService
{
    private readonly ILoggingService _loggingService;

    public WindowActivationService(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <summary>
    /// Activates and brings the current application window to the foreground
    /// </summary>
    public async Task ActivateCurrentWindowAsync()
    {
        try
        {
            // Get the current window from the main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var window = GetCurrentWindow();
                if (window != null)
                {
                    _loggingService.Log("WindowActivationService: Activating window");
                    
#if WINDOWS
                    await ActivateWindowsWindow(window);
#elif ANDROID
                    await ActivateAndroidWindow(window);
#elif IOS
                    await ActivateIosWindow(window);
#elif MACCATALYST
                    await ActivateMacWindow(window);
#else
                    _loggingService.Log(LogLevel.Warning, "WindowActivationService: Platform-specific activation not implemented for this platform");
#endif
                    
                    // Generic activation attempt
                    await Task.Delay(100); // Small delay to ensure activation completes
                }
                else
                {
                    _loggingService.Log(LogLevel.Warning, "WindowActivationService: Could not find current window for activation");
                }
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "WindowActivationService: Error activating window");
        }
    }

    private static Window? GetCurrentWindow()
    {
        // Try to get the window from the current application
        return Application.Current?.Windows?.FirstOrDefault();
    }

#if WINDOWS
    private async Task ActivateWindowsWindow(Window window)
    {
        try
        {
            var platformWindow = window.Handler?.PlatformView;
            if (platformWindow is Microsoft.UI.Xaml.Window winUIWindow)
            {
                // Method 1: Standard activation
                winUIWindow.Activate();
                _loggingService.Log("WindowActivationService: Called winUIWindow.Activate()");
                
                // Method 2: Try to get the native HWND and use Windows API
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(winUIWindow);
                if (hwnd != IntPtr.Zero)
                {
                    // Use Windows API to force window to foreground
                    SetForegroundWindow(hwnd);
                    ShowWindow(hwnd, SW_RESTORE); // Restore if minimized
                    SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    _loggingService.Log("WindowActivationService: Applied Windows API activation methods");
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "WindowActivationService: Error in Windows-specific activation");
        }
    }

    // Windows API declarations for window activation
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    private const int SW_RESTORE = 9;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;
#endif

#if ANDROID
    private Task ActivateAndroidWindow(Window window)
    {
        // Android-specific window activation logic can be added here
        _loggingService.Log("WindowActivationService: Android window activation (placeholder)");
        return Task.CompletedTask;
    }
#endif

#if IOS
    private Task ActivateIosWindow(Window window)
    {
        // iOS-specific window activation logic can be added here
        _loggingService.Log("WindowActivationService: iOS window activation (placeholder)");
        return Task.CompletedTask;
    }
#endif

#if MACCATALYST
    private Task ActivateMacWindow(Window window)
    {
        // macOS-specific window activation logic can be added here
        _loggingService.Log("WindowActivationService: macOS window activation (placeholder)");
        return Task.CompletedTask;
    }
#endif
}