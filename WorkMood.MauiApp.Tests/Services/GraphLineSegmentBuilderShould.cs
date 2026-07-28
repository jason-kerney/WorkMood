using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests for <see cref="GraphLineSegmentBuilder"/> - the shared calendar-gap segmentation
/// helper reused by both the 2-week drawable and the image generator.
/// </summary>
public class GraphLineSegmentBuilderShould
{
    private sealed class TestPoint
    {
        public TestPoint(DateOnly date, float x, float y)
        {
            Date = date;
            X = x;
            Y = y;
        }

        public DateOnly Date { get; }
        public float X { get; }
        public float Y { get; }
    }

    [Fact]
    public void ReturnNoSegments_ForEmptyInput()
    {
        var result = GraphLineSegmentBuilder.BuildSegments(Array.Empty<TestPoint>(), p => p.Date);

        Assert.Empty(result);
    }

    [Fact]
    public void ReturnOneSegment_ForSinglePoint()
    {
        var point = new TestPoint(new DateOnly(2025, 1, 1), 10f, 20f);

        var result = GraphLineSegmentBuilder.BuildSegments(new[] { point }, p => p.Date);

        var segment = Assert.Single(result);
        var only = Assert.Single(segment);
        Assert.Same(point, only);
    }

    [Fact]
    public void ReturnOneSegment_ForConsecutiveCalendarDays()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 1), 0f, 0f),
            new TestPoint(new DateOnly(2025, 1, 2), 1f, 1f),
            new TestPoint(new DateOnly(2025, 1, 3), 2f, 2f)
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        var segment = Assert.Single(result);
        Assert.Equal(3, segment.Count);
    }

    [Fact]
    public void StartNewSegment_WhenACalendarDayIsSkipped()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 1), 0f, 0f), // day 1
            new TestPoint(new DateOnly(2025, 1, 2), 1f, 1f), // day 2 - contiguous with day 1
            new TestPoint(new DateOnly(2025, 1, 4), 2f, 2f), // day 4 - skips day 3
            new TestPoint(new DateOnly(2025, 1, 5), 3f, 3f)  // day 5 - contiguous with day 4
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(2, result[1].Count);
        Assert.Same(points[1], result[0][1]);
        Assert.Same(points[2], result[1][0]);
    }

    [Fact]
    public void StartANewSegmentForEachGap_WithMultipleNonAdjacentGaps()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 1), 0f, 0f),
            new TestPoint(new DateOnly(2025, 1, 3), 1f, 1f), // gap
            new TestPoint(new DateOnly(2025, 1, 4), 2f, 2f),
            new TestPoint(new DateOnly(2025, 1, 8), 3f, 3f), // gap
            new TestPoint(new DateOnly(2025, 1, 9), 4f, 4f)
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        Assert.Equal(3, result.Count);
        Assert.Single(result[0]);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(2, result[2].Count);
    }

    [Fact]
    public void KeepSameCalendarDayPointsInOneSegment_ForRawDataMultiplePointsPerDay()
    {
        var day = new DateOnly(2025, 1, 5);
        var points = new[]
        {
            new TestPoint(day, 0f, 0f), // start-of-work
            new TestPoint(day, 1f, 1f)  // end-of-work, same calendar day
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        var segment = Assert.Single(result);
        Assert.Equal(2, segment.Count);
    }

    [Fact]
    public void BreakSegment_WhenRawDataTimestampsSkipACalendarDay()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 5), 0f, 0f),
            new TestPoint(new DateOnly(2025, 1, 5), 1f, 1f),
            new TestPoint(new DateOnly(2025, 1, 7), 2f, 2f) // skips Jan 6
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Count);
        Assert.Single(result[1]);
    }

    [Fact]
    public void NotMutateOrRecomputeCoordinates()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 1), 123.5f, -45.25f),
            new TestPoint(new DateOnly(2025, 1, 5), 987.25f, 6.75f)
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        Assert.Equal(2, result.Count);
        Assert.Equal(123.5f, result[0][0].X);
        Assert.Equal(-45.25f, result[0][0].Y);
        Assert.Equal(987.25f, result[1][0].X);
        Assert.Equal(6.75f, result[1][0].Y);
    }

    [Fact]
    public void PreserveStableFirstAndLastPointIdentity_PerSegment()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 1), 0f, 0f),
            new TestPoint(new DateOnly(2025, 1, 2), 1f, 1f),
            new TestPoint(new DateOnly(2025, 1, 3), 2f, 2f)
        };

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date);

        var segment = Assert.Single(result);
        Assert.Same(points[0], segment[0]);
        Assert.Same(points[^1], segment[^1]);
    }

    [Fact]
    public void KeepFridayToMondayInSameSegment_WhenOnlyWeekendDaysAreMissing_AndPolicyAllowsIt()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 3), 0f, 0f),  // Friday
            new TestPoint(new DateOnly(2025, 1, 6), 1f, 1f)   // Monday
        };

        static bool shouldBreak(DateOnly previousDate, DateOnly currentDate)
        {
            for (var date = previousDate.AddDays(1); date < currentDate; date = date.AddDays(1))
            {
                var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                if (!isWeekend)
                {
                    return true;
                }
            }

            return false;
        }

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date, shouldBreak);

        var segment = Assert.Single(result);
        Assert.Equal(2, segment.Count);
    }

    [Fact]
    public void StillBreakWhenAWeekdayIsMissing_BeforeWeekendGap_EvenWithWeekendAwarePolicy()
    {
        var points = new[]
        {
            new TestPoint(new DateOnly(2025, 1, 2), 0f, 0f),  // Thursday
            new TestPoint(new DateOnly(2025, 1, 6), 1f, 1f)   // Monday (Friday missing)
        };

        static bool shouldBreak(DateOnly previousDate, DateOnly currentDate)
        {
            for (var date = previousDate.AddDays(1); date < currentDate; date = date.AddDays(1))
            {
                var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                if (!isWeekend)
                {
                    return true;
                }
            }

            return false;
        }

        var result = GraphLineSegmentBuilder.BuildSegments(points, p => p.Date, shouldBreak);

        Assert.Equal(2, result.Count);
        Assert.Single(result[0]);
        Assert.Single(result[1]);
    }
}
