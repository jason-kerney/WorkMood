using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Strategies;

/// <summary>
/// Default color strategy for mood values using red-green color scheme
/// </summary>
public class DefaultMoodColorStrategy : IMoodColorStrategy
{
    public Color GetColorForValue(double value, double maxAbsValue)
    {
        // Value 0 is neutral (no change)
        // Values > 0 are positive (mood improved - green shades)
        // Values < 0 are negative (mood declined - red shades)
        // Scale intensity based on the actual data range for better visibility
        
        if (value > 0)
        {
            // Positive - shades of green (mood improved)
            var intensity = Math.Min(1.0, value / maxAbsValue);
            return Color.FromRgb(
                (float)(0.2 + (1.0 - intensity) * 0.6), // Red component (darker green when higher)
                1.0f, // Green component
                (float)(0.2 + (1.0 - intensity) * 0.6)  // Blue component
            );
        }
        else if (value < 0)
        {
            // Negative - shades of red (mood declined)
            var intensity = Math.Min(1.0, Math.Abs(value) / maxAbsValue);
            return Color.FromRgb(
                1.0f, // Red component
                (float)(0.2 + (1.0 - intensity) * 0.6), // Green component (darker red when lower)
                (float)(0.2 + (1.0 - intensity) * 0.6)  // Blue component
            );
        }
        else
        {
            // Zero - neutral blue (no mood change)
            return Color.FromRgb(0.5f, 0.7f, 1.0f);
        }
    }
}

/// <summary>
/// Alternative color strategy using blue-orange color scheme for accessibility
/// </summary>
public class AccessibleMoodColorStrategy : IMoodColorStrategy
{
    public Color GetColorForValue(double value, double maxAbsValue)
    {
        if (value > 0)
        {
            // Positive - shades of blue
            var intensity = Math.Min(1.0, value / maxAbsValue);
            return Color.FromRgb(
                (float)(0.1 + (1.0 - intensity) * 0.5), // Red component
                (float)(0.3 + (1.0 - intensity) * 0.4), // Green component
                1.0f  // Blue component (full blue for positive)
            );
        }
        else if (value < 0)
        {
            // Negative - shades of orange
            var intensity = Math.Min(1.0, Math.Abs(value) / maxAbsValue);
            return Color.FromRgb(
                1.0f, // Red component (full red for orange)
                (float)(0.4 + (1.0 - intensity) * 0.3), // Green component (orange when combined with red)
                (float)(0.1 + (1.0 - intensity) * 0.2)  // Blue component
            );
        }
        else
        {
            // Zero - neutral gray
            return Color.FromRgb(0.6f, 0.6f, 0.6f);
        }
    }
}