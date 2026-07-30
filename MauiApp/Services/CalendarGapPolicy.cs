namespace WorkMood.MauiApp.Services;

/// <summary>
/// Centralized calendar-gap policy used by graph display paths.
/// Owns weekend compression rules and segment break decisions.
/// </summary>
public static class CalendarGapPolicy
{
    public static bool IsWeekend(DayOfWeek dayOfWeek)
    {
        return dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    public static bool ShouldConsumeCompressedDaySlot(DateOnly date, IReadOnlySet<DateOnly> recordedDates)
    {
        ArgumentNullException.ThrowIfNull(recordedDates);

        return !IsWeekend(date.DayOfWeek) || recordedDates.Contains(date);
    }

    public static bool ShouldBreakWhenWeekdaysMissing(DateOnly previousDate, DateOnly currentDate)
    {
        if (currentDate <= previousDate)
        {
            return false;
        }

        for (var date = previousDate.AddDays(1); date < currentDate; date = date.AddDays(1))
        {
            if (!IsWeekend(date.DayOfWeek))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ShouldBreakForRecordedSeries(
        DateOnly previousDate,
        DateOnly currentDate,
        IReadOnlySet<DateOnly> recordedDates)
    {
        ArgumentNullException.ThrowIfNull(recordedDates);

        if (currentDate <= previousDate)
        {
            return false;
        }

        for (var date = previousDate.AddDays(1); date < currentDate; date = date.AddDays(1))
        {
            if (ShouldConsumeCompressedDaySlot(date, recordedDates))
            {
                return true;
            }
        }

        return false;
    }
}
