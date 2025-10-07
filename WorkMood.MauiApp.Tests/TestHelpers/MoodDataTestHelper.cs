using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests.TestHelpers;

/// <summary>
/// Test helper class for generating fake mood data for testing purposes
/// </summary>
public static class MoodDataTestHelper
{
    /// <summary>
    /// Generates a list of fake mood entries starting from the specified date
    /// </summary>
    /// <param name="startDate">The starting date for the first mood entry</param>
    /// <param name="moods">Array of tuples containing (startMood, endMood) values for each day</param>
    /// <returns>A tuple containing the end date (day after last entry) and the list of mood entries</returns>
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetFakeData(DateOnly startDate, params (int? startMood, int? endMood)[] moods)
    {
        return GetFakeData(startDate, false, moods);
    }

    /// <summary>
    /// Generates a list of fake mood entries starting from the specified date
    /// </summary>
    /// <param name="startDate">The starting date for the first mood entry</param>
    /// <param name="skipWeekends">If true, weekends (Saturday and Sunday) will be skipped but still count against the total number of entries</param>
    /// <param name="moods">Array of tuples containing (startMood, endMood) values for each day</param>
    /// <returns>A tuple containing the end date (day after last entry) and the list of mood entries</returns>
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetFakeData(DateOnly startDate, bool skipWeekends, params (int? startMood, int? endMood)[] moods)
    {
        var entries = new List<MoodEntry>();
        var date = startDate;

        for (int i = 0; i < moods.Length; i++)
        {
            // If we're skipping weekends and this is a weekend, advance the date but still use the mood data
            if (!skipWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
            {
                entries.Add(new MoodEntry
                {
                    Date = date,
                    StartOfWork = moods[i].startMood,
                    EndOfWork = moods[i].endMood,
                    CreatedAt = date.ToDateTime(new TimeOnly(8, 0, 0)),
                    LastModified = date.ToDateTime(new TimeOnly(17, 0, 0))
                });
            }
            date = date.AddDays(1);
        }

        return (date, entries);
    }

    /// <summary>
    /// Generates a list of fake mood entries with randomly generated mood values
    /// </summary>
    /// <param name="startDate">The starting date for the first mood entry</param>
    /// <param name="seed">Seed for the random number generator to ensure reproducible results</param>
    /// <param name="count">Number of mood entries to generate</param>
    /// <param name="minMood">Minimum mood value (default: 1)</param>
    /// <param name="maxMood">Maximum mood value (default: 10)</param>
    /// <returns>A tuple containing the end date (day after last entry) and the list of mood entries</returns>
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetRandomFakeData(DateOnly startDate, int seed, int count)
    {
        return GetRandomFakeData(startDate, false, seed, count);
    }
    
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetRandomFakeData(DateOnly startDate, bool skippWeekends, int seed, int count)
    {
        const int minMood = 1, maxMood = 10;

        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var random = new Random(seed);
        var moodPairs = new (int? startMood, int? endMood)[count];

        for (int i = 0; i < count; i++)
        {
            var startMood = random.Next(minMood, maxMood + 1);
            var endMood = random.Next(minMood, maxMood + 1);
            moodPairs[i] = (startMood, endMood);
        }

        return GetFakeData(startDate, skippWeekends, moodPairs);
    }

    /// <summary>
    /// Generates a list of fake mood entries with only start-of-work moods (EndOfWork will be null)
    /// </summary>
    /// <param name="startDate">The starting date for the first mood entry</param>
    /// <param name="startMoods">Array of start mood values for each day</param>
    /// <returns>A tuple containing the end date (day after last entry) and the list of mood entries</returns>
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetFakeStartOnlyData(DateOnly startDate, params int?[] startMoods)
    {
        return GetFakeStartOnlyData(startDate, false, startMoods);
    }

    public static (DateOnly EndDate, List<MoodEntry> Entries) GetFakeStartOnlyData(DateOnly startDate, bool skipWeekends, params int?[] startMoods)
    {
        var entries = new List<MoodEntry>();
        var date = startDate;

        for (int i = 0; i < startMoods.Length; i++)
        {
            if (!skipWeekends || (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday))
            {
                entries.Add(new MoodEntry
                {
                    Date = date,
                    StartOfWork = startMoods[i],
                    EndOfWork = null, // Only start moods
                    CreatedAt = date.ToDateTime(new TimeOnly(8, 0, 0)),
                    LastModified = date.ToDateTime(new TimeOnly(8, 0, 0)) // Same as created since no end mood
                });
            }
            date = date.AddDays(1);
        }

        return (date, entries);
    }

    /// <summary>
    /// Generates a list of fake mood entries with randomly generated start-of-work moods only (EndOfWork will be null)
    /// </summary>
    /// <param name="startDate">The starting date for the first mood entry</param>
    /// <param name="seed">Seed for the random number generator to ensure reproducible results</param>
    /// <param name="count">Number of mood entries to generate</param>
    /// <returns>A tuple containing the end date (day after last entry) and the list of mood entries</returns>
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetRandomFakeStartOnlyData(DateOnly startDate, int seed, int count)
    {
        return GetRandomFakeStartOnlyData(startDate, false, seed, count);
    }
    public static (DateOnly EndDate, List<MoodEntry> Entries) GetRandomFakeStartOnlyData(DateOnly startDate, bool skipWeekends, int seed, int count)
    {
        const int minMood = 1, maxMood = 10;

        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        var random = new Random(seed);
        var startMoods = new int?[count];

        for (int i = 0; i < count; i++)
        {
            startMoods[i] = random.Next(minMood, maxMood + 1);
        }

        return GetFakeStartOnlyData(startDate, skipWeekends, startMoods);
    }

    /// <summary>
    /// Converts a collection of MoodEntry objects to RawMoodDataPoint objects
    /// Each MoodEntry creates two RawMoodDataPoints: one for StartOfWork and one for EndOfWork
    /// </summary>
    /// <param name="moodEntries">Collection of mood entries to convert</param>
    /// <returns>Collection of raw mood data points</returns>
    public static IEnumerable<RawMoodDataPoint> ConvertToRawMoodDataPoints(IEnumerable<MoodEntry> moodEntries)
    {
        return moodEntries.SelectMany(dp =>
        {
            int startOfWork = dp.StartOfWork.GetValueOrDefault();
            return new[]
            {
                new RawMoodDataPoint(dp.CreatedAt, startOfWork, MoodType.StartOfWork, dp.Date),
                new RawMoodDataPoint(dp.LastModified, dp.EndOfWork.GetValueOrDefault(startOfWork), MoodType.EndOfWork, dp.Date)
            };
        });
    }
}