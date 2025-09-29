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
    /// Gets the start date for the given date range relative to yesterday
    /// </summary>
    public static DateOnly GetStartDate(this DateRange dateRange)
    {
        var yesterday = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        
        return dateRange switch
        {
            DateRange.Last7Days => yesterday.AddDays(-6), // Yesterday + 6 days before = 7 total days
            DateRange.Last14Days => yesterday.AddDays(-13), // Yesterday + 13 days before = 14 total days
            DateRange.LastMonth => yesterday.AddMonths(-1).AddDays(1), // Start from yesterday, go back 1 month
            DateRange.Last3Months => yesterday.AddMonths(-3).AddDays(1), // Start from yesterday, go back 3 months
            DateRange.Last6Months => yesterday.AddMonths(-6).AddDays(1), // Start from yesterday, go back 6 months
            DateRange.LastYear => yesterday.AddYears(-1).AddDays(1), // Start from yesterday, go back 1 year
            DateRange.Last2Years => yesterday.AddYears(-2).AddDays(1), // Start from yesterday, go back 2 years
            DateRange.Last3Years => yesterday.AddYears(-3).AddDays(1), // Start from yesterday, go back 3 years
            _ => yesterday.AddDays(-6) // Default to last 7 days
        };
    }
    
    /// <summary>
    /// Gets the end date for the given date range (always yesterday - excludes today)
    /// </summary>
    public static DateOnly GetEndDate(this DateRange dateRange)
    {
        return DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
    }
}