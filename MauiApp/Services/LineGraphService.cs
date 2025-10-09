using SkiaSharp;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

// Sha: 6d7b2feb687d49cec0f8c2b736b310c7919e1c6d

/// <summary>
/// Implementation of line graph service using SkiaSharp for rendering
/// </summary>
public class LineGraphService(IDrawShimFactory drawShimFactory, IFileShimFactory fileShimFactory, IGraphDataTransformer? dataTransformer = null) : ILineGraphService
{
    private const int MinYValue = -9;
    private const int MaxYValue = 9;
    private const int Padding = 60;
    
    private readonly IGraphDataTransformer _dataTransformer = dataTransformer ?? new GraphDataTransformer();
    
    /// <summary>
    /// Initializes a new instance of the LineGraphService with default factory implementations.
    /// </summary>
    public LineGraphService() : this(new DrawShimFactory(), new FileShimFactory()) { }

    /// <summary>
    /// Creates a bitmap with canvas, executes the drawing action, and returns the encoded PNG bytes
    /// </summary>
    private async Task<byte[]> GenerateImageAsync(int width, int height, Func<ICanvasShim, Task> drawAction)
    {
        using var bitmap = drawShimFactory.BitmapFromDimensions(width, height);
        using var canvas = drawShimFactory.CanvasFromBitmap(bitmap);
        
        await drawAction(canvas);
        
        using var image = drawShimFactory.ImageFromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>
    /// Sets up the canvas background - either white or custom image
    /// </summary>
    private void SetupCanvasBackground(ICanvasShim canvas, string? backgroundImagePath, int width, int height)
    {
        if (!string.IsNullOrEmpty(backgroundImagePath))
        {
            var fileShim = fileShimFactory.Create();
            if (fileShim.Exists(backgroundImagePath))
            {
                using var backgroundBitmap = drawShimFactory.DecodeBitmapFromFile(backgroundImagePath);
                if (backgroundBitmap != null)
                {
                    canvas.DrawBitmap(backgroundBitmap, new SKRect(0, 0, width, height));
                    return;
                }
            }
        }
        
        // Fallback to white background
        canvas.Clear(drawShimFactory.Colors.White);
    }

    /// <summary>
    /// Saves generated image bytes to the specified file path
    /// </summary>
    private async Task SaveImageDataAsync(byte[] imageData, string filePath)
    {
        var fileShim = fileShimFactory.Create();
        await fileShim.WriteAllBytesAsync(filePath, imageData);
    }

    /// <summary>
    /// Draws grid lines with the specified Y range and vertical line configuration
    /// </summary>
    private void DrawGrid(ICanvasShim canvas, SKRect area, int minY, int maxY, int verticalLineCount = 10, int verticalLineSpacing = 80)
    {
        using var gridPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = drawShimFactory.PathEffects.CreateDash([5, 5], 0)
        });

        // Horizontal grid lines
        var yRange = maxY - minY;
        var yStep = area.Height / yRange;
        for (int i = minY + 1; i < maxY; i++)
        {
            var y = area.Bottom - ((i - minY) * yStep);
            canvas.DrawLine(area.Left, y, area.Right, y, gridPaint);
        }

