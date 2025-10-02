using SkiaSharp;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

// Sha: 6d7b2feb687d49cec0f8c2b736b310c7919e1c6d

/// <summary>
/// Implementation of line graph service using SkiaSharp for rendering
/// </summary>
public class LineGraphService(IDrawShimFactory drawShimFactory) : ILineGraphService
{
    private const int MinYValue = -9;
    private const int MaxYValue = 9;
    private const int Padding = 60;
    private const int GridLineSpacing = 20;

    public LineGraphService() : this(new DrawShimFactory()) { }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width = 800, int height = 600)
    {
        return await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, Colors.Blue, width, height);
    }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, Color lineColor, int width = 800, int height = 600)
    {
        var filteredEntries = moodEntries
            .Where(e => e.Value.HasValue)
            .OrderBy(e => e.Date)
            .ToList();

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Clear canvas with white background
        canvas.Clear(SKColors.White);

        await Task.Run(() => DrawGraph(canvas, filteredEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, true)); // Draw white background for normal graphs

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string backgroundImagePath, int width = 800, int height = 600)
    {
        return await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, backgroundImagePath, Colors.Blue, width, height);
    }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var filteredEntries = moodEntries
            .Where(e => e.Value.HasValue)
            .OrderBy(e => e.Date)
            .ToList();

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Load and draw custom background
        if (File.Exists(backgroundImagePath))
        {
            using var backgroundBitmap = SKBitmap.Decode(backgroundImagePath);
            if (backgroundBitmap != null)
            {
                canvas.DrawBitmap(backgroundBitmap, new SKRect(0, 0, width, height));
            }
        }
        else
        {
            // Fallback to white background if image doesn't exist
            canvas.Clear(SKColors.White);
        }

        await Task.Run(() => DrawGraph(canvas, filteredEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, false)); // Don't draw white background when using custom background

        // Convert to PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, backgroundImagePath, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, backgroundImagePath, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    // New overloads with GraphMode support

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, int width = 800, int height = 600)
    {
        return await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, graphMode, Colors.Blue, width, height);
    }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, Color lineColor, int width = 800, int height = 600)
    {
        // Filter entries based on graph mode
        var filteredEntries = FilterEntriesForGraphMode(moodEntries, graphMode)
            .OrderBy(e => e.Date)
            .ToList();

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Clear canvas with white background
        canvas.Clear(SKColors.White);

        await Task.Run(() => DrawGraphForMode(canvas, filteredEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, graphMode, true)); // Draw white background for normal graphs

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        // Filter entries based on graph mode
        var filteredEntries = FilterEntriesForGraphMode(moodEntries, graphMode)
            .OrderBy(e => e.Date)
            .ToList();

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Load and draw custom background
        if (File.Exists(backgroundImagePath))
        {
            using var backgroundBitmap = SKBitmap.Decode(backgroundImagePath);
            if (backgroundBitmap != null)
            {
                canvas.DrawBitmap(backgroundBitmap, new SKRect(0, 0, width, height));
            }
        }
        else
        {
            // Fallback to white background if image doesn't exist
            canvas.Clear(SKColors.White);
        }

        await Task.Run(() => DrawGraphForMode(canvas, filteredEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, graphMode, false)); // Don't draw white background when using custom background

        // Convert to PNG
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private void DrawGraph(SKCanvas canvas, List<MoodEntry> entries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width, int height, Color lineColor, bool drawWhiteBackground = true)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);

        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.GetStartDate();
        var requestedEndDate = dateRange.GetEndDate();

        // Conditionally draw background
        if (drawWhiteBackground)
        {
            DrawBackground(canvas, graphArea);
        }

        // Conditionally draw grid and axes
        if (showAxesAndGrid)
        {
            DrawGrid(canvas, graphArea);
            DrawAxes(canvas, graphArea);
        }

        if (entries.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw data line and optionally points with proportional positioning
        DrawDataLine(canvas, graphArea, entries, requestedStartDate, requestedEndDate, lineColor);
        if (showDataPoints)
        {
            DrawDataPoints(canvas, graphArea, entries, requestedStartDate, requestedEndDate, lineColor);
        }

        // Conditionally draw labels
        if (showAxesAndGrid)
        {
            DrawYAxisLabels(canvas, graphArea);
            DrawXAxisLabels(canvas, graphArea, entries, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
        }

        // Conditionally draw title
        if (showTitle)
        {
            DrawTitle(canvas, width);
        }
    }

    private void DrawBackground(SKCanvas canvas, SKRect area)
    {
        DrawBackground(drawShimFactory.FromRaw(canvas), area);
    }

    private void DrawBackground(ICanvasShim canvas, SKRect area)
    {
        using var backgroundPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill
        });
        canvas.DrawRect(area, backgroundPaint);
    }

    private void DrawGrid(SKCanvas canvas, SKRect area)
    {
        using var gridPaint = new SKPaint
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash([5, 5], 0)
        };

        // Horizontal grid lines
        var yRange = MaxYValue - MinYValue;
        var yStep = area.Height / yRange;
        for (int i = MinYValue + 1; i < MaxYValue; i++)
        {
            var y = area.Bottom - ((i - MinYValue) * yStep);
            canvas.DrawLine(area.Left, y, area.Right, y, gridPaint);
        }

        // Vertical grid lines - draw fewer lines for readability
        if (area.Width > 200)
        {
            var verticalLines = Math.Min(10, (int)(area.Width / 80));
            var xStep = area.Width / verticalLines;
            for (int i = 1; i < verticalLines; i++)
            {
                var x = area.Left + (i * xStep);
                canvas.DrawLine(x, area.Top, x, area.Bottom, gridPaint);
            }
        }
    }

    private void DrawAxes(SKCanvas canvas, SKRect area)
    {
        using var axisPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };

        // Y-axis
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);

        // X-axis
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);

        // Zero line (horizontal line at y=0)
        var zeroY = area.Bottom - ((0 - MinYValue) * area.Height / (MaxYValue - MinYValue));
        using var zeroLinePaint = new SKPaint
        {
            Color = SKColors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        };
        canvas.DrawLine(area.Left, zeroY, area.Right, zeroY, zeroLinePaint);
    }

    private void DrawDataLine(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor)
    {
        if (entries.Count < 2) return;

        using var linePaint = new SKPaint
        {
            Color = new SKColor((byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        };

        using var path = new SKPath();

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        for (int i = 0; i < entries.Count; i++)
        {
            // Calculate proportional position based on actual date within the requested range
            var daysFromStart = entries[i].Date.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;

            var x = area.Left + (proportionalPosition * area.Width);
            var normalizedY = (entries[i].Value!.Value - MinYValue) / (double)(MaxYValue - MinYValue);
            var y = area.Bottom - (float)(normalizedY * area.Height);

            if (i == 0)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }

        canvas.DrawPath(path, linePaint);
    }

    private void DrawDataPoints(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor)
    {
        DrawDataPoints(drawShimFactory.FromRaw(canvas), area, entries, requestedStartDate, requestedEndDate, lineColor);
    }

    private void DrawDataPoints(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor)
    {
        using var pointPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180)), // Slightly darker than line color
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;

        for (int i = 0; i < entries.Count; i++)
        {
            // Calculate proportional position based on actual date within the requested range
            var daysFromStart = entries[i].Date.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;

            var x = area.Left + (proportionalPosition * area.Width);
            var normalizedY = (entries[i].Value!.Value - MinYValue) / (double)(MaxYValue - MinYValue);
            var y = area.Bottom - (float)(normalizedY * area.Height);

            canvas.DrawCircle(x, y, 4, pointPaint);
        }
    }

    private void DrawYAxisLabels(SKCanvas canvas, SKRect area)
    {
        DrawYAxisLabels(drawShimFactory.FromRaw(canvas), area);
    }

    private void DrawYAxisLabels(ICanvasShim canvas, SKRect area)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        });

        var yRange = MaxYValue - MinYValue;
        var yStep = area.Height / yRange;

        for (int i = MinYValue; i <= MaxYValue; i += 3) // Show every 3rd value to avoid crowding
        {
            var y = area.Bottom - ((i - MinYValue) * yStep);
            canvas.DrawText(i.ToString(), area.Left - 10, y + 4, labelPaint);
        }
    }

    private void DrawXAxisLabels(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor)
    {
        DrawXAxisLabels(drawShimFactory.FromRaw(canvas), area, entries, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
    }

    private void DrawXAxisLabels(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints, Color lineColor)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
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
        if (showDataPoints && entries.Count > 0 && entries.Count <= 10)
        {
            using var dataLabelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
            {
                Color = new SKColor((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180)), // Match data point color
                TextSize = 8,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            });

            foreach (var entry in entries)
            {
                var daysFromStart = entry.Date.DayNumber - requestedStartDate.DayNumber;
                var proportionalPosition = (float)daysFromStart / totalDays;
                var x = area.Left + (proportionalPosition * area.Width);

                canvas.DrawText("●", x, area.Bottom + 35, dataLabelPaint);
            }
        }
    }

    private void DrawTitle(SKCanvas canvas, int width)
    {
        DrawTitle(drawShimFactory.FromRaw(canvas), width);
    }

    private void DrawTitle(ICanvasShim canvas, int width)
    {
        using var titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        });

        canvas.DrawText("Mood Change Over Time", width / 2, 30, titlePaint);
    }

    private void DrawNoDataMessage(SKCanvas canvas, SKRect area)
    {
        DrawNoDataMessage(drawShimFactory.FromRaw(canvas), area);
    }

    private void DrawNoDataMessage(ICanvasShim canvas, SKRect area)
    {
        using var messagePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Gray,
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
        return graphMode switch
        {
            GraphMode.Impact => moodEntries.Where(e => e.Value.HasValue),
            GraphMode.Average => moodEntries.Where(e => e.GetAdjustedAverageMood().HasValue),
            _ => throw new ArgumentOutOfRangeException(nameof(graphMode), graphMode, null)
        };
    }

    /// <summary>
    /// Draws graph with mode-specific logic for data extraction
    /// </summary>
    private void DrawGraphForMode(SKCanvas canvas, List<MoodEntry> entries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width, int height, Color lineColor, GraphMode graphMode, bool drawWhiteBackground = true)
    {
        DrawGraphForMode(drawShimFactory.FromRaw(canvas), entries, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, graphMode, drawWhiteBackground);
    }

    private void DrawGraphForMode(ICanvasShim canvas, List<MoodEntry> entries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width, int height, Color lineColor, GraphMode graphMode, bool drawWhiteBackground = true)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);

        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.GetStartDate();
        var requestedEndDate = dateRange.GetEndDate();

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

        if (entries.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }

        // Draw data line and optionally points with proportional positioning
        DrawDataLineForMode(canvas, graphArea, entries, requestedStartDate, requestedEndDate, lineColor, graphMode);
        if (showDataPoints)
        {
            DrawDataPointsForMode(canvas, graphArea, entries, requestedStartDate, requestedEndDate, lineColor, graphMode);
        }

        // Conditionally draw labels
        if (showAxesAndGrid)
        {
            DrawYAxisLabelsForMode(canvas, graphArea, graphMode);
            DrawXAxisLabelsForMode(canvas, graphArea, entries, requestedStartDate, requestedEndDate, showDataPoints, lineColor, graphMode);
        }

        // Conditionally draw title
        if (showTitle)
        {
            DrawTitleForMode(canvas, width, graphMode);
        }
    }

    private void DrawGridForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        // Use existing grid method - the Y range constants will be updated based on mode
        var (minY, maxY) = GetYRangeForMode(graphMode);

        using var gridPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash([5, 5], 0)
        });

        // Horizontal grid lines
        var yRange = maxY - minY;
        var yStep = area.Height / yRange;
        for (int i = minY + 1; i < maxY; i++)
        {
            var y = area.Bottom - ((i - minY) * yStep);
            canvas.DrawLine(area.Left, y, area.Right, y, gridPaint);
        }

        // Vertical grid lines - draw fewer lines for readability
        if (area.Width > 200)
        {
            var verticalLines = Math.Min(10, (int)(area.Width / 80));
            var xStep = area.Width / verticalLines;
            for (int i = 1; i < verticalLines; i++)
            {
                var x = area.Left + (i * xStep);
                canvas.DrawLine(x, area.Top, x, area.Bottom, gridPaint);
            }
        }
    }

    private void DrawAxesForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        using var axisPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        });

        // Y-axis
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);

        // X-axis
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);

        // Zero line (horizontal line at y=0)
        var (minY, maxY) = GetYRangeForMode(graphMode);
        var zeroY = area.Bottom - ((0 - minY) * area.Height / (maxY - minY));
        using var zeroLinePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.DarkGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        });
        canvas.DrawLine(area.Left, zeroY, area.Right, zeroY, zeroLinePaint);
    }

    private void DrawDataLineForMode(ICanvasShim canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, Color lineColor, GraphMode graphMode)
    {
        if (entries.Count < 2) return;

        using var linePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        });

        using var path = new SKPath();

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;
        var (minY, maxY) = GetYRangeForMode(graphMode);

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var daysFromStart = entry.Date.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;
            var x = area.Left + (proportionalPosition * area.Width);

            var value = GetValueForMode(entry, graphMode);
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
        using var pointPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(lineColor.Red * 180), (byte)(lineColor.Green * 180), (byte)(lineColor.Blue * 180)),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        // Calculate total days in the requested range
        var totalDays = requestedEndDate.DayNumber - requestedStartDate.DayNumber;
        var (minY, maxY) = GetYRangeForMode(graphMode);

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var daysFromStart = entry.Date.DayNumber - requestedStartDate.DayNumber;
            var proportionalPosition = (float)daysFromStart / totalDays;
            var x = area.Left + (proportionalPosition * area.Width);

            var value = GetValueForMode(entry, graphMode);
            var y = (float)(area.Bottom - ((value - minY) * area.Height / (maxY - minY)));

            canvas.DrawCircle(x, y, 4, pointPaint);
        }
    }

    private void DrawYAxisLabelsForMode(ICanvasShim canvas, SKRect area, GraphMode graphMode)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
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
        // Reuse existing X-axis label logic since it doesn't depend on graph mode
        DrawXAxisLabels(canvas, area, entries, requestedStartDate, requestedEndDate, showDataPoints, lineColor);
    }

    private void DrawTitleForMode(ICanvasShim canvas, int width, GraphMode graphMode)
    {
        using var titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
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
        return graphMode switch
        {
            GraphMode.Impact => entry.Value ?? 0,
            GraphMode.Average => entry.GetAdjustedAverageMood() ?? 0,
            _ => entry.Value ?? 0
        };
    }

    // Raw Data drawing methods
    private void DrawRawDataGraph(SKCanvas canvas, List<RawMoodDataPoint> dataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width, int height, Color pointColor, bool drawWhiteBackground = true)
    {
        DrawRawDataGraph(drawShimFactory.FromRaw(canvas), dataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, pointColor, drawWhiteBackground);
    }

    /// <summary>
    /// Draws a scatter plot graph for raw mood data points
    /// </summary>
    private void DrawRawDataGraph(ICanvasShim canvas, List<RawMoodDataPoint> dataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, int width, int height, Color pointColor, bool drawWhiteBackground = true)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);

        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.GetStartDate();
        var requestedEndDate = dateRange.GetEndDate();

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
        DrawRawDataLines(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor);

        // Draw data points on top of lines
        if (showDataPoints)
        {
            DrawRawDataPoints(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor);
        }

        // Conditionally draw labels
        if (showAxesAndGrid)
        {
            DrawRawDataYAxisLabels(canvas, graphArea);
            DrawRawDataXAxisLabels(canvas, graphArea, dataPoints, startDateTime, endDateTime, pointColor);
        }

        // Conditionally draw title
        if (showTitle)
        {
            DrawRawDataTitle(canvas, width);
        }
    }

    private void DrawRawDataGrid(ICanvasShim canvas, SKRect area)
    {
        using var gridPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash([5, 5], 0)
        });

        // Horizontal grid lines for mood scale 1-10
        var yRange = 10 - 1;
        var yStep = area.Height / yRange;
        for (int i = 2; i < 10; i++) // Skip 1 and 10 to avoid edge lines
        {
            var y = area.Bottom - ((i - 1) * yStep);
            canvas.DrawLine(area.Left, y, area.Right, y, gridPaint);
        }

        // Vertical grid lines - fewer lines for datetime readability
        if (area.Width > 200)
        {
            var verticalLines = Math.Min(6, (int)(area.Width / 120));
            var xStep = area.Width / verticalLines;
            for (int i = 1; i < verticalLines; i++)
            {
                var x = area.Left + (i * xStep);
                canvas.DrawLine(x, area.Top, x, area.Bottom, gridPaint);
            }
        }
    }

    private void DrawRawDataAxes(ICanvasShim canvas, SKRect area)
    {
        using var axisPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2
        });

        // Y-axis
        canvas.DrawLine(area.Left, area.Top, area.Left, area.Bottom, axisPaint);

        // X-axis
        canvas.DrawLine(area.Left, area.Bottom, area.Right, area.Bottom, axisPaint);
    }

    private void DrawRawDataLines(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color lineColor)
    {
        if (dataPoints.Count < 2)
            return;

        using var linePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(lineColor.Red * 255), (byte)(lineColor.Green * 255), (byte)(lineColor.Blue * 255)),
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

            // Calculate Y position based on mood value (1-10 range)
            var y = (float)(area.Bottom - ((point.MoodValue - 1) * area.Height / 9)); // 9 = 10-1

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

    private void DrawRawDataPoints(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        // Create different paints for start and end of work
        using var startPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(pointColor.Red * 255), (byte)(pointColor.Green * 255), (byte)(pointColor.Blue * 255)),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        });

        using var endPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = new SKColor((byte)(pointColor.Red * 180), (byte)(pointColor.Green * 180), (byte)(pointColor.Blue * 180)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        });

        var totalTimeSpan = endDateTime - startDateTime;

        foreach (var point in dataPoints)
        {
            if (point.Timestamp >= startDateTime && point.Timestamp <= endDateTime)
            {
                // Calculate X position based on timestamp
                var timeFromStart = point.Timestamp - startDateTime;
                var proportionalPosition = (float)(timeFromStart.TotalMilliseconds / totalTimeSpan.TotalMilliseconds);
                var x = area.Left + (proportionalPosition * area.Width);

                // Calculate Y position based on mood value (1-10 range)
                var y = (float)(area.Bottom - ((point.MoodValue - 1) * area.Height / 9)); // 9 = 10-1

                // Draw different shapes for start vs end of work
                if (point.MoodType == MoodType.StartOfWork)
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

    private void DrawRawDataYAxisLabels(ICanvasShim canvas, SKRect area)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
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

    private void DrawRawDataXAxisLabels(ICanvasShim canvas, SKRect area, List<RawMoodDataPoint> dataPoints, DateTime startDateTime, DateTime endDateTime, Color pointColor)
    {
        using var labelPaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
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

    private void DrawRawDataTitle(ICanvasShim canvas, int width)
    {
        using IPaintShim titlePaint = drawShimFactory.PaintFromArgs(new PaintShimArgs
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        });

        canvas.DrawText("Raw Mood Data Over Time", width / 2, 30, titlePaint);
    }

    // New save methods with GraphMode support

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, GraphMode graphMode, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, graphMode, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, GraphMode graphMode, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, showAxesAndGrid, showTitle, graphMode, backgroundImagePath, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    // Raw Data graph implementations

    public async Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, Color lineColor, int width = 800, int height = 600)
    {
        var sortedPoints = rawDataPoints.OrderBy(p => p.Timestamp).ToList();

        using var bitmap = drawShimFactory.BitmapFromDimensions(width, height);
        using var canvas = drawShimFactory.CanvasFromBitmap(bitmap);

        // Clear canvas with white background
        canvas.Clear(SKColors.White);

        await Task.Run(() => DrawRawDataGraph(canvas.Raw, sortedPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, true));

        // using var image = SKImage.FromBitmap(bitmap);
        using IImageShim image = drawShimFactory.ImageFromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    public async Task<byte[]> GenerateRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var sortedPoints = rawDataPoints.OrderBy(p => p.Timestamp).ToList();

        using var bitmap = drawShimFactory.BitmapFromDimensions(width, height);
        using var canvas = drawShimFactory.CanvasFromBitmap(bitmap);

        // Load and draw custom background
        if (File.Exists(backgroundImagePath))
        {
            using var backgroundBitmap = drawShimFactory.DecodeBitmapFromFile(backgroundImagePath);
            if (backgroundBitmap != null)
            {
                canvas.DrawBitmap(backgroundBitmap, new SKRect(0, 0, width, height));
            }
        }
        else
        {
            // Fallback to white background if image doesn't exist
            canvas.Clear(SKColors.White);
        }

        await Task.Run(() => DrawRawDataGraph(canvas.Raw, sortedPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, width, height, lineColor, false));

        using IImageShim image = drawShimFactory.ImageFromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public async Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateRawDataGraphAsync(rawDataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task SaveRawDataGraphAsync(IEnumerable<RawMoodDataPoint> rawDataPoints, DateRange dateRange, bool showDataPoints, bool showAxesAndGrid, bool showTitle, string filePath, string backgroundImagePath, Color lineColor, int width = 800, int height = 600)
    {
        var imageData = await GenerateRawDataGraphAsync(rawDataPoints, dateRange, showDataPoints, showAxesAndGrid, showTitle, backgroundImagePath, lineColor, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }
}