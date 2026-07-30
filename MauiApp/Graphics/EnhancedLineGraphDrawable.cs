using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Graphics;

/// <summary>
/// Interface for drawing components following Single Responsibility Principle
/// </summary>
public interface IGraphComponent
{
    void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data);
}

/// <summary>
/// Enhanced drawable for rendering mood visualization graphs following SOLID principles
/// </summary>
public class EnhancedLineGraphDrawable : IDrawable
{
    private readonly MoodVisualizationData _data;
    private readonly IList<IGraphComponent> _components;
    
    public EnhancedLineGraphDrawable(MoodVisualizationData data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _components = CreateComponents();
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_data?.DailyValues == null) return;
        
        // Draw background
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);
        
        // Draw each component
        foreach (var component in _components)
        {
            component.Draw(canvas, dirtyRect, _data);
        }
    }
    
    private static IList<IGraphComponent> CreateComponents()
    {
        return new List<IGraphComponent>
        {
            new GridComponent(),
            new BaselineComponent(),
            new LineComponent(),
            new DataPointComponent(),
            new MissingDataComponent()
        };
    }
}

internal static class VisualizationGraphLayout
{
    private const float Margin = 20f;

    public static float MarginSize => Margin;

    public static float[] GetPointPositions(RectF bounds, int slotCount)
    {
        if (slotCount <= 0)
        {
            return Array.Empty<float>();
        }

        var graphWidth = bounds.Width - (Margin * 2);

        if (slotCount == 1)
        {
            return new[] { Margin + (graphWidth / 2f) };
        }

        var pointSpacing = graphWidth / (slotCount - 1);

        return Enumerable.Range(0, slotCount)
            .Select(index => Margin + (index * pointSpacing))
            .ToArray();
    }
}

internal readonly record struct VisualizationDisplayContext(
    IReadOnlyList<DailyMoodValue> DisplayValues,
    float[] XPositions,
    float Width,
    float Height,
    float Margin,
    float GraphWidth,
    float GraphHeight,
    float CenterY,
    double MaxAbsValue,
    double ScaleFactor)
{
    public static VisualizationDisplayContext Create(RectF bounds, MoodVisualizationData data)
    {
        var displayValues = VisualizationDisplayProjector.GetDisplayValues(data);
        var xPositions = VisualizationGraphLayout.GetPointPositions(bounds, displayValues.Count);
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = VisualizationGraphLayout.MarginSize;
        var graphWidth = width - (margin * 2);
        var graphHeight = height - (margin * 2);
        var centerY = margin + (graphHeight / 2f);
        var maxAbsValue = Math.Max(1.0, data.MaxAbsoluteValue);
        var scaleFactor = graphHeight / (2.0 * maxAbsValue);

        return new VisualizationDisplayContext(
            displayValues,
            xPositions,
            width,
            height,
            margin,
            graphWidth,
            graphHeight,
            centerY,
            maxAbsValue,
            scaleFactor);
    }
}

/// <summary>
/// Component for drawing grid lines
/// </summary>
public class GridComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var context = VisualizationDisplayContext.Create(bounds, data);
        
        canvas.StrokeColor = Colors.LightGray;
        canvas.StrokeSize = 0.5f;
        
        if (context.XPositions.Length == 1)
        {
            canvas.DrawLine(context.XPositions[0], context.Margin, context.XPositions[0], context.Height - context.Margin);
        }
        else
        {
            var pointSpacing = context.XPositions.Length > 1 ? context.GraphWidth / (context.XPositions.Length - 1) : 0f;

            for (int i = 0; i <= context.XPositions.Length; i++)
            {
                var x = context.Margin + (i * pointSpacing);
                canvas.DrawLine(x, context.Margin, x, context.Height - context.Margin);
            }
        }
        
        // Horizontal grid lines based on data range
        var gridInterval = context.MaxAbsValue <= 3 ? 1 : Math.Ceiling(context.MaxAbsValue / 3);
        
        for (double i = -context.MaxAbsValue; i <= context.MaxAbsValue; i += gridInterval)
        {
            if (Math.Abs(i) < 0.001) continue; // Skip center line
            var y = context.CenterY - (float)(i * context.ScaleFactor);
            canvas.DrawLine(context.Margin, y, context.Width - context.Margin, y);
        }
    }
}

