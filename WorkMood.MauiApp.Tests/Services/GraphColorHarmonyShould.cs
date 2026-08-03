using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using Xunit;

namespace WorkMood.MauiApp.Tests.Services;

public class GraphColorHarmonyShould
{
    [Theory]
    [InlineData(GapSegmentSecondaryPenMode.Complementary, 0f, 1f, 1f)]
    [InlineData(GapSegmentSecondaryPenMode.FirstTriadic, 0f, 1f, 0f)]
    [InlineData(GapSegmentSecondaryPenMode.SecondTriadic, 0f, 0f, 1f)]
    public void GetDerivedColor_ShouldReturnExpectedHueRotation_ForRedAnchors(GapSegmentSecondaryPenMode mode, float expectedRed, float expectedGreen, float expectedBlue)
    {
        var result = GraphColorHarmony.GetDerivedColor(Colors.Red, mode);

        Assert.Equal(expectedRed, result.Red, 3);
        Assert.Equal(expectedGreen, result.Green, 3);
        Assert.Equal(expectedBlue, result.Blue, 3);
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnLightChromaticContrast_ForDarkLine_WhenHighContrastRequested()
    {
        var lineColor = Color.FromRgb(0.05f, 0.15f, 0.65f);
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(lineColor, GraphColorSuggestionMode.HighContrast);

        Assert.NotEqual(Colors.White, result);
        Assert.NotEqual(Colors.Black, result);
        Assert.True(CalculateRelativeLuminance(result) > CalculateRelativeLuminance(lineColor));
        Assert.True(CalculateContrastRatio(lineColor, result) >= 3.0d);
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnDarkChromaticContrast_ForLightLine_WhenHighContrastRequested()
    {
        var lineColor = Color.FromRgb(0.95f, 0.9f, 0.35f);
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(lineColor, GraphColorSuggestionMode.HighContrast);

        Assert.NotEqual(Colors.White, result);
        Assert.NotEqual(Colors.Black, result);
        Assert.True(CalculateRelativeLuminance(result) < CalculateRelativeLuminance(lineColor));
        Assert.True(CalculateContrastRatio(lineColor, result) >= 3.0d);
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnOppositeToneNeutral_ForNeutralLine_WhenHighContrastRequested()
    {
        var lineColor = Color.FromRgb(0.5f, 0.5f, 0.5f);
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(lineColor, GraphColorSuggestionMode.HighContrast);

        Assert.NotEqual(lineColor, result);
        Assert.Equal(result.Red, result.Green, 3);
        Assert.Equal(result.Green, result.Blue, 3);
        Assert.True(CalculateContrastRatio(lineColor, result) >= 3.0d);
    }

    [Fact]
    public void GetDerivedColor_ShouldKeepGrayscaleInputGrayscale()
    {
        var result = GraphColorHarmony.GetDerivedColor(Colors.Gray, GapSegmentSecondaryPenMode.Complementary);

        Assert.Equal(result.Red, result.Green, 3);
        Assert.Equal(result.Green, result.Blue, 3);
    }

    private static double CalculateContrastRatio(Color first, Color second)
    {
        var firstLuminance = CalculateRelativeLuminance(first);
        var secondLuminance = CalculateRelativeLuminance(second);
        var lighter = Math.Max(firstLuminance, secondLuminance);
        var darker = Math.Min(firstLuminance, secondLuminance);

        return (lighter + 0.05d) / (darker + 0.05d);
    }

    private static double CalculateRelativeLuminance(Color color)
    {
        return (0.2126d * ConvertToLinear(color.Red))
            + (0.7152d * ConvertToLinear(color.Green))
            + (0.0722d * ConvertToLinear(color.Blue));
    }

    private static double ConvertToLinear(float channel)
    {
        return channel <= 0.03928f
            ? channel / 12.92d
            : Math.Pow((channel + 0.055d) / 1.055d, 2.4d);
    }
}