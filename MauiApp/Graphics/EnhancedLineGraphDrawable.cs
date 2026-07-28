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

/// <summary>
/// Component for drawing grid lines
/// </summary>
public class GridComponent : IGraphComponent
{
    public void Draw(ICanvas canvas, RectF bounds, MoodVisualizationData data)
    {
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = 20f;
        var graphWidth = width - (margin * 2);
        var graphHeight = height - (margin * 2);
        var pointSpacing = graphWidth / 13f; // 13 gaps between 14 points
        
        canvas.StrokeColor = Colors.LightGray;
        canvas.StrokeSize = 0.5f;
        
        // Vertical grid lines (one for each day)
        for (int i = 0; i <= 14; i++)
        {
            var x = margin + (i * pointSpacing);
            canvas.DrawLine(x, margin, x, height - margin);
        }
        
        // Horizontal grid lines based on data range
        var centerY = margin + (graphHeight / 2f);
        var maxAbsValue = Math.Max(1.0, data.MaxAbsoluteValue);
        var scaleFactor = graphHeight / (2.0 * maxAbsValue);
        var gridInterval = maxAbsValue <= 3 ? 1 : Math.Ceiling(maxAbsValue / 3);
        
        for (double i = -maxAbsValue; i <= maxAbsValue; i += gridInterval)
        {
            if (Math.Abs(i) < 0.001) continue; // Skip center line
            var y = centerY - (float)(i * scaleFactor);
            canvas.DrawLine(margin, y, width - margin, y);
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
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = 20f;
        var graphWidth = width - (margin * 2);
        var graphHeight = height - (margin * 2);
        var pointSpacing = graphWidth / 13f;
        var centerY = margin + (graphHeight / 2f);
        var maxAbsValue = Math.Max(1.0, data.MaxAbsoluteValue);
        var scaleFactor = graphHeight / (2.0 * maxAbsValue);
        
        // Collect points with data, keeping each point's calendar date so a missing day can
        // break the line instead of being bridged by it.
        var dataPoints = new List<(DateOnly Date, PointF Point)>();
        
        for (int day = 0; day < 14 && day < data.DailyValues.Length; day++)
        {
            var dailyValue = data.DailyValues[day];
            if (dailyValue.HasData && dailyValue.Value.HasValue)
            {
                var x = margin + (day * pointSpacing);
                var value = (float)dailyValue.Value.Value;
                var y = centerY - (float)(value * scaleFactor);
                
                dataPoints.Add((dailyValue.Date, new PointF(x, y)));
            }
        }
        
        // Draw connecting lines only within calendar-contiguous segments; a missing day between
        // two recorded days breaks the line instead of bridging the gap.
        var segments = GraphLineSegmentBuilder.BuildSegments(dataPoints, p => p.Date);

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
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = 20f;
        var graphWidth = width - (margin * 2);
        var graphHeight = height - (margin * 2);
        var pointSpacing = graphWidth / 13f;
        var centerY = margin + (graphHeight / 2f);
        var maxAbsValue = Math.Max(1.0, data.MaxAbsoluteValue);
        var scaleFactor = graphHeight / (2.0 * maxAbsValue);
        
        for (int day = 0; day < 14 && day < data.DailyValues.Length; day++)
        {
            var dailyValue = data.DailyValues[day];
            if (dailyValue.HasData && dailyValue.Value.HasValue)
            {
                var x = margin + (day * pointSpacing);
                var value = (float)dailyValue.Value.Value;
                var y = centerY - (float)(value * scaleFactor);
                
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
        var width = bounds.Width;
        var height = bounds.Height;
        var margin = 20f;
        var graphWidth = width - (margin * 2);
        var graphHeight = height - (margin * 2);
        var pointSpacing = graphWidth / 13f;
        var centerY = margin + (graphHeight / 2f);
        
        // Draw missing data indicators (gray dots on zero line)
        for (int day = 0; day < 14; day++)
        {
            var dailyValue = data.DailyValues[day];
            if (!dailyValue.HasData || !dailyValue.Value.HasValue)
            {
                var x = margin + (day * pointSpacing);
                canvas.FillColor = Colors.LightGray;
                canvas.FillCircle(x, centerY, 2f);
            }
        }
    }
}