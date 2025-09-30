namespace WorkMood.MauiApp.Shims;

public class DateShim : IDateShim
{
    public DateOnly GetDate(int yearOffset)
    {
        return DateOnly.FromDateTime(DateTime.Today.AddYears(yearOffset));
    }

    public DateTime GetToday()
    {
        return DateTime.Today;
    }

    public DateOnly GetTodayDate()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    public DateTime Now() => DateTime.Now;
}
