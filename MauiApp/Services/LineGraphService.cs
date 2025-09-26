using SkiaSharp;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

/// <summary>
/// Implementation of line graph service using SkiaSharp for rendering
/// </summary>
public class LineGraphService : ILineGraphService
{
    private const int MinYValue = -9;
    private const int MaxYValue = 9;
    private const int Padding = 60;
    private const int GridLineSpacing = 20;
    
    public async Task<byte[]> GenerateLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, int width = 800, int height = 600)
    {
        var filteredEntries = moodEntries
            .Where(e => e.Value.HasValue)
            .OrderBy(e => e.Date)
            .ToList();

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        
        // Clear canvas with white background
        canvas.Clear(SKColors.White);
        
        await Task.Run(() => DrawGraph(canvas, filteredEntries, dateRange, showDataPoints, width, height));
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return data.ToArray();
    }
    
    public async Task SaveLineGraphAsync(IEnumerable<MoodEntry> moodEntries, DateRange dateRange, bool showDataPoints, string filePath, int width = 800, int height = 600)
    {
        var imageData = await GenerateLineGraphAsync(moodEntries, dateRange, showDataPoints, width, height);
        await File.WriteAllBytesAsync(filePath, imageData);
    }
    
    private void DrawGraph(SKCanvas canvas, List<MoodEntry> entries, DateRange dateRange, bool showDataPoints, int width, int height)
    {
        var graphArea = new SKRect(Padding, Padding, width - Padding, height - Padding);
        
        // Calculate the full date range for proportional positioning
        var requestedStartDate = dateRange.GetStartDate();
        var requestedEndDate = dateRange.GetEndDate();
        
        // Draw background and grid
        DrawBackground(canvas, graphArea);
        DrawGrid(canvas, graphArea);
        DrawAxes(canvas, graphArea);
        
        if (entries.Count == 0)
        {
            DrawNoDataMessage(canvas, graphArea);
            return;
        }
        
        // Draw data line and optionally points with proportional positioning
        DrawDataLine(canvas, graphArea, entries, requestedStartDate, requestedEndDate);
        if (showDataPoints)
        {
            DrawDataPoints(canvas, graphArea, entries, requestedStartDate, requestedEndDate);
        }
        
        // Draw labels
        DrawYAxisLabels(canvas, graphArea);
        DrawXAxisLabels(canvas, graphArea, entries, requestedStartDate, requestedEndDate, showDataPoints);
        DrawTitle(canvas, width);
    }
    
    private void DrawBackground(SKCanvas canvas, SKRect area)
    {
        using var backgroundPaint = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill
        };
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
    
    private void DrawDataLine(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate)
    {
        if (entries.Count < 2) return;
        
        using var linePaint = new SKPaint
        {
            Color = SKColors.Blue,
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
    
    private void DrawDataPoints(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate)
    {
        using var pointPaint = new SKPaint
        {
            Color = SKColors.DarkBlue,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        
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
        using var labelPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        
        var yRange = MaxYValue - MinYValue;
        var yStep = area.Height / yRange;
        
        for (int i = MinYValue; i <= MaxYValue; i += 3) // Show every 3rd value to avoid crowding
        {
            var y = area.Bottom - ((i - MinYValue) * yStep);
            canvas.DrawText(i.ToString(), area.Left - 10, y + 4, labelPaint);
        }
    }
    
    private void DrawXAxisLabels(SKCanvas canvas, SKRect area, List<MoodEntry> entries, DateOnly requestedStartDate, DateOnly requestedEndDate, bool showDataPoints)
    {
        using var labelPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 10,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        
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
            using var dataLabelPaint = new SKPaint
            {
                Color = SKColors.DarkBlue,
                TextSize = 8,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            
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
        using var titlePaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };
        
        canvas.DrawText("Mood Change Over Time", width / 2, 30, titlePaint);
    }
    
    private void DrawNoDataMessage(SKCanvas canvas, SKRect area)
    {
        using var messagePaint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 14,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        
        var centerX = area.Left + area.Width / 2;
        var centerY = area.Top + area.Height / 2;
        
        canvas.DrawText("No mood data available for the selected period", centerX, centerY, messagePaint);
    }
}