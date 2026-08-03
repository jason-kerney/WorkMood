using SkiaSharp;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of line graph generator using SkiaSharp for rendering, designed to work with GraphData objects.
/// Mimics LineGraphService functionality while accepting GraphData as input.
/// </summary>
public class LineGraphGenerator(IDrawShimFactory drawShimFactory, IFileShimFactory fileShimFactory) : ILineGraphGenerator
{
    private const int Padding = 60;

    private sealed class WeekendAwareXAxisMapper
    {
        private readonly DateTime _rangeStart;
        private readonly Dictionary<DateOnly, double> _daySlotOffsetByDate;
        private readonly double _totalSlotSpan;

        private WeekendAwareXAxisMapper(DateTime rangeStart, Dictionary<DateOnly, double> daySlotOffsetByDate, double totalSlotSpan)
        {
            _rangeStart = rangeStart;
            _daySlotOffsetByDate = daySlotOffsetByDate;
            _totalSlotSpan = totalSlotSpan <= 0 ? 1 : totalSlotSpan;
        }

        public static WeekendAwareXAxisMapper Create(DateOnly startDate, DateOnly endDate, IReadOnlyList<FilledGraphDataPoint> dataPoints)
        {
            var datesWithData = dataPoints
                .Select(p => DateOnly.FromDateTime(p.Timestamp))
                .ToHashSet();

            var offsets = new Dictionary<DateOnly, double>();
            var offset = 0d;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                offsets[date] = offset;
                var consumesSlot = CalendarGapPolicy.ShouldConsumeCompressedDaySlot(date, datesWithData);
                if (consumesSlot)
                {
                    offset += 1d;
                }
            }

            return new WeekendAwareXAxisMapper(startDate.ToDateTime(TimeOnly.MinValue), offsets, offset);
        }

