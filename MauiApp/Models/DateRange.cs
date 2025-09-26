using System.ComponentModel;

namespace WorkMood.MauiApp.Models;

/// <summary>
/// Represents selectable date ranges for mood data visualization
/// </summary>
public enum DateRange
{
    [Description("Last 7 Days")]
    Last7Days,
    
    [Description("Last 14 Days")]
    Last14Days,
    
    [Description("Last Month")]
    LastMonth,
    
    [Description("Last 3 Months")]
    Last3Months,
    
    [Description("Last 6 Months")]
    Last6Months,
    
    [Description("Last Year")]
    LastYear,
    
    [Description("Last 2 Years")]
    Last2Years,
    
    [Description("Last 3 Years")]
    Last3Years
}

/// <summary>
/// Extension methods for DateRange enum
/// </summary>
public static class DateRangeExtensions
{
    /// <summary>
    /// Gets the display name for the date range
    /// </summary>
    public static string GetDisplayName(this DateRange dateRange)
    {
        var field = dateRange.GetType().GetField(dateRange.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? dateRange.ToString();
    }
    
    /// <summary>
    /// Gets the start date for the given date range relative to today
    /// </summary>
    public static DateOnly GetStartDate(this DateRange dateRange)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        return dateRange switch
        {
            DateRange.Last7Days => today.AddDays(-7),
            DateRange.Last14Days => today.AddDays(-14),
            DateRange.LastMonth => today.AddMonths(-1),
            DateRange.Last3Months => today.AddMonths(-3),
            DateRange.Last6Months => today.AddMonths(-6),
            DateRange.LastYear => today.AddYears(-1),
            DateRange.Last2Years => today.AddYears(-2),
            DateRange.Last3Years => today.AddYears(-3),
            _ => today.AddDays(-7)
        };
    }
    
    /// <summary>
    /// Gets the end date for the given date range (always today)
    /// </summary>
    public static DateOnly GetEndDate(this DateRange dateRange)
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }
}