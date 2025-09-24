using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Strategies;
using WorkMood.MauiApp.Processors;

namespace WorkMood.MauiApp.Factories;

/// <summary>
/// Factory interface for creating visualization services
/// </summary>
public interface IVisualizationServiceFactory
{
    /// <summary>
    /// Creates a visualization service with the specified configuration
    /// </summary>
    /// <param name="colorScheme">The color scheme to use</param>
    /// <returns>Configured visualization service</returns>
    IMoodVisualizationService CreateVisualizationService(VisualizationColorScheme colorScheme = VisualizationColorScheme.Default);
}

/// <summary>
/// Available color schemes for visualizations
/// </summary>
public enum VisualizationColorScheme
{
    Default,
    Accessible
}

/// <summary>
/// Factory for creating mood visualization services with different configurations
/// </summary>
public class VisualizationServiceFactory : IVisualizationServiceFactory
{
    public IMoodVisualizationService CreateVisualizationService(VisualizationColorScheme colorScheme = VisualizationColorScheme.Default)
    {
        var colorStrategy = CreateColorStrategy(colorScheme);
        var dataProcessor = new VisualizationDataProcessor(colorStrategy);
        
        return new MoodVisualizationService(dataProcessor, colorStrategy);
    }

    /// <summary>
    /// Creates the appropriate color strategy based on the selected scheme
    /// </summary>
    private static IMoodColorStrategy CreateColorStrategy(VisualizationColorScheme colorScheme)
    {
        return colorScheme switch
        {
            VisualizationColorScheme.Accessible => new AccessibleMoodColorStrategy(),
            VisualizationColorScheme.Default => new DefaultMoodColorStrategy(),
            _ => new DefaultMoodColorStrategy()
        };
    }
}