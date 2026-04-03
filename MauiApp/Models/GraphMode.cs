namespace WorkMood.MauiApp.Models;

/// <summary>
/// Defines the different modes for displaying mood data in graphs
/// </summary>
public enum GraphMode
{
    /// <summary>
    /// Shows the impact/change in mood throughout the day
    /// Formula: (EndOfWork ?? StartOfWork) - StartOfWork
    /// Range: -9 to +9
    /// </summary>
    Impact,

    /// <summary>
    /// Shows the impact on mood between recorded work periods.
    /// Formula: Current StartOfWork - (Previous EndOfWork ?? Previous StartOfWork)
    /// The first matching day is omitted because no previous comparison point exists.
    /// Range: -9 to +9
    /// </summary>
    GeneralImpact,

    /// <summary>
    /// Shows the average mood for the day, adjusted to a -5 to +5 scale
    /// Formula: ((StartOfWork + (EndOfWork ?? StartOfWork)) / 2) - 5
    /// Range: -4 to +5 (since mood values are 1-10)
    /// </summary>
    Average,

    /// <summary>
    /// Shows individual mood recordings as separate data points based on timestamps
    /// Uses CreatedAt as start time and LastModified as end time
    /// If times are equal, assumes 8 hours between start and end
    /// Range: 1 to 10 (raw mood values)
    /// </summary>
    RawData
}