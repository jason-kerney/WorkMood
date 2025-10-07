using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Tests.TestHelpers;

/// <summary>
/// Test helper implementation of IDateShim that uses a fixed DateTime for all operations
/// </summary>
public class FakeDateShim : IDateShim
{
    private readonly DateTime _fixedDateTime;

    /// <summary>
    /// Initializes a new instance of FakeDateShim with the specified fixed DateTime
    /// </summary>
    /// <param name="fixedDateTime">The DateTime to use for all date operations</param>
    public FakeDateShim(DateTime fixedDateTime)
    {
        _fixedDateTime = fixedDateTime;
    }

    /// <summary>
    /// Initializes a new instance of FakeDateShim with the specified DateOnly at noon
    /// </summary>
    /// <param name="fixedDate">The DateOnly to use, converted to DateTime at 12:00:00</param>
    public FakeDateShim(DateOnly fixedDate) : this(fixedDate.ToDateTime(new TimeOnly(12, 0, 0)))
    {
    }

    /// <summary>
    /// Returns a DateOnly offset by the specified number of years from the fixed date
    /// </summary>
    /// <param name="yearOffset">Number of years to offset (can be negative)</param>
    /// <returns>DateOnly offset by the specified years</returns>
    public DateOnly GetDate(int yearOffset)
    {
        return DateOnly.FromDateTime(_fixedDateTime.AddYears(yearOffset));
    }

    /// <summary>
    /// Returns the fixed date as DateOnly
    /// </summary>
    /// <returns>The fixed date as DateOnly</returns>
    public DateOnly GetTodayDate()
    {
        return DateOnly.FromDateTime(_fixedDateTime);
    }

    /// <summary>
    /// Returns the fixed DateTime
    /// </summary>
    /// <returns>The fixed DateTime</returns>
    public DateTime GetToday()
    {
        return _fixedDateTime;
    }

    /// <summary>
    /// Returns the fixed DateTime (same as GetToday for this fake implementation)
    /// </summary>
    /// <returns>The fixed DateTime</returns>
    public DateTime Now()
    {
        return _fixedDateTime;
    }
}