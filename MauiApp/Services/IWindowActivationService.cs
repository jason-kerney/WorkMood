namespace WorkMood.MauiApp.Services;

/// <summary>
/// Service for activating and bringing application windows to the foreground
/// </summary>
public interface IWindowActivationService
{
    /// <summary>
    /// Activates and brings the current application window to the foreground
    /// </summary>
    /// <returns>A task representing the activation operation</returns>
    Task ActivateCurrentWindowAsync();
}