        public float GetNormalizedX(DateTime timestamp)
        {
            var date = DateOnly.FromDateTime(timestamp);
            if (!_daySlotOffsetByDate.TryGetValue(date, out var slotOffset))
            {
                return 0f;
            }

            // Keep intra-day ordering (important for RawData mode) while weekends without
            // recorded points still collapse to zero-width slots.
            var intraDayFraction = (timestamp - date.ToDateTime(TimeOnly.MinValue)).TotalDays;
            var compressedOffset = slotOffset + intraDayFraction;
            var normalized = compressedOffset / _totalSlotSpan;

            if (normalized < 0) return 0f;
            if (normalized > 1) return 1f;
            return (float)normalized;
        }
    }
    
    /// <summary>
    /// Initializes a new instance with default factory implementations.
    /// </summary>
    public LineGraphGenerator() : this(new DrawShimFactory(), new FileShimFactory()) { }

    /// <summary>
    /// Generates a line graph PNG image from GraphData
    /// </summary>
    public async Task<byte[]> GenerateLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, int width = 800, int height = 600)
    {
        return await RenderGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath: null, lineColor, backgroundColor: null, width, height);
    }

    /// <summary>
    /// Generates a line graph PNG image from GraphData with a solid background color
    /// </summary>
    public async Task<byte[]> GenerateLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, Color lineColor, Color backgroundColor, int width = 800, int height = 600)
    {
        return await RenderGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath: null, lineColor, backgroundColor, width, height);
    }

    /// <summary>
    /// Generates a line graph PNG image from GraphData with custom background
    /// </summary>
    public async Task<byte[]> GenerateLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        return await RenderGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, backgroundColor: null, width, height);
    }

    private async Task<byte[]> RenderGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string? backgroundImagePath, Color lineColor, Color? backgroundColor, int width, int height)
    {
        return await GenerateImageAsync(width, height, async canvas =>
        {
            var hasWhiteBackground = SetupCanvasBackground(canvas, backgroundImagePath, backgroundColor, width, height);
            await Task.Run(() => DrawGraph(canvas, graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, width, height, lineColor, hasWhiteBackground));
        });
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path
    /// </summary>
    public async Task SaveLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with a solid background color
    /// </summary>
    public async Task SaveLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, Color lineColor, Color backgroundColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, lineColor, backgroundColor, width, height);
        await SaveImageDataAsync(imageData, filePath);
    }

    /// <summary>
    /// Saves a line graph PNG image to the specified file path with custom background
    /// </summary>
    public async Task SaveLineGraphAsync(GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(graphData, dateRange, showDataPoints, showAxesAndGrid, showTitle, showTrendLine, backgroundImagePath, lineColor, width, height);
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
    private bool SetupCanvasBackground(ICanvasShim canvas, string? backgroundImagePath, Color? backgroundColor, int width, int height)
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
                    return false;
                }
            }
        }

        if (backgroundColor is not null)
        {
            var customColor = backgroundColor;
            canvas.Clear(drawShimFactory.Colors.FromArgb(
                (byte)(customColor.Red * 255),
                (byte)(customColor.Green * 255),
                (byte)(customColor.Blue * 255),
                (byte)(customColor.Alpha * 255)));
            return false;
        }

        canvas.Clear(drawShimFactory.Colors.White);
        return true;
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
    /// Main drawing logic using GraphData, mimicking LineGraphService behavior
    /// </summary>
    private void DrawGraph(ICanvasShim canvas, GraphData graphData, DateRangeInfo dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, bool showTrendLine, int width, int height, Color lineColor, bool hasWhiteBackground)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);
        var dataPoints = graphData.DataPoints.ToList();

        // Calculate the full datetime range for proportional positioning
        var requestedStartDateTime = dateRange.StartDate.ToDateTime(TimeOnly.MinValue);
        
        // Determine the end time based on actual data
        var requestedEndDateTime = CalculateEndDateTime(dateRange.EndDate, dataPoints);
        var xAxisMapper = WeekendAwareXAxisMapper.Create(dateRange.StartDate, dateRange.EndDate, dataPoints);

        // Use GraphData's Y range
        var minY = (int)graphData.YAxisRange.Min;
        var maxY = (int)graphData.YAxisRange.Max;

        // Conditionally draw grid and axes
        if (showAxesAndGrid)
        {
            DrawGrid(canvas, graphArea, minY, maxY);
            DrawBasicAxes(canvas, graphArea);
            
            // Draw center line if specified in GraphData
            if (graphData.CenterLineValue.HasValue)
            {
                DrawCenterLine(canvas, graphArea, graphData.CenterLineValue.Value, minY, maxY);
            }
        }

        if (dataPoints.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw data line and optionally points with proportional positioning
        DrawDataLine(canvas, graphArea, dataPoints, requestedStartDateTime, requestedEndDateTime, lineColor, minY, maxY, xAxisMapper, graphData.GapSegmentSecondaryPenMode);
        
        if (showDataPoints)
        {
            DrawDataPoints(canvas, graphArea, dataPoints, requestedStartDateTime, requestedEndDateTime, lineColor, minY, maxY, xAxisMapper, graphData.GapSegmentSecondaryPenMode);
        }

        // Draw trend line if requested
        if (showTrendLine)
        {
            DrawTrendLine(canvas, graphArea, dataPoints, requestedStartDateTime, requestedEndDateTime, lineColor, minY, maxY, hasWhiteBackground, xAxisMapper);
        }

        // Draw axes labels
        if (showAxesAndGrid)
        {
            DrawYAxisLabels(canvas, graphArea, minY, maxY, graphData.YAxisLabelStep);
            DrawXAxisLabels(canvas, graphArea, dataPoints, requestedStartDateTime, requestedEndDateTime, showDataPoints, lineColor, xAxisMapper);
        }

        // Draw title
        if (showTitle)
        {
            DrawGraphTitle(canvas, width, graphData.Title);
        }
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

        // Y-axis
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);

        // X-axis
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);
    }

    /// <summary>
    /// Draws a center line at the specified Y value (mimicking LineGraphService behavior)
    /// </summary>
    private void DrawCenterLine(ICanvasShim canvas, SKRect area, float centerValue, int minY, int maxY)
    {
        var centerY = area.Bottom - ((centerValue - minY) * area.Height / (maxY - minY));
        using var centerLinePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 4
        });
        canvas.DrawLine(area.Left, centerY, area.Right, centerY, centerLinePaint);
    }

    private void DrawNoDataMessage(ICanvasShim canvas, SKRect area)
    {
        using var textPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Gray,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        });

        var centerX = area.Left + area.Width / 2;
        var centerY = area.Top + area.Height / 2;
        canvas.DrawText("No data available for the selected range", centerX, centerY, textPaint);
    }

    private void DrawDataLine(ICanvasShim canvas, SKRect area, List<FilledGraphDataPoint> dataPoints, DateTime requestedStartDateTime, DateTime requestedEndDateTime, Color lineColor, int minY, int maxY, WeekendAwareXAxisMapper xAxisMapper, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode)
    {
        if (dataPoints.Count < 2) return;

        using var linePaint = CreateStrokePaint(lineColor);

        using var secondaryLinePaint = dataPoints.Any(point => point.IsSyntheticGapFill) && gapSegmentSecondaryPenMode.HasValue
            ? CreateStrokePaint(GetDerivedGapSegmentColor(lineColor, gapSegmentSecondaryPenMode.Value))
            : null;

        // Draw one path per weekend-aware segment so Friday->Monday remains connected when
        // only unrecorded weekend days are missing.
        var datesWithData = dataPoints
            .Select(p => DateOnly.FromDateTime(p.Timestamp))
            .ToHashSet();

        var segments = GraphLineSegmentBuilder.BuildSegments(
            dataPoints,
            p => DateOnly.FromDateTime(p.Timestamp),
            (previousDate, currentDate) => CalendarGapPolicy.ShouldBreakForRecordedSeries(previousDate, currentDate, datesWithData));

        foreach (var segment in segments)
        {
            if (segment.Count < 2) continue;

            DrawSegmentRuns(canvas, area, segment, minY, maxY, xAxisMapper, linePaint, secondaryLinePaint);
        }
    }

    private void DrawSegmentRuns(
        ICanvasShim canvas,
        SKRect area,
        IReadOnlyList<FilledGraphDataPoint> segment,
        int minY,
        int maxY,
        WeekendAwareXAxisMapper xAxisMapper,
        IPaintShim primaryPaint,
        IPaintShim? secondaryPaint)
    {
        var currentRun = new List<FilledGraphDataPoint> { segment[0] };
        var currentUsesSecondary = false;

        for (var index = 0; index < segment.Count - 1; index++)
        {
            var currentPoint = segment[index];
            var nextPoint = segment[index + 1];
            var edgeUsesSecondary = secondaryPaint != null && (currentPoint.IsSyntheticGapFill || nextPoint.IsSyntheticGapFill);

            if (currentRun.Count == 1)
            {
                currentUsesSecondary = edgeUsesSecondary;
                currentRun.Add(nextPoint);
                continue;
            }

            if (edgeUsesSecondary == currentUsesSecondary)
            {
                currentRun.Add(nextPoint);
                continue;
            }

            DrawRunPath(canvas, area, currentRun, minY, maxY, xAxisMapper, currentUsesSecondary ? secondaryPaint! : primaryPaint);
            currentRun = new List<FilledGraphDataPoint> { currentPoint, nextPoint };
            currentUsesSecondary = edgeUsesSecondary;
        }

        if (currentRun.Count >= 2)
        {
            DrawRunPath(canvas, area, currentRun, minY, maxY, xAxisMapper, currentUsesSecondary ? secondaryPaint! : primaryPaint);
        }
    }

    private void DrawRunPath(
        ICanvasShim canvas,
        SKRect area,
        IReadOnlyList<FilledGraphDataPoint> run,
        int minY,
        int maxY,
        WeekendAwareXAxisMapper xAxisMapper,
        IPaintShim paint)
    {
        using var path = new SKPath();

        for (var index = 0; index < run.Count; index++)
        {
            var dataPoint = run[index];
            var normalizedX = xAxisMapper.GetNormalizedX(dataPoint.Timestamp);
            var x = area.Left + (normalizedX * area.Width);
            var value = dataPoint.Value ?? 0;
            var y = (float)(area.Bottom - ((value - minY) * area.Height / (maxY - minY)));

            if (index == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        canvas.DrawPath(path, paint);
    }

    private IPaintShim CreateStrokePaint(Color color)
    {
        return drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), 255),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        });
    }

    private static Color GetDerivedGapSegmentColor(Color primaryColor, GapSegmentSecondaryPenMode gapSegmentSecondaryPenMode)
    {
        var (hue, saturation, value) = RgbToHsv(primaryColor);

        var derivedHue = gapSegmentSecondaryPenMode switch
        {
            GapSegmentSecondaryPenMode.Complementary => NormalizeHue(hue + 180f),
            GapSegmentSecondaryPenMode.FirstTriadic => NormalizeHue(hue + 120f),
            GapSegmentSecondaryPenMode.SecondTriadic => NormalizeHue(hue + 240f),
            _ => hue
        };

        return HsvToColor(derivedHue, saturation, value);
    }

    private static (float Hue, float Saturation, float Value) RgbToHsv(Color color)
    {
        var red = color.Red;
        var green = color.Green;
        var blue = color.Blue;

        var max = Math.Max(red, Math.Max(green, blue));
        var min = Math.Min(red, Math.Min(green, blue));
        var delta = max - min;

        if (delta == 0)
        {
            return (0f, 0f, max);
        }

        float hue;
        if (max == red)
        {
            hue = 60f * (((green - blue) / delta) % 6f);
        }
        else if (max == green)
        {
            hue = 60f * (((blue - red) / delta) + 2f);
        }
        else
        {
            hue = 60f * (((red - green) / delta) + 4f);
        }

        if (hue < 0f)
        {
            hue += 360f;
        }

        var saturation = max == 0f ? 0f : delta / max;
        return (hue, saturation, max);
    }

    private static Color HsvToColor(float hue, float saturation, float value)
    {
        if (saturation == 0f)
        {
            return Color.FromRgb(value, value, value);
        }

        var sector = hue / 60f;
        var sectorIndex = (int)Math.Floor(sector) % 6;
        var fraction = sector - (float)Math.Floor(sector);

        var p = value * (1f - saturation);
        var q = value * (1f - saturation * fraction);
        var t = value * (1f - saturation * (1f - fraction));

        return sectorIndex switch
        {
            0 => Color.FromRgb(value, t, p),
            1 => Color.FromRgb(q, value, p),
            2 => Color.FromRgb(p, value, t),
            3 => Color.FromRgb(p, q, value),
            4 => Color.FromRgb(t, p, value),
            _ => Color.FromRgb(value, p, q)
        };
    }

    private static float NormalizeHue(float hue)
    {
        var normalizedHue = hue % 360f;
        return normalizedHue < 0f ? normalizedHue + 360f : normalizedHue;
    }

    private void DrawDataPoints(ICanvasShim canvas, SKRect area, List<FilledGraphDataPoint> dataPoints, DateTime requestedStartDateTime, DateTime requestedEndDateTime, Color lineColor, int minY, int maxY, WeekendAwareXAxisMapper xAxisMapper, GapSegmentSecondaryPenMode? gapSegmentSecondaryPenMode)
    {
        using var primaryPointPaint = CreateDataPointPaint(lineColor);

        using var secondaryPointPaint = dataPoints.Any(point => point.IsSyntheticGapFill) && gapSegmentSecondaryPenMode.HasValue
            ? CreateDataPointPaint(GetDerivedGapSegmentColor(lineColor, gapSegmentSecondaryPenMode.Value), useFullColorIntensity: true)
            : null;

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            var normalizedX = xAxisMapper.GetNormalizedX(dataPoint.Timestamp);
            var x = area.Left + (normalizedX * area.Width);

            var value = dataPoint.Value ?? 0; // Use 0 if null (shouldn't happen for filled data points)
            var y = (float)(area.Bottom - ((value - minY) * area.Height / (maxY - minY)));

            var pointPaint = dataPoint.IsSyntheticGapFill && secondaryPointPaint != null
                ? secondaryPointPaint
                : primaryPointPaint;

            canvas.DrawCircle(x, y, 4, pointPaint);
        }
    }

    private IPaintShim CreateDataPointPaint(Color color, bool useFullColorIntensity = false)
    {
        var intensity = useFullColorIntensity ? 255 : 180;

        return drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.FromArgb((byte)(color.Red * intensity), (byte)(color.Green * intensity), (byte)(color.Blue * intensity), 255),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });
    }

    private void DrawTrendLine(ICanvasShim canvas, SKRect area, List<FilledGraphDataPoint> dataPoints, DateTime requestedStartDateTime, DateTime requestedEndDateTime, Color lineColor, int minY, int maxY, bool drawWhiteBackground, WeekendAwareXAxisMapper xAxisMapper)
    {
        if (dataPoints.Count < 2) return;

        // Calculate linear regression
        var regressionPoints = new List<(double x, double y)>();
        
        foreach (var dataPoint in dataPoints)
        {
            if (!dataPoint.Value.HasValue) continue;
            var normalizedX = xAxisMapper.GetNormalizedX(dataPoint.Timestamp);
            regressionPoints.Add((normalizedX, dataPoint.Value.GetValueOrDefault()));
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

    private void DrawYAxisLabels(ICanvasShim canvas, SKRect area, int minY, int maxY, int labelStep)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        });

        var yRange = maxY - minY;
        var yStep = area.Height / yRange;

        for (int i = minY; i <= maxY; i += labelStep)
        {
            var y = area.Bottom - ((i - minY) * yStep);
            canvas.DrawText(i.ToString(), area.Left - 10, y + 4, labelPaint);
        }
    }

    private void DrawXAxisLabels(ICanvasShim canvas, SKRect area, List<FilledGraphDataPoint> dataPoints, DateTime requestedStartDateTime, DateTime requestedEndDateTime, bool showDataPoints, Color lineColor, WeekendAwareXAxisMapper xAxisMapper)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        });

        // Show labels for the requested date range
        var totalTimeSpan = requestedEndDateTime - requestedStartDateTime;
        var totalDays = Math.Max(1, (int)totalTimeSpan.TotalDays);
        var labelCount = Math.Min(8, Math.Max(2, totalDays / 30)); // At least 2 labels, roughly monthly intervals

        for (int i = 0; i <= labelCount; i++)
        {
            var proportion = (float)i / labelCount;
            var dateTime = requestedStartDateTime.AddMilliseconds(proportion * totalTimeSpan.TotalMilliseconds);
            var normalizedX = xAxisMapper.GetNormalizedX(dateTime);
            var x = area.Left + (normalizedX * area.Width);
            canvas.DrawText(DateOnly.FromDateTime(dateTime).ToString("MM/dd"), x, area.Bottom + 20, labelPaint);
        }

        // Show data point dots on the X-axis if showDataPoints is true
        if (showDataPoints && dataPoints.Count > 0 && dataPoints.Count <= 10)
        {
            using var dataLabelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
            {
                Color = drawShimFactory.Colors.FromArgb((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180), 255),
                TextSize = 8,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            });

            foreach (var dataPoint in dataPoints)
            {
                var normalizedX = xAxisMapper.GetNormalizedX(dataPoint.Timestamp);
                var x = area.Left + (normalizedX * area.Width);

                canvas.DrawText("●", x, area.Bottom + 35, dataLabelPaint);
            }
        }
    }

    private void DrawGraphTitle(ICanvasShim canvas, int width, string title)
    {
        using var titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = drawShimFactory.Colors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = drawShimFactory.Fonts.FromFamilyName("Arial", drawShimFactory.Fonts.Styles.Bold)
        });

        canvas.DrawText(title, width / 2, 30, titlePaint);
    }

    private IColorShim GetOptimalTrendLineColor(Color lineColor, bool hasWhiteBackground)
    {
        // Convert MAUI Color to RGB values (0-1 range)
        var lineR = lineColor.Red;
        var lineG = lineColor.Green;
        var lineB = lineColor.Blue;

        // Calculate luminance of the line color
        var lineLuminance = 0.299 * lineR + 0.587 * lineG + 0.114 * lineB;

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

    /// <summary>
    /// Calculates the appropriate end DateTime based on the requested end date and actual data points.
    /// If the last data point is on the same day as the end date, use that time.
    /// Otherwise, use the beginning of the end date (TimeOnly.MinValue).
    /// </summary>
    private DateTime CalculateEndDateTime(DateOnly endDate, List<FilledGraphDataPoint> dataPoints)
    {
        if (dataPoints == null || dataPoints.Count == 0)
        {
            return endDate.ToDateTime(TimeOnly.MinValue);
        }

        // Find the last data point chronologically
        var lastDataPoint = dataPoints.OrderBy(dp => dp.Timestamp).LastOrDefault();
        
        if (lastDataPoint == null)
        {
            return endDate.ToDateTime(TimeOnly.MinValue);
        }

        var lastDataPointDate = DateOnly.FromDateTime(lastDataPoint.Timestamp);
        
        // If the last data point is on the same day as the requested end date, 
        // use the time from that data point
        if (lastDataPointDate == endDate)
        {
            return lastDataPoint.Timestamp;
        }
        
        // Otherwise, use the beginning of the end date
        return endDate.ToDateTime(TimeOnly.MinValue);
    }
}