        // Vertical grid lines
        if (area.Width > 200)
        {
            var verticalLines = Math.Min(verticalLineCount, (int)(area.Width / verticalLineSpacing));
            var xStep = area.Width / verticalLines;
            for (int i = 1; i < verticalLines; i++)
            {
                var x = area.Left + (i * xStep);
                canvas.DrawLine(x, area.Top, x, area.Bottom, gridPaint);
            }
        }
    }

    /// <summary>
    /// Draws basic X and Y axes
    /// </summary>
    private void DrawBasicAxes(ICanvasShim canvas, SKRect area)
    {
        using var axisPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        });

        // Y-axis
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);

        // X-axis
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);
    }

    // New overloads with GraphMode support

    /// <summary>
    /// Generates a line graph PNG image from mood entry data with the specified graph mode and white background.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact or Average)</param>
    /// <param name="lineColor">Color for the graph line and data points</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>PNG image data as byte array</returns>
    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, GraphMode graphMode, Color lineColor, int width = 800, int height = 600)
    {
        return await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, graphMode, "", lineColor, width, height);
    }

    /// <summary>
    /// Generates a line graph PNG image from mood entry data with the specified graph mode and custom background image.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact or Average)</param>
    /// <param name="backgroundImagePath">Path to the custom background image file, or empty string for white background</param>
    /// <param name="lineColor">Color for the graph line and data points</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>PNG image data as byte array</returns>
    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        // Filter entries based on graph mode and date range
        var filteredEntries = FilterEntriesForGraphMode(moodEntries, graphMode)
            .Where(e => e.Date >= dateRange.StartDate && e.Date <= dateRange.EndDate)
            .OrderBy(e => e.Date)
            .ToList();

        return await GenerateImageAsync(width, height, async canvas =>
        {
            SetupCanvasBackground(canvas, backgroundImagePath, width, height);
            var hasCustomBackground = !string.IsNullOrEmpty(backgroundImagePath);

            await Task.Run(() => DrawGraphForMode(canvas, filteredEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, graphMode, !hasCustomBackground));
        });
    }

    private void DrawBackground(ICanvasShim canvas, SKRect area)
    {
        using var backgroundPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.White,
            Style = SKPaintStyle.Fill
        });
        canvas.DrawRect(area, backgroundPaint);
    }

    private void DrawXAxisLabelsFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        });

        // Show labels for the requested date range, not just the data points
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        // Show at most 8 date labels distributed across the entire requested range
        var labelCount = Math.Min(8, Math.Max(2, totalDays / 30)); // At least 2 labels, roughly monthly intervals

        for (int i = 0; i <= labelCount; i++)
        {
            var proportionalPosition = (float)i / labelCount;
            var x = area.Left + (proportionalPosition * area.Width);

            // Calculate the date for this position
            var daysFromStart = (int)(proportionalPosition * totalDays);
            var labelDate = requestedStartDate.AddDays(daysFromStart);
            var dateText = labelDate.ToString("MM/dd");

            canvas.DrawText(dateText, x, area.Bottom + 20, labelPaint);
        }

        // Also show data point dates if they don't overlap too much and showDataPoints is true
        if (showDataPoints && dataPoints.Count > 0 && dataPoints.Count <= 10)
        {
            using var dataLabelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
            {
                Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180), 255), // Match data point color
                TextSize = 8,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            });

            foreach (var dataPoint in dataPoints)
            {
                var dataDate = DateOnly.FromDateTime(dataPoint.Timestamp);
                var daysFromStart = dataDate.DayNumber - requestedStartDate.DayNumber;
                var proportionalPosition = (float)daysFromStart / totalDays;
                var x = area.Left + (proportionalPosition * area.Width);

                canvas.DrawText("●", x, area.Bottom + 35, dataLabelPaint);
            }
        }
    }

    private void DrawXAxisLabels(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        // For this legacy method, we'll use Impact mode as the default since this method doesn't specify a mode
        var graphDataPoints = _dataTransformer.TransformMoodEntries(entries, GraphMode.Impact).ToList();
        DrawXAxisLabelsFromGraphDataPoints(canvas, area, graphDataPoints, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
    }

    private void DrawNoDataMessage(ICanvasShim canvas, SKRect area)
    {
        using var messagePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Gray,
            TextSize = 14,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        });

        var centerX = area.Left + area.Width / 2;
        var centerY = area.Top + area.Height / 2;

        canvas.DrawText("No mood data available for the selected period", centerX, centerY, messagePaint);
    }

    // New helper methods for GraphMode support

    /// <summary>
    /// Filters mood entries based on the selected graph mode
    /// </summary>
    private IEnumerable<MoodEntry> FilterEntriesForGraphMode(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode)
    {
        // Use the data transformer's filtering logic by getting the transformed data and finding matching original entries
        var transformedData = _dataTransformer.TransformMoodEntries(moodEntries, graphMode).ToList();
        
        // Since the transformer filters, match back to original entries based on date
        var transformedDates = transformedData.Select(d => DateOnly.FromDateTime(d.Timestamp)).ToHashSet();
        
        return moodEntries.Where(entry => transformedDates.Contains(entry.Date));
    }

    /// <summary>
    /// Transforms mood entries to graph data points using the data transformer
    /// </summary>
    private IEnumerable<GraphDataPoint> TransformMoodEntriesToGraphDataPoints(IEnumerable<MoodEntry> moodEntries, GraphMode graphMode)
    {
        return _dataTransformer.TransformMoodEntries(moodEntries, graphMode);
    }

    /// <summary>
    /// Draws graph with mode-specific logic for data extraction using GraphDataPoint format
    /// </summary>
    private void DrawGraphForModeFromGraphDataPoints(ICanvasShim canvas, List<GraphDataPoint> dataPoints, List<MoodEntry> originalEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color lineColor, GraphMode graphMode, bool drawWhiteBackground = true)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);

        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.StartDate;
        var requestedEndDate = dateRange.EndDate;

        // Conditionally draw background
        if (drawWhiteBackground)
        {
            DrawBackground(canvas, graphArea);
        }

        // Conditionally draw grid and axes with mode-specific Y range
        if (showAxesAndGrid)
        {
            DrawGridForMode(canvas, graphArea, graphMode);
            DrawAxesForMode(canvas, graphArea, graphMode);
        }

        if (dataPoints.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw data line and optionally points with proportional positioning
        DrawDataLineFromGraphDataPoints(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode);
        if (showDataPoints)
        {
            DrawDataPointsFromGraphDataPoints(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode);
        }

        // Draw trend line if requested
        if (showTrendLine)
        {
            DrawTrendLineFromGraphDataPoints(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode, drawWhiteBackground);
        }

        // Conditionally draw labels
        if (showAxesAndGrid)
        {
            DrawYAxisLabelsForMode(canvas, graphArea, graphMode);
            DrawXAxisLabelsFromGraphDataPoints(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
        }

        // Conditionally draw title
        if (showTitle)
        {
            DrawTitleForMode(canvas, width, graphMode);
        }
    }

    /// <summary>
    /// Draws graph with mode-specific logic for data extraction
    /// </summary>
    private void DrawGraphForMode(ICanvasShim canvas, List<MoodEntry> entries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color lineColor, GraphMode graphMode, bool drawWhiteBackground = true)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformMoodEntries(entries, graphMode).ToList();
        DrawGraphForModeFromGraphDataPoints(canvas, graphDataPoints, entries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, graphMode, drawWhiteBackground);
    }

    private void DrawGridForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        var (minY, maxY) = GetYRangeForMode(graphMode);
        DrawGrid(canvas, area, minY, maxY);
    }

    private void DrawAxesForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        DrawBasicAxes(canvas, area);

        // Zero line (horizontal line at y=0)
        var (minY, maxY) = GetYRangeForMode(graphMode);
        var zeroY = area.Bottom - ((0 - minY) * area.Height / (maxY - minY));
        using var zeroLinePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4
        });
        canvas.DrawLine(area.Left, zeroY, area.Right, zeroY, zeroLinePaint);
    }

    private void DrawDataLineForMode(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        var dataPoints = _dataTransformer.TransformMoodEntries(entries, graphMode).ToList();
        DrawDataLineFromGraphDataPoints(canvas, area, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode);
    }

    /// <summary>
    /// Draws data line using unified GraphDataPoint objects (refactored version)
    /// </summary>
    private void DrawDataLineFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode)
    {
        if (dataPoints.Count < 2) return;

        using var linePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255), 255),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        });

        using var path = new SKPath();

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;
        var (minY, maxY) = GetYRangeForMode(graphMode);

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            var entryDate = DateOnly.FromDateTime(dataPoint.Timestamp);
            var daysFromStart = entryDate.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;
            var x = area.Left + (proportionalPosition * area.Width);

            var value = dataPoint.Value;
            var y = (float)(area.Bottom - ((value - minY) * area.Height / (maxY - minY)));

            if (i == 0)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }

        canvas.DrawPath(path, linePaint);
    }

    private void DrawDataPointsForMode(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        var dataPoints = _dataTransformer.TransformMoodEntries(entries, graphMode).ToList();
        DrawDataPointsFromGraphDataPoints(canvas, area, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode);
    }

    /// <summary>
    /// Draws data points using unified GraphDataPoint objects (refactored version)
    /// </summary>
    private void DrawDataPointsFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode)
    {
        using var pointPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180), 255),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;
        var (minY, maxY) = GetYRangeForMode(graphMode);

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            var entryDate = DateOnly.FromDateTime(dataPoint.Timestamp);
            var daysFromStart = entryDate.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;
            var x = area.Left + (proportionalPosition * area.Width);

            var value = dataPoint.Value;
            var y = (float)(area.Bottom - ((value - minY) * area.Height / (maxY - minY)));

            canvas.DrawCircle(x, y, 4, pointPaint);
        }
    }

    private void DrawYAxisLabelsForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        });

        var (minY, maxY) = GetYRangeForMode(graphMode);
        var yRange = maxY - minY;
        var yStep = area.Height / yRange;

        for (int i = minY; i <= maxY; i += 3) // Show every 3rd value to avoid crowding
        {
            var y = area.Bottom - ((i - minY) * yStep);
            canvas.DrawText(i.ToString(), area.Left - 10, y + 4, labelPaint);
        }
    }

    private void DrawXAxisLabelsForMode(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor, GraphMode graphMode)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformMoodEntries(entries, graphMode).ToList();
        DrawXAxisLabelsFromGraphDataPoints(canvas, area, graphDataPoints, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
    }

    private void DrawTitleForMode(ICanvasShim canvas, int width, GraphMode graphMode)
    {
        using var titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        var title = graphMode switch
        {
            GraphMode.Impact => "Mood Change Over Time",
            GraphMode.Average => "Average Mood Over Time",
            _ => "Mood Data Over Time"
        };

        canvas.DrawText(title, width / 2, 30, titlePaint);
    }

    private (int minY, int maxY) GetYRangeForMode(GraphMode graphMode)
    {
        return graphMode switch
        {
            GraphMode.Impact => (MinYValue, MaxYValue), // -9 to +9
            GraphMode.Average => (-5, 5), // -4 to +5 realistically, but -5 to +5 for nice round numbers
            GraphMode.RawData => (1, 10), // 1 to 10 (raw mood values)
            _ => (MinYValue, MaxYValue)
        };
    }

    private double GetValueForMode(MoodEntry entry, GraphMode graphMode)
    {
        return _dataTransformer.GetValueForMoodEntry(entry, graphMode);
    }

    /// <summary>
    /// Draws a trend line using linear regression for the mood data
    /// </summary>
    private void DrawTrendLineForMode(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode, bool drawWhiteBackground)
    {
        // Transform MoodEntry objects to GraphDataPoint objects and delegate to the refactored method
        var dataPoints = _dataTransformer.TransformMoodEntries(entries, graphMode).ToList();
        DrawTrendLineFromGraphDataPoints(canvas, area, dataPoints, requestedStartDate, requestedEndDate, lineColor, graphMode, drawWhiteBackground);
    }

    /// <summary>
    /// Draws trend line using unified GraphDataPoint objects (refactored version)
    /// </summary>
    private void DrawTrendLineFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode, bool drawWhiteBackground)
    {
        if (dataPoints.Count < 2) return;

        // Calculate linear regression
        var regressionPoints = new List<(double x, double y)>();
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;
        
        foreach (var dataPoint in dataPoints)
        {
            var entryDate = DateOnly.FromDateTime(dataPoint.Timestamp);
            var dayOffset = entryDate.DayNumber - requestedStartDate.DayNumber;
            var normalizedX = (double)dayOffset / totalDays; // Normalize to 0-1
            regressionPoints.Add((normalizedX, dataPoint.Value));
        }

        // Calculate linear regression coefficients (y = mx + b)
        var n = regressionPoints.Count;
        var sumX = regressionPoints.Sum(p => p.x);
        var sumY = regressionPoints.Sum(p => p.y);
        var sumXY = regressionPoints.Sum(p => p.x * p.y);
        var sumXX = regressionPoints.Sum(p => p.x * p.x);

        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        // Calculate trend line endpoints
        var (minY, maxY) = GetYRangeForMode(graphMode);
        var startY = intercept; // x = 0
        var endY = slope + intercept; // x = 1

        // Convert to canvas coordinates
        var startX = area.Left;
        var endX = area.Right;
        var startCanvasY = (float)(area.Bottom - ((startY - minY) * area.Height / (maxY - minY)));
        var endCanvasY = (float)(area.Bottom - ((endY - minY) * area.Height / (maxY - minY)));

        // Determine trend line color for optimal visibility
        var trendColor = GetOptimalTrendLineColor(lineColor, drawWhiteBackground);

        using var trendPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = trendColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true,
            PathEffect = drawShimFactory.PathEffects.CreateDash([10, 5], 0) // Dashed line to distinguish from data line
        });

        canvas.DrawLine(startX, startCanvasY, endX, endCanvasY, trendPaint);
    }

    /// <summary>
    /// Calculates the optimal color for the trend line to ensure visibility against background and line color
    /// </summary>
    private IColorShim GetOptimalTrendLineColor(Color lineColor, bool hasWhiteBackground)
    {
        // Convert MAUI Color to RGB values (0-1 range)
        var lineR = lineColor.Red;
        var lineG = lineColor.Green;
        var lineB = lineColor.Blue;

        // Calculate luminance of the line color
        var lineLuminance = 0.299 * lineR + 0.587 * lineG + 0.114 * lineB;

        // Background is assumed to be white if hasWhiteBackground is true, otherwise unknown/dark
        var backgroundLuminance = hasWhiteBackground ? 1.0f : 0.0f;

        // We want the trend line to be different from both the line color and background
        // If line is dark and background is white, make trend line a medium tone
        // If line is light and background is white, make trend line darker
        // If background is unknown/dark, use a lighter color that contrasts with the line

        IColorShim trendColor;

        if (hasWhiteBackground)
        {
            // White background case
            if (lineLuminance < 0.3) // Line is dark
            {
                // Use a medium gray that's different from the dark line
                trendColor = drawShimFactory.Colors.FromArgb(100, 100, 100, 255);
            }
            else if (lineLuminance > 0.7) // Line is light
            {
                // Use a darker color
                trendColor = drawShimFactory.Colors.FromArgb(50, 50, 50, 255);
            }
            else // Line is medium
            {
                // Use either darker or lighter depending on which provides more contrast
                var darkContrast = Math.Abs(lineLuminance - 0.2);
                var lightContrast = Math.Abs(lineLuminance - 0.8);
                
                if (darkContrast > lightContrast)
                {
                    trendColor = drawShimFactory.Colors.FromArgb(50, 50, 50, 255);
                }
                else
                {
                    trendColor = drawShimFactory.Colors.FromArgb(200, 200, 200, 255);
                }
            }
        }
        else
        {
            // Unknown/potentially dark background - use a color that should be visible
            // Pick a complementary or contrasting color to the line color
            if (lineLuminance < 0.5)
            {
                // Line is darkish, use a lighter trend line
                trendColor = drawShimFactory.Colors.FromArgb(180, 180, 180, 255);
            }
            else
            {
                // Line is lightish, use a darker trend line
                trendColor = drawShimFactory.Colors.FromArgb(80, 80, 80, 255);
            }
        }

        return trendColor;
    }

    // Raw Data drawing methods
    private void DrawRawDataGraph(SKCanvas canvas, List<RawMoodDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color pointColor, bool drawWhiteBackground = true)
    {
        DrawRawDataGraph(drawShimFactory.FromRaw(canvas), dataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, pointColor, drawWhiteBackground);
    }

    /// <summary>
    /// Draws a scatter plot graph for raw mood data points using GraphDataPoint format
    /// </summary>
    private void DrawRawDataGraphFromGraphDataPoints(ICanvasShim canvas, List<GraphDataPoint> dataPoints, List<RawMoodDataPoint> originalDataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color pointColor, bool drawWhiteBackground = true)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);

        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.StartDate;
        var requestedEndDate = dateRange.EndDate;

        // Convert to DateTime for timestamp calculations
        var startDateTime = requestedStartDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = requestedEndDate.ToDateTime(TimeOnly.MaxValue);

        // Conditionally draw background
        if (drawWhiteBackground)
        {
            DrawBackground(canvas, graphArea);
        }

        // Conditionally draw grid and axes for raw data (1-10 range)
        if (showAxesAndGrid)
        {
            DrawRawDataGrid(canvas, graphArea);
            DrawRawDataAxes(canvas, graphArea);
        }

        if (dataPoints.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw connecting lines between points
        DrawRawDataLinesFromGraphDataPoints(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor);

        // Draw data points on top of lines
        if (showDataPoints)
        {
            DrawRawDataPointsFromGraphDataPoints(canvas, graphArea, dataPoints, originalDataPoints, startDateTime, endDateTime, pointColor);
        }

        // Conditionally draw labels
        if (showAxesAndGrid)
        {
            DrawRawDataYAxisLabels(canvas, graphArea);
            DrawRawDataXAxisLabelsFromGraphDataPoints(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor);
        }

        // Conditionally draw trend line
        if (showTrendLine && dataPoints.Count > 1)
        {
            DrawRawDataTrendLineFromGraphDataPoints(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor, drawWhiteBackground);
        }

        // Conditionally draw title
        if (showTitle)
        {
            DrawRawDataTitle(canvas, width);
        }
    }

    /// <summary>
    /// Draws a scatter plot graph for raw mood data points
    /// </summary>
    private void DrawRawDataGraph(ICanvasShim canvas, List<RawMoodDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color pointColor, bool drawWhiteBackground = true)
    {
        // Transform RawMoodDataPoint objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformRawDataPoints(dataPoints).ToList();
        DrawRawDataGraphFromGraphDataPoints(canvas, graphDataPoints, dataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, pointColor, drawWhiteBackground);
    }

    private void DrawRawDataGrid(ICanvasShim canvas, SKRect area)
    {
        // For raw data, we want to skip edge lines (2 to 9 instead of 1 to 10)
        // and use fewer vertical lines for datetime readability
        DrawGrid(canvas, area, 1, 10, verticalLineCount: 6, verticalLineSpacing: 120);
    }

    private void DrawRawDataAxes(ICanvasShim canvas, SKRect area)
    {
        DrawBasicAxes(canvas, area);

        // Center line at mood value 5.5 (middle of 1-10 range)
        var centerValue = 5.5f;
        var centerY = area.Bottom - ((centerValue - 1) * area.Height / 9); // 9 = 10-1
        using var centerLinePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4
        });
        canvas.DrawLine(area.Left, centerY, area.Right, centerY, centerLinePaint);
    }

    private void DrawRawDataLines(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color lineColor)
    {
        // Transform RawMoodDataPoint objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformRawDataPoints(dataPoints).ToList();
        DrawRawDataLinesFromGraphDataPoints(canvas, area, graphDataPoints, startDateTime, endDateTime, lineColor);
    }

    /// <summary>
    /// Draws raw data lines using unified GraphDataPoint objects (refactored version)
    /// </summary>
    private void DrawRawDataLinesFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color lineColor)
    {
        if (dataPoints.Count < 2)
            return;

        using var linePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255), 255),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        });

        using var path = new SKPath();
        var totalTimeSpan = endDateTime - startDateTime;
        bool isFirstPoint = true;

        // Sort dataPoints by timestamp to ensure proper line connections
        var sortedPoints = dataPoints
            .Where(point => point.Timestamp >= startDateTime && point.Timestamp <= endDateTime)
            .OrderBy(point => point.Timestamp)
            .ToList();

        foreach (var point in sortedPoints)
        {
            // Calculate X position based on timestamp
            var timeFromStart = point.Timestamp - startDateTime;
            var proportionalPosition = (float)(timeFromStart.TotalMilliseconds / totalTimeSpan.TotalMilliseconds);
            var x = area.Left + (proportionalPosition * area.Width);

            // Calculate Y position based on mood value (assuming 1-10 range)
            var y = (float)(area.Bottom - ((point.Value - 1) * area.Height / 9)); // 9 = 10-1

            if (isFirstPoint)
            {
                path.MoveTo(x, y);
                isFirstPoint = false;
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        canvas.DrawPath(path, linePaint);
    }

    private void DrawRawDataPointsFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, List<RawMoodDataPoint> originalDataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        // Create different paints for start and end of work
        using var startPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(pointColor.Red * 255), (byte)(pointColor.Green * 255), (byte)(pointColor.Blue * 255), 255),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        using var endPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(pointColor.Red * 180), (byte)(pointColor.Green * 180), (byte)(pointColor.Blue * 180), 255),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        });

        var totalTimeSpan = endDateTime - startDateTime;

        // We need the original data to determine MoodType for shape rendering
        for (int i = 0; i < dataPoints.Count && i < originalDataPoints.Count; i++)
        {
            var graphPoint = dataPoints[i];
            var originalPoint = originalDataPoints[i];
            
            if (graphPoint.Timestamp >= startDateTime && graphPoint.Timestamp <= endDateTime)
            {
                // Calculate X position based on timestamp
                var timeFromStart = graphPoint.Timestamp - startDateTime;
                var proportionalPosition = (float)(timeFromStart.TotalMilliseconds / totalTimeSpan.TotalMilliseconds);
                var x = area.Left + (proportionalPosition * area.Width);

                // Calculate Y position based on mood value (1-10 range)
                var y = (float)(area.Bottom - ((graphPoint.Value - 1) * area.Height / 9)); // 9 = 10-1

                // Draw different shapes for start vs end of work
                if (originalPoint.MoodType == MoodType.StartOfWork)
                {
                    // Filled circle for start of work
                    canvas.DrawCircle(x, y, 6, startPaint);
                }
                else
                {
                    // Hollow circle for end of work
                    canvas.DrawCircle(x, y, 6, endPaint);
                }
            }
        }
    }

    private void DrawRawDataPoints(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        // Transform RawMoodDataPoint objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformRawDataPoints(dataPoints).ToList();
        DrawRawDataPointsFromGraphDataPoints(canvas, area, graphDataPoints, dataPoints, startDateTime, endDateTime, pointColor);
    }

    private void DrawRawDataYAxisLabels(ICanvasShim canvas, SKRect area)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        });

        var yRange = 9; // 10 - 1
        var yStep = area.Height / yRange;

        for (int i = 1; i <= 10; i += 2) // Show every other value to avoid crowding
        {
            var y = area.Bottom - ((i - 1) * yStep);
            canvas.DrawText(i.ToString(), area.Left - 10, y + 4, labelPaint);
        }
    }

    private void DrawRawDataXAxisLabelsFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        });

        var totalTimeSpan = endDateTime - startDateTime;

        // Show at most 4 time labels distributed across the range
        var labelCount = Math.Min(4, Math.Max(2, (int)(totalTimeSpan.TotalDays)));

        for (int i = 0; i <= labelCount; i++)
        {
            var proportionalPosition = (float)i / labelCount;
            var x = area.Left + (proportionalPosition * area.Width);

            var labelDateTime = startDateTime.AddTicks((long)(proportionalPosition * totalTimeSpan.Ticks));
            var timeText = labelDateTime.ToString("MM/dd HH:mm");

            canvas.DrawText(timeText, x, area.Bottom + 20, labelPaint);
        }
    }

    private void DrawRawDataXAxisLabels(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        // Transform RawMoodDataPoint objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformRawDataPoints(dataPoints).ToList();
        DrawRawDataXAxisLabelsFromGraphDataPoints(canvas, area, graphDataPoints, startDateTime, endDateTime, pointColor);
    }

    private void DrawRawDataTitle(ICanvasShim canvas, int width)
    {
        using IPaintShim titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        canvas.DrawText("Raw Mood Data Over Time", width / 2, 30, titlePaint);
    }

    private void DrawRawDataTrendLineFromGraphDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor, bool drawWhiteBackground)
    {
        if (dataPoints.Count < 2) return;

        // Filter and sort points within the date range
        var filteredPoints = dataPoints
            .Where(point => point.Timestamp >= startDateTime && point.Timestamp <= endDateTime)
            .OrderBy(point => point.Timestamp)
            .ToList();

        if (filteredPoints.Count < 2) return;

        // Calculate linear regression (y = mx + b) where x is time and y is mood value
        var n = filteredPoints.Count;
        var totalTimeSpan = endDateTime - startDateTime;
        
        // Convert timestamps to normalized time values (0 to 1)
        var timeValues = filteredPoints.Select(p => 
        {
            var timeFromStart = p.Timestamp - startDateTime;
            return timeFromStart.TotalMilliseconds / totalTimeSpan.TotalMilliseconds;
        }).ToList();
        
        var moodValues = filteredPoints.Select(p => (double)p.Value).ToList();

        // Linear regression calculations
        var sumX = timeValues.Sum();
        var sumY = moodValues.Sum();
        var sumXY = timeValues.Zip(moodValues, (x, y) => x * y).Sum();
        var sumXX = timeValues.Select(x => x * x).Sum();

        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        // Calculate start and end points of trend line
        var startY = slope * 0.0 + intercept; // At time 0
        var endY = slope * 1.0 + intercept;   // At time 1

        // Convert to screen coordinates
        var startX = area.Left;
        var endX = area.Right;
        var startScreenY = (float)(area.Bottom - ((startY - 1) * area.Height / 9)); // 9 = 10-1 for raw data range
        var endScreenY = (float)(area.Bottom - ((endY - 1) * area.Height / 9));

        // Get optimal trend line color
        var trendLineColor = GetOptimalTrendLineColor(pointColor, drawWhiteBackground);

        using var trendLinePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = trendLineColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true,
            PathEffect = drawShimFactory.PathEffects.CreateDash([10, 5], 0) // Dashed line
        });

        canvas.DrawLine(startX, startScreenY, endX, endScreenY, trendLinePaint);
    }

    private void DrawRawDataTrendLine(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor, bool drawWhiteBackground)
    {
        // Transform RawMoodDataPoint objects to GraphDataPoint objects and delegate to the refactored method
        var graphDataPoints = _dataTransformer.TransformRawDataPoints(dataPoints).ToList();
        DrawRawDataTrendLineFromGraphDataPoints(canvas, area, graphDataPoints, startDateTime, endDateTime, pointColor, drawWhiteBackground);
    }

    // New save methods with GraphMode support

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with the specified graph mode and white background.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show the trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact or Average)</param>
    /// <param name="lineColor">Color for the graph line and data points</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, GraphMode graphMode, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, graphMode, lineColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with the specified graph mode and custom background image.
    /// </summary>
    /// <param name="moodEntries">The mood entries to graph</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="graphMode">The graph mode determining how mood data is interpreted (Impact or Average)</param>
    /// <param name="backgroundImagePath">Path to the custom background image file</param>
    /// <param name="lineColor">Color for the graph line and data points</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, graphMode, backgroundImagePath, lineColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    // Raw Data graph implementations

    /// <summary>
    /// Generates a scatter plot PNG image from raw mood data points with white background.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="lineColor">Color for the data points and connecting lines</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>PNG image data as byte array</returns>
    public async Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        return await GenerateRawDataGraphAsync(rawDataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, null, lineColor, width, height);
    }

    /// <summary>
    /// Generates a scatter plot PNG image from raw mood data points with custom background image.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="backgroundImagePath">Path to the custom background image file, or null for white background</param>
    /// <param name="lineColor">Color for the data points and connecting lines</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>PNG image data as byte array</returns>
    public async Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string? backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var sortedPoints = rawDataPoints.OrderBy(p => p.Timestamp).ToList();

        return await GenerateImageAsync(width, height, async canvas =>
        {
            // Need special handling for RawData background since it uses .Raw property
            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                var fileShim = fileShimFactory.Create();
                if (fileShim.Exists(backgroundImagePath))
                {
                    using var backgroundBitmap = drawShimFactory.DecodeBitmapFromFile(backgroundImagePath);
                    if (backgroundBitmap != null)
                    {
                        canvas.DrawBitmap(backgroundBitmap, new SKRect(0, 0, width, height));
                    }
                }
                else
                {
                    canvas.Clear(drawShimFactory.Colors.White.Raw);
                }
            }
            else
            {
                canvas.Clear(drawShimFactory.Colors.White.Raw);
            }

            var hasCustomBackground = !string.IsNullOrEmpty(backgroundImagePath);
            await Task.Run(() => DrawRawDataGraph(canvas.Raw, sortedPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, !hasCustomBackground));
        });
    }

    /// <summary>
    /// Saves a scatter plot PNG image to the specified file path from raw mood data points with white background.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show a trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="lineColor">Color for the data points and connecting lines</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateRawDataGraphAsync(rawDataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    /// <summary>
    /// Saves a scatter plot PNG image to the specified file path from raw mood data points with custom background image.
    /// </summary>
    /// <param name="rawDataPoints">The raw mood data points to plot</param>
    /// <param name="dateRange">The requested date range for proportional positioning</param>
    /// <param name="showDataPoints">Whether to show individual data points on the graph</param>
    /// <param name="showAxesAndGrid">Whether to show axes and grid lines</param>
    /// <param name="showTitle">Whether to show the graph title</param>
    /// <param name="showTrendLine">Whether to show a trend line</param>
    /// <param name="filePath">Path where to save the PNG file</param>
    /// <param name="backgroundImagePath">Path to the custom background image file</param>
    /// <param name="lineColor">Color for the data points and connecting lines</param>
    /// <param name="width">Width of the graph in pixels (default: 800)</param>
    /// <param name="height">Height of the graph in pixels (default: 600)</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateRawDataGraphAsync(rawDataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }
}