/// <summary>
/// Component for drawing the baseline (zero line)
/// </summary>
public class BaselineComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = 20f;
        var graphHeight = height - (margin * 2);
        var centerY = margin + (graphHeight / 2f);
        
        canvas.StrokeColor = Color.FromRgba(200, 200, 200, 255);
        canvas.StrokeSize = 1f;
        canvas.DrawLine(margin, centerY, width - margin, centerY);
    }
}

/// <summary>
/// Component for drawing connecting lines between data points
/// </summary>
public class LineComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var context = VisualizationDisplayContext.Create(bounds, data);
        
        // Collect points with data, keeping each point's calendar date so a missing day can
        // break the line instead of being bridged by it.
        var dataPoints = new List<(DateOnly Date, PointF Point)>();
        
        for (int day = 0; day < context.DisplayValues.Count && day < context.XPositions.Length; day++)
        {
            var dailyValue = context.DisplayValues[day];
            if (dailyValue.HasData && dailyValue.Value.HasValue)
            {
                var x = context.XPositions[day];
                var value = (float)dailyValue.Value.Value;
                var y = context.CenterY - (float)(value * context.ScaleFactor);
                
                dataPoints.Add((dailyValue.Date, new PointF(x, y)));
            }
        }
        
        // Draw connecting lines only within calendar-contiguous segments; a missing day between
        // two recorded days breaks the line instead of bridging the gap.
        var segments = GraphLineSegmentBuilder.BuildSegments(
            dataPoints,
            p => p.Date,
            CalendarGapPolicy.ShouldBreakWhenWeekdaysMissing);

        if (segments.Any(segment => segment.Count > 1))
        {
            canvas.StrokeColor = Colors.DarkBlue;
            canvas.StrokeSize = 2f;

            foreach (var segment in segments)
            {
                for (int i = 0; i < segment.Count - 1; i++)
                {
                    canvas.DrawLine(segment[i].Point, segment[i + 1].Point);
                }
            }
        }
    }
}

/// <summary>
/// Component for drawing data points as circles
/// </summary>
public class DataPointComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var context = VisualizationDisplayContext.Create(bounds, data);
        
        for (int day = 0; day < context.DisplayValues.Count && day < context.XPositions.Length; day++)
        {
            var dailyValue = context.DisplayValues[day];
            if (dailyValue.HasData && dailyValue.Value.HasValue)
            {
                var x = context.XPositions[day];
                var value = (float)dailyValue.Value.Value;
                var y = context.CenterY - (float)(value * context.ScaleFactor);
                
                // Draw filled circle for data point
                canvas.FillColor = dailyValue.Color;
                canvas.FillCircle(x, y, 4f);
                
                // Draw border around circle
                canvas.StrokeColor = Colors.DarkGray;
                canvas.StrokeSize = 1f;
                canvas.DrawCircle(x, y, 4f);
            }
        }
    }
}

/// <summary>
/// Component for drawing missing data indicators
/// </summary>
public class MissingDataComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var context = VisualizationDisplayContext.Create(bounds, data);
        
        // Draw missing data indicators (gray dots on zero line)
        for (int day = 0; day < context.DisplayValues.Count && day < context.XPositions.Length; day++)
        {
            var dailyValue = context.DisplayValues[day];
            if (!dailyValue.HasData || !dailyValue.Value.HasValue)
            {
                var x = context.XPositions[day];
                canvas.FillColor = Colors.LightGray;
                canvas.FillCircle(x, context.CenterY, 2f);
            }
        }
    }
}