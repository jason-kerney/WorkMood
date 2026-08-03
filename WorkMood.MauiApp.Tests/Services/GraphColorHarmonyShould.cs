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
    public void GetSuggestedBackgroundColor_ShouldReturnWhiteForDarkLine_WhenHighContrastRequested()
    {
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(Colors.DarkBlue, GraphColorSuggestionMode.HighContrast);

        Assert.Equal(Colors.White, result);
    }

    [Fact]
    public void GetSuggestedBackgroundColor_ShouldReturnBlackForLightLine_WhenHighContrastRequested()
    {
        var result = GraphColorHarmony.GetSuggestedBackgroundColor(Colors.LightYellow, GraphColorSuggestionMode.HighContrast);

        Assert.Equal(Colors.Black, result);
    }

    [Fact]
    public void GetDerivedColor_ShouldKeepGrayscaleInputGrayscale()
    {
        var result = GraphColorHarmony.GetDerivedColor(Colors.Gray, GapSegmentSecondaryPenMode.Complementary);

        Assert.Equal(result.Red, result.Green, 3);
        Assert.Equal(result.Green, result.Blue, 3);
    }
}