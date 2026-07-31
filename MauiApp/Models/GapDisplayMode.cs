namespace WorkMood.MauiApp.Models;

/// <summary>
/// Defines how missing weekday gaps are displayed in line graphs.
/// </summary>
public enum GapDisplayMode
{
    ShowGaps = 0,
    GapsAsZero = 1,
    GapsAsMax = 2,
    GapsAsAverage = 3
}
