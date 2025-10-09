namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents a single data point for graph rendering with a value and timestamp
/// </summary>
/// <param name="Value">The numeric value to plot on the Y-axis</param>
/// <param name="Timestamp">The timestamp for X-axis positioning</param>
public record GraphDataPoint(float Value, DateTime Timestamp);