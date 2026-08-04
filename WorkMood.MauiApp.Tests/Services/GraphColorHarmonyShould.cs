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

    #region PureGrayscale Mode Tests

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnLightGray_ForDarkLineColor_WhenPureGrayscaleModeRequested()
    {
        // Arrange: Dark blue line (luminance < 0.5)
        var darkLineColor = Color.FromRgb(0.05f, 0.15f, 0.65f);

        // Act
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(darkLineColor, GraphColorSuggestionMode.PureGrayscale);

        // Assert: Should return light gray (achromatic with high value)
        Assert.Equal(result.Red, result.Green, 3);  // Must be grayscale (R=G=B)
        Assert.Equal(result.Green, result.Blue, 3);
        Assert.True(result.Red > 0.5f, "For dark line, should return light gray (value > 0.5)");
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnDarkGray_ForLightLineColor_WhenPureGrayscaleModeRequested()
    {
        // Arrange: Light yellow line (luminance >= 0.5)
        var lightLineColor = Color.FromRgb(0.95f, 0.9f, 0.35f);

        // Act
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(lightLineColor, GraphColorSuggestionMode.PureGrayscale);

        // Assert: Should return dark gray (achromatic with low value)
        Assert.Equal(result.Red, result.Green, 3);  // Must be grayscale (R=G=B)
        Assert.Equal(result.Green, result.Blue, 3);
        Assert.True(result.Red < 0.5f, "For light line, should return dark gray (value < 0.5)");
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnMidtoneGray_ForMidtoneLineColor_WhenPureGrayscaleModeRequested()
    {
        // Arrange: Midtone grey line
        var midtoneGreyColor = Color.FromRgb(0.5f, 0.5f, 0.5f);

        // Act
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(midtoneGreyColor, GraphColorSuggestionMode.PureGrayscale);

        // Assert: Should return midtone gray (R=G=B ~= 0.5)
        Assert.Equal(result.Red, result.Green, 3);
        Assert.Equal(result.Green, result.Blue, 3);
        Assert.True(Math.Abs(result.Red - 0.5f) < 0.1f, "For midtone line, should return near-midtone gray");
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnTrueGrayscale_ForAllColors_WhenPureGrayscaleModeRequested()
    {
        // Arrange: Various test colors
        var testColors = new[]
        {
            Color.FromRgb(0.1f, 0.1f, 0.1f),   // Very dark
            Color.FromRgb(0.2f, 0.3f, 0.5f),   // Dark blue
            Color.FromRgb(0.5f, 0.5f, 0.5f),   // Mid gray
            Color.FromRgb(0.9f, 0.2f, 0.2f),   // Light red
            Color.FromRgb(0.9f, 0.9f, 0.9f),   // Very light
        };

        // Act & Assert
        foreach (var lineColor in testColors)
        {
            var result = GraphColorHarmony.GetSuggestedBackgroundColor(lineColor, GraphColorSuggestionMode.PureGrayscale);

            // Result must be pure grayscale (R=G=B)
            Assert.Equal(result.Red, result.Green, 3);
            Assert.Equal(result.Green, result.Blue, 3);
            
            // Result should not be pure black or pure white (should be gray)
            var isPureBlack = result.Red == 0f && result.Green == 0f && result.Blue == 0f;
            var isPureWhite = result.Red == 1f && result.Green == 1f && result.Blue == 1f;
            Assert.False(isPureBlack || isPureWhite,
                $"For line color with luminance {CalculateRelativeLuminance(lineColor):F3}, " +
                $"expected a shade of gray but got RGB({result.Red:F3}, {result.Green:F3}, {result.Blue:F3})");
        }
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldInvertLuminance_ForPureGrayscaleMode()
    {
        // Arrange: Test that grayscale value is inverted from luminance
        var darkColor = Color.FromRgb(0.2f, 0.2f, 0.2f);   // Low luminance
        var brightColor = Color.FromRgb(0.8f, 0.8f, 0.8f);  // High luminance

        // Act
        var darkResult = GraphColorHarmony.GetSuggestedBackgroundColor(darkColor, GraphColorSuggestionMode.PureGrayscale);
        var brightResult = GraphColorHarmony.GetSuggestedBackgroundColor(brightColor, GraphColorSuggestionMode.PureGrayscale);

        // Assert: Darker input should produce lighter gray output (inverted)
        Assert.True(darkResult.Red > brightResult.Red,
            $"Darker line color should produce lighter gray background. " +
            $"Dark input result: {darkResult.Red:F3}, Bright input result: {brightResult.Red:F3}");
    }

    #endregion

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