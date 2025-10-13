using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Models;

/// <summary>
/// Contains complete graph data including data points and metadata for rendering
/// </summary>
public class GraphData
{
    /// <summary>
    /// The data points to be plotted on the graph
    /// </summary>
    public IEnumerable<FilledGraphDataPoint> DataPoints { get; init; } = Enumerable.Empty<FilledGraphDataPoint>();

    /// <summary>
    /// The title for the graph based on the graph mode
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The Y-axis range (minimum and maximum values) for the graph
    /// </summary>
    public AxisRange YAxisRange { get; init; } = new(0, 10);

    /// <summary>
    /// The Y-axis value where the centerline should be drawn (if applicable)
    /// </summary>
    public float? CenterLineValue { get; init; }

    /// <summary>
    /// The Y-axis label for the graph
    /// </summary>
    public string YAxisLabel { get; init; } = string.Empty;

    /// <summary>
    /// The X-axis label for the graph
    /// </summary>
    public string XAxisLabel { get; init; } = "Time";

    /// <summary>
    /// Indicates whether this graph represents raw data points (affects rendering style)
    /// </summary>
    public bool IsRawData { get; init; }

    /// <summary>
    /// The step size for Y-axis labels (how often to show labels)
    /// </summary>
    public int YAxisLabelStep { get; init; } = 3;

    /// <summary>
    /// Description of what the data represents
    /// </summary>
    public string Description { get; init; } = string.Empty;
}