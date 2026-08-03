using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Services;

public enum GraphColorSuggestionMode
{
    HighContrast,
    Complementary,
    FirstTriadic,
    SecondTriadic
}

public static class GraphColorHarmony
{
    public static Color GetDerivedColor(Color primaryColor, GapSegmentSecondaryPenMode gapSegmentSecondaryPenMode)
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

    public static Color GetSuggestedBackgroundColor(Color lineColor, GraphColorSuggestionMode suggestionMode)
    {
        return suggestionMode switch
        {
            GraphColorSuggestionMode.HighContrast => GetHighContrastBackgroundColor(lineColor),
            GraphColorSuggestionMode.Complementary => GetDerivedColor(lineColor, GapSegmentSecondaryPenMode.Complementary),
            GraphColorSuggestionMode.FirstTriadic => GetDerivedColor(lineColor, GapSegmentSecondaryPenMode.FirstTriadic),
            GraphColorSuggestionMode.SecondTriadic => GetDerivedColor(lineColor, GapSegmentSecondaryPenMode.SecondTriadic),
            _ => Colors.White
        };
    }

    public static Color GetReadableTextColor(Color backgroundColor)
    {
        return CalculateLuminance(backgroundColor) >= 0.5f ? Colors.Black : Colors.White;
    }

    private static Color GetHighContrastBackgroundColor(Color lineColor)
    {
        var invertedColor = Color.FromRgb(1f - lineColor.Red, 1f - lineColor.Green, 1f - lineColor.Blue);
        var (hue, saturation, _) = RgbToHsv(invertedColor);
        var targetValue = CalculateLuminance(lineColor) < 0.5f ? 0.95f : 0.18f;

        return HsvToColor(hue, saturation, targetValue);
    }

    private static float CalculateLuminance(Color color)
    {
        return (float)((0.299 * color.Red) + (0.587 * color.Green) + (0.114 * color.Blue));
    }

    private static (float Hue, float Saturation, float Value) RgbToHsv(Color color)
    {
        var red = color.Red;
        var green = color.Green;
        var blue = color.Blue;

        var max = Math.Max(red, Math.Max(green, blue));
        var min = Math.Min(red, Math.Min(green, blue));
        var delta = max - min;

        if (delta == 0f)
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
}