namespace WorkMood.MauiApp.Models;

/// <summary>
/// Configuration options for graph display and rendering.
/// Encapsulates all visual settings to simplify graph service interfaces.
/// </summary>
public class GraphDisplayOptions
{
    /// <summary>
    /// Whether to show individual data points on the graph
    /// </summary>
    public bool ShowDataPoints { get; set; } = true;

    /// <summary>
    /// Whether to show axes and grid lines
    /// </summary>
    public bool ShowAxesAndGrid { get; set; } = true;

    /// <summary>
    /// Whether to show the graph title
    /// </summary>
    public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// Whether to show the trend line
    /// </summary>
    public bool ShowTrendLine { get; set; } = false;

    /// <summary>
    /// Color for the graph line and data points
    /// </summary>
    public Color LineColor { get; set; } = Colors.Blue;

    /// <summary>
    /// Width of the graph in pixels
    /// </summary>
    public int Width { get; set; } = 800;

    /// <summary>
    /// Height of the graph in pixels
    /// </summary>
    public int Height { get; set; } = 600;

    /// <summary>
    /// Creates GraphDisplayOptions with default values
    /// </summary>
    public GraphDisplayOptions()
    {
    }

    /// <summary>
    /// Creates GraphDisplayOptions with specified line color and default other values
    /// </summary>
    /// <param name="lineColor">Color for the graph line and data points</param>
    public GraphDisplayOptions(Color lineColor)
    {
        LineColor = lineColor;
    }

    /// <summary>
    /// Creates GraphDisplayOptions with full customization
    /// </summary>
    /// <param name="showDataPoints">Whether to show individual data points</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="lineColor">Color for the graph line and data points</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    public GraphDisplayOptions(bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        ShowDataPoints = showDataPoints;
        ShowAxesAndGrid = showAxesAndGrid;
        ShowTitle = showTitle;
        ShowTrendLine = showTrendLine;
        LineColor = lineColor;
        Width = width;
        Height = height;
    }
}