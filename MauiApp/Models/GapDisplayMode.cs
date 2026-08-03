namespace WorkMood.MauiApp.Models;

/// <summary>
/// Defines how missing weekday gaps are displayed in line graphs.
/// </summary>
public enum GapDisplayMode
{
    ShowGaps = 0,
    GapsAsMin = 1,
    GapsAsMax = 2,
    GapsAsAverage = 3,
    GapsAsSurroundingAverage = 4,
    GapsAsMatchPreviousValue = 5,
    GapsAsMatchFollowingValue = 6
}
