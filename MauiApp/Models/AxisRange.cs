namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents the range for a graph axis with minimum and maximum values
/// </summary>
/// <param name="Min">The minimum value for the axis</param>
/// <param name="Max">The maximum value for the axis</param>
public record AxisRange(int Min, int Max)
{
    /// <summary>
    /// Creates an AxisRange for Impact mode data (-9 to +9)
    /// </summary>
    public static AxisRange Impact => new(-9, 9);
    
    /// <summary>
    /// Creates an AxisRange for Average mode data (-5 to +5)
    /// </summary>
    public static AxisRange Average => new(-5, 5);
    
    /// <summary>
    /// Creates an AxisRange for Raw Data mode data (1 to 10)
    /// </summary>
    public static AxisRange RawData => new(1, 10);
    
    /// <summary>
    /// Gets the range span (Max - Min)
    /// </summary>
    public int Range => Max - Min;
    
    /// <summary>
    /// Checks if a value falls within this range (inclusive)
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if the value is within the range</returns>
    public bool Contains(float value) => value >= Min && value <= Max;
    
    /// <summary>
    /// Checks if a value falls within this range (inclusive)
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if the value is within the range</returns>
    public bool Contains(int value) => value >= Min && value <= Max;
}