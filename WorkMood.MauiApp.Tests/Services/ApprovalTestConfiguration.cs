using ApprovalTests.Reporters;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Global configuration for ApprovalTests to ensure consistent behavior across all test files
/// </summary>
public static class ApprovalTestConfiguration
{
    static ApprovalTestConfiguration()
    {
        // Configuration is applied through attributes on test classes
        // This static constructor ensures the class is loaded
    }
    
    /// <summary>
    /// Call this method in test constructors to ensure configuration is applied
    /// </summary>
    public static void Initialize()
    {
        // Configuration is applied in static constructor
        // This method serves as an explicit initialization point if needed
    }
}