using SkiaSharp;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of line graph generator using SkiaSharp for rendering
/// </summary>
public class LineGraphGenerator(IDrawShimFactory drawShimFactory, IFileShimFactory fileShimFactory) : ILineGraphGenerator
{
    private const int Padding = 60;
    
    /// <summary>
    /// Initializes a new instance with default factory implementations.
    /// </summary>
    public LineGraphGenerator() : this(new DrawShimFactory(), new FileShimFactory()) { }

    /// <summary>
    /// Generates a line graph PNG image from unified graph data points
    /// </summary>
    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, AxisRange yAxisRange, string title, int width = 800, int height = 600)
    {
        return await GenerateImageAsync(width, height, canvas =>
        {
            DrawGraph(canvas, dataPoints.ToList(), dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, yAxisRange, title);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Generates a line graph PNG image from unified graph data points with custom background
    /// </summary>
    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, AxisRange yAxisRange, string title, int width = 800, int height = 600)
    {
        return await GenerateImageAsync(width, height, canvas =>
        {
            SetupCanvasBackground(canvas, backgroundImagePath, width, height);
            DrawGraph(canvas, dataPoints.ToList(), dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, yAxisRange, title, drawWhiteBackground: false);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path
    /// </summary>
    public async Task SaveLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, AxisRange yAxisRange, string title, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(dataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, yAxisRange, title, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background
    /// </summary>
    public async Task SaveLineGraphAsync(IEnumerable<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, AxisRange yAxisRange, string title, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(dataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, yAxisRange, title, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

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
    /// Main drawing logic for unified data points
    /// </summary>
    private void DrawGraph(ICanvasShim canvas, List<GraphDataPoint> dataPoints, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color lineColor, AxisRange yAxisRange, string title, bool drawWhiteBackground = true)
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

        // Use provided Y range
        var (minY, maxY) = (yAxisRange.Min, yAxisRange.Max);

        // Conditionally draw grid and axes
        if (showAxesAndGrid)
        {
            DrawGrid(canvas, graphArea, minY, maxY);
            DrawBasicAxes(canvas, graphArea);
        }

        if (dataPoints.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw data line and optionally points with proportional positioning
        DrawDataLine(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor);
        
        if (showDataPoints)
        {
            DrawDataPoints(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor);
        }

        // Draw trend line if requested
        if (showTrendLine)
        {
            DrawTrendLine(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, lineColor, drawWhiteBackground);
        }

        // Draw axes labels
        if (showAxesAndGrid)
        {
            DrawYAxisLabels(canvas, graphArea, minY, maxY);
            DrawXAxisLabels(canvas, graphArea, dataPoints, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
        }

        // Draw title
        if (showTitle)
        {
            DrawGraphTitle(canvas, width, title);
        }
    }

    /// <summary>
    /// Determine Y range based on data points (simplified approach for unified data)
    /// </summary>
    private static (int minY, int maxY) GetYRangeForDataPoints(List<GraphDataPoint> dataPoints)
    {
        if (dataPoints.Count == 0)
            return (-5, 5);

        // Check if values appear to be raw mood values (1-10) or impact values (-9 to +9)
        var minValue = dataPoints.Min(p => p.Value);
        var maxValue = dataPoints.Max(p => p.Value);

        if (minValue >= 1 && maxValue <= 10)
        {
            // Raw data range
            return (1, 10);
        }
        else if (minValue >= -9 && maxValue <= 9)
        {
            // Impact/average range
            return (-9, 9);
        }
        else
        {
            // Generic range - expand to nearest reasonable bounds
            var range = maxValue - minValue;
            var padding = Math.Max(1, range * 0.1f);
            return ((int)Math.Floor(minValue - padding), (int)Math.Ceiling(maxValue + padding));
        }
    }

    // Placeholder implementations for the drawing methods - these will be extracted from LineGraphService incrementally
    private void DrawBackground(ICanvasShim canvas, SKRect area)
    {
        using var backgroundPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.White,
            Style = SKPaintStyle.Fill
        });
        canvas.DrawRect(area, backgroundPaint);
    }

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

    private void DrawBasicAxes(ICanvasShim canvas, SKRect area)
    {
        using var axisPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        });

        // X-axis (bottom)
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);
        
        // Y-axis (left)
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);
    }

    private void DrawNoDataMessage(ICanvasShim canvas, SKRect area)
    {
        using var messagePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.DarkGray,
            Style = SKPaintStyle.Fill,
            TextSize = 16,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        var centerX = area.MidX;
        var centerY = area.MidY;

        canvas.DrawText("No mood data available for the selected period", centerX, centerY, messagePaint);
    }

    private void DrawDataLine(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor)
    {
        if (dataPoints.Count < 2) return;

        var (minY, maxY) = GetYRangeForDataPoints(dataPoints);
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        using var linePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Alpha * 255), (byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        });

        using var path = new SKPath();
        bool isFirst = true;

        foreach (var point in dataPoints.OrderBy(p => p.Timestamp))
        {
            var dayOffset = DateOnly.FromDateTime(point.Timestamp).DayNumber - requestedStartDate.DayNumber;
            var normalizedX = (double)dayOffset / totalDays;
            var x = (float)(area.Left + normalizedX * area.Width);
            var y = area.Bottom - ((point.Value - minY) * area.Height / (maxY - minY));

            if (isFirst)
            {
                path.MoveTo(x, y);
                isFirst = false;
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        canvas.DrawPath(path, linePaint);
    }

    private void DrawDataPoints(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor)
    {
        var (minY, maxY) = GetYRangeForDataPoints(dataPoints);
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        using var pointPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Alpha * 255), (byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255)),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        foreach (var point in dataPoints)
        {
            var dayOffset = DateOnly.FromDateTime(point.Timestamp).DayNumber - requestedStartDate.DayNumber;
            var normalizedX = (double)dayOffset / totalDays;
            var x = (float)(area.Left + normalizedX * area.Width);
            var y = area.Bottom - ((point.Value - minY) * area.Height / (maxY - minY));

            canvas.DrawCircle(x, y, 4, pointPaint);
        }
    }

    private void DrawTrendLine(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, bool drawWhiteBackground)
    {
        if (dataPoints.Count < 2) return;

        var (minY, maxY) = GetYRangeForDataPoints(dataPoints);
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        // Calculate linear regression
        var regressionPoints = new List<(double x, double y)>();
        
        foreach (var point in dataPoints)
        {
            var dayOffset = DateOnly.FromDateTime(point.Timestamp).DayNumber - requestedStartDate.DayNumber;
            var normalizedX = (double)dayOffset / totalDays;
            regressionPoints.Add((normalizedX, point.Value));
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
        var startY = intercept; // x = 0
        var endY = slope + intercept; // x = 1

        // Convert to canvas coordinates
        var startX = area.Left;
        var endX = area.Right;
        var startCanvasY = (float)(area.Bottom - ((startY - minY) * area.Height / (maxY - minY)));
        var endCanvasY = (float)(area.Bottom - ((endY - minY) * area.Height / (maxY - minY)));

        using var trendPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true,
            PathEffect = drawShimFactory.PathEffects.CreateDash([10, 5], 0)
        });

        canvas.DrawLine(startX, startCanvasY, endX, endCanvasY, trendPaint);
    }

    private void DrawYAxisLabels(ICanvasShim canvas, SKRect area, int minY, int maxY)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            Style = SKPaintStyle.Fill,
            TextSize = 12,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        var yRange = maxY - minY;
        var labelStep = Math.Max(1, yRange / 6); // Show about 6 labels

        for (int i = minY; i <= maxY; i += labelStep)
        {
            var y = area.Bottom - ((i - minY) * area.Height / yRange);
            var labelX = area.Left - 10;
            canvas.DrawText(i.ToString(), labelX, y + 5, labelPaint);
        }
    }

    private void DrawXAxisLabels(ICanvasShim canvas, SKRect area, List<GraphDataPoint> dataPoints, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            Style = SKPaintStyle.Fill,
            TextSize = 10,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        // Show start and end dates
        canvas.DrawText(requestedStartDate.ToString("MM/dd"), area.Left, area.Bottom + 20, labelPaint);
        canvas.DrawText(requestedEndDate.ToString("MM/dd"), area.Right - 30, area.Bottom + 20, labelPaint);
    }

    private void DrawGraphTitle(ICanvasShim canvas, int width, string title)
    {
        using var titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            Style = SKPaintStyle.Fill,
            TextSize = 16,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        var titleX = width / 2f;
        var titleY = 30f;
        canvas.DrawText(title, titleX, titleY, titlePaint);
    }
}