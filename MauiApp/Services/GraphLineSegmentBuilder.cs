namespace WorkMood.MauiApp.Services;

/// <summary>
/// Groups chronologically ordered graph points into calendar-contiguous segments so renderers
/// can draw connecting lines only between dates that are not separated by a missing calendar day.
/// </summary>
/// <remarks>
/// This helper is pure, stateless, and rendering-framework-agnostic: it never inspects or mutates
/// x/y (or any other) values on the points it groups, and it trusts the caller's ordering rather
/// than re-sorting. It is shared by both <see cref="WorkMood.MauiApp.Graphics.EnhancedLineGraphDrawable"/>
/// (discrete 14-slot layout) and <see cref="LineGraphGenerator"/> (continuous time-proportional layout)
/// so gap semantics cannot drift between the two renderers.
/// </remarks>
public static class GraphLineSegmentBuilder
{
    /// <summary>
    /// Splits <paramref name="orderedPoints"/> into segments such that every pair of adjacent
    /// points within a segment is at most one calendar day apart. A gap of more than one calendar
    /// day between two consecutive points starts a new segment. Points that share the same
    /// calendar date always stay together in the same segment.
    /// </summary>
    /// <typeparam name="T">The renderer-specific point type; never inspected beyond its date.</typeparam>
    /// <param name="orderedPoints">Points already ordered chronologically by the caller.</param>
    /// <param name="dateSelector">Projects the calendar date used for gap detection from a point.</param>
    /// <returns>
    /// An ordered list of segments. Each segment is an ordered, contiguous run of the input points
    /// with no missing calendar day between adjacent points. Empty input yields zero segments; a
    /// single point yields one single-point segment.
    /// </returns>
    public static IReadOnlyList<IReadOnlyList<T>> BuildSegments<T>(
        IReadOnlyList<T> orderedPoints,
        Func<T, DateOnly> dateSelector)
    {
        ArgumentNullException.ThrowIfNull(orderedPoints);
        ArgumentNullException.ThrowIfNull(dateSelector);

        var segments = new List<IReadOnlyList<T>>();

        if (orderedPoints.Count == 0)
        {
            return segments;
        }

        var currentSegment = new List<T> { orderedPoints[0] };

        for (var i = 1; i < orderedPoints.Count; i++)
        {
            var previousDate = dateSelector(orderedPoints[i - 1]);
            var currentDate = dateSelector(orderedPoints[i]);
            var calendarDayGap = currentDate.DayNumber - previousDate.DayNumber;

            if (calendarDayGap > 1)
            {
                segments.Add(currentSegment);
                currentSegment = new List<T>();
            }

            currentSegment.Add(orderedPoints[i]);
        }

        segments.Add(currentSegment);
        return segments;
    }
}
