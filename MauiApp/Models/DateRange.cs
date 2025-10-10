using System.ComponentModel;
using WorkMood.MauiApp.Shims;

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

public class DateRangeInfo
{
    public DateRangeInfo(DateRange dateRange, IDateShim dateShim) 
        : this(dateRange, dateShim.GetTodayDate().AddDays(-1))
    {
    }

    public DateRangeInfo(DateRange dateRange, DateOnly endDay)
    {
        DateRange = dateRange;

        var field = dateRange.GetType().GetField(dateRange.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        DisplayName = attribute?.Description ?? dateRange.ToString();

        StartDate = dateRange switch
        {
            DateRange.Last7Days => endDay.AddDays(-6), // Yesterday + 6 days before = 7 total days
            DateRange.Last14Days => endDay.AddDays(-13), // Yesterday + 13 days before = 14 total days
            DateRange.LastMonth => endDay.AddMonths(-1).AddDays(1), // Start from yesterday, go back 1 month
            DateRange.Last3Months => endDay.AddMonths(-3).AddDays(1), // Start from yesterday, go back 3 months
            DateRange.Last6Months => endDay.AddMonths(-6).AddDays(1), // Start from yesterday, go back 6 months
            DateRange.LastYear => endDay.AddYears(-1).AddDays(1), // Start from yesterday, go back 1 year
            DateRange.Last2Years => endDay.AddYears(-2).AddDays(1), // Start from yesterday, go back 2 years
            DateRange.Last3Years => endDay.AddYears(-3).AddDays(1), // Start from yesterday, go back 3 years
            _ => endDay.AddDays(-6) // Default to last 7 days
        };

        EndDate = endDay;
    }

    public DateRange DateRange { get; private set; }

    public string DisplayName { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }
}