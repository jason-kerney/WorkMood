using Microsoft.Maui.Graphics;
using System.Globalization;
using WorkMood.MauiApp.Converters;
using Xunit;

namespace WorkMood.MauiApp.Tests.Converters;

/// <summary>
/// Tests for BoolToColorConverter - parameter-based boolean to color conversion
/// Component 14 testing following established IValueConverter patterns
/// </summary>
public class BoolToColorConverterShould
{
    private readonly BoolToColorConverter _converter;

    public BoolToColorConverterShould()
    {
        _converter = new BoolToColorConverter();
    }

    #region Checkpoint 1: Basic Structure

    [Fact]
    public void ImplementIValueConverterInterface()
    {
        // Verify the converter implements the required interface
        Assert.IsAssignableFrom<Microsoft.Maui.Controls.IValueConverter>(_converter);
    }

    [Fact]
    public void Convert_WithValidBooleanAndParameter_ReturnsColor()
    {
        // Test basic Convert method with valid inputs
        var result = _converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.NotNull(result);
        Assert.IsType<Color>(result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Verify ConvertBack is not implemented (unidirectional converter)
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(Colors.White, typeof(bool), "White,Black", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsTransparent()
    {
        // Test null input handling
        var result = _converter.Convert(null, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithNonBooleanValue_ReturnsTransparent()
    {
        // Test non-boolean input handling
        var result = _converter.Convert("not a boolean", typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithNullParameter_ReturnsTransparent()
    {
        // Test null parameter handling
        var result = _converter.Convert(true, typeof(Color), null, CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithNonStringParameter_ReturnsTransparent()
    {
        // Test non-string parameter handling
        var result = _converter.Convert(true, typeof(Color), 123, CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void HasPublicParameterlessConstructor()
    {
        // Verify XAML compatibility
        var instance = new BoolToColorConverter();
        Assert.NotNull(instance);
    }

    #endregion

    #region Checkpoint 2: Core Logic - Boolean Selection

    [Fact]
    public void Convert_WithTrueValue_ReturnsFirstColor()
    {
        // Test true value selects first color in parameter
        var result = _converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, result);
    }

    [Fact]
    public void Convert_WithFalseValue_ReturnsSecondColor()
    {
        // Test false value selects second color in parameter
        var result = _converter.Convert(false, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Black, result);
    }

    [Fact]
    public void Convert_WithTrueAndReversedColors_ReturnsFirstColor()
    {
        // Test true always selects first color regardless of order
        var result = _converter.Convert(true, typeof(Color), "Black,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Black, result);
    }

    [Fact]
    public void Convert_WithFalseAndReversedColors_ReturnsSecondColor()
    {
        // Test false always selects second color regardless of order
        var result = _converter.Convert(false, typeof(Color), "Black,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, result);
    }

    #endregion

    #region Checkpoint 2: Core Logic - Predefined Colors

    [Theory]
    [InlineData("White", typeof(Color))]
    [InlineData("Black", typeof(Color))]
    [InlineData("Transparent", typeof(Color))]
    [InlineData("Gray", typeof(Color))]
    public void Convert_WithPredefinedColorNames_ReturnsCorrectColorType(string colorName, Type expectedType)
    {
        // Test predefined color name recognition
        var parameter = $"{colorName},Black";
        var result = _converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
        
        Assert.IsType(expectedType, result);
    }

    [Fact]
    public void Convert_WithWhiteColorName_ReturnsWhiteColor()
    {
        // Test White color name recognition
        var result = _converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, result);
    }

    [Fact]
    public void Convert_WithBlackColorName_ReturnsBlackColor()
    {
        // Test Black color name recognition
        var result = _converter.Convert(true, typeof(Color), "Black,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Black, result);
    }

    [Fact]
    public void Convert_WithTransparentColorName_ReturnsTransparentColor()
    {
        // Test Transparent color name recognition
        var result = _converter.Convert(true, typeof(Color), "Transparent,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithGrayColorName_ReturnsGrayColor()
    {
        // Test Gray color name recognition
        var result = _converter.Convert(true, typeof(Color), "Gray,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Gray, result);
    }

    [Fact]
    public void Convert_WithCustomHexColor_ReturnsColorFromArgb()
    {
        // Test custom hex color parsing via Color.FromArgb
        var result = _converter.Convert(true, typeof(Color), "#FF0000,White", CultureInfo.InvariantCulture);
        
        Assert.NotNull(result);
        Assert.IsType<Color>(result);
        // Note: Color.FromArgb("#FF0000") should return red color
    }

    #endregion

    #region Checkpoint 2: Core Logic - Parameter Processing

    [Fact]
    public void Convert_WithWhitespaceInParameter_TrimsCorrectly()
    {
        // Test whitespace trimming in parameter parsing
        var result = _converter.Convert(true, typeof(Color), " White , Black ", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, result);
    }

    [Fact]
    public void Convert_WithSingleColorParameter_ReturnsTransparent()
    {
        // Test invalid parameter format (only one color)
        var result = _converter.Convert(true, typeof(Color), "White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithEmptyParameter_ReturnsTransparent()
    {
        // Test empty parameter string
        var result = _converter.Convert(true, typeof(Color), "", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithThreeColorParameter_ReturnsTransparent()
    {
        // Test invalid parameter format (too many colors)
        var result = _converter.Convert(true, typeof(Color), "White,Black,Gray", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    [Fact]
    public void Convert_WithNoCommaInParameter_ReturnsTransparent()
    {
        // Test parameter without comma separator
        var result = _converter.Convert(true, typeof(Color), "WhiteBlack", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, result);
    }

    #endregion

    #region Checkpoint 3: Integration Patterns - Real-world XAML Scenarios

    [Fact]
    public void Convert_WithMoodRecordingButtonColors_WorksCorrectly()
    {
        // Test actual parameter values from MoodRecording.xaml
        var backgroundResult = _converter.Convert(true, typeof(Color), "White,Transparent", CultureInfo.InvariantCulture);
        var textResult = _converter.Convert(true, typeof(Color), "Black,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, backgroundResult);
        Assert.Equal(Colors.Black, textResult);
    }

    [Fact]
    public void Convert_WithMoodRecordingUnselectedState_WorksCorrectly()
    {
        // Test unselected state from MoodRecording.xaml
        var backgroundResult = _converter.Convert(false, typeof(Color), "White,Transparent", CultureInfo.InvariantCulture);
        var textResult = _converter.Convert(false, typeof(Color), "Black,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.Transparent, backgroundResult);
        Assert.Equal(Colors.White, textResult);
    }

    [Fact]
    public void Convert_IsCultureIndependent()
    {
        // Test culture independence
        var resultInvariant = _converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        var resultGerman = _converter.Convert(true, typeof(Color), "White,Black", new CultureInfo("de-DE"));
        
        Assert.Equal(resultInvariant, resultGerman);
    }

    [Fact]
    public void Convert_WithDifferentTargetTypes_StillReturnsColor()
    {
        // Test with different target types (XAML flexibility)
        var result1 = _converter.Convert(true, typeof(object), "White,Black", CultureInfo.InvariantCulture);
        var result2 = _converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);
        
        Assert.Equal(result1, result2);
        Assert.IsType<Color>(result1);
    }

    [Fact]
    public void Convert_PerformanceWithComplexColors_IsAcceptable()
    {
        // Test performance with complex color parsing
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            _converter.Convert(i % 2 == 0, typeof(Color), "#FF0000,#00FF00", CultureInfo.InvariantCulture);
        }
        
        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 100, "Performance should be acceptable for UI binding");
    }

    #endregion

    #region Checkpoint 3: Integration Patterns - Edge Cases

    [Fact]
    public void Convert_WithInvalidHexColor_HandlesGracefully()
    {
        // Test invalid hex color handling
        // Note: This tests Color.FromArgb behavior with invalid input
        var result = _converter.Convert(true, typeof(Color), "InvalidColor,White", CultureInfo.InvariantCulture);
        
        // Should either parse successfully or throw - depends on Color.FromArgb implementation
        Assert.NotNull(result);
    }

    [Fact]
    public void Convert_WithMixedValidInvalidColors_WorksForValidOnes()
    {
        // Test mixed valid/invalid color names
        var validResult = _converter.Convert(false, typeof(Color), "InvalidColor,White", CultureInfo.InvariantCulture);
        
        Assert.Equal(Colors.White, validResult);
    }

    [Theory]
    [InlineData(true, "White,Black")]
    [InlineData(false, "White,Black")]
    [InlineData(true, "Transparent,Gray")]
    [InlineData(false, "Transparent,Gray")]
    public void Convert_IsThreadSafe(bool boolValue, string parameter)
    {
        // Test thread safety for concurrent XAML binding
        var tasks = new Task[10];
        var results = new Color[10];
        
        for (int i = 0; i < 10; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                results[index] = (Color)_converter.Convert(boolValue, typeof(Color), parameter, CultureInfo.InvariantCulture)!;
            });
        }
        
        Task.WaitAll(tasks);
        
        // All results should be identical
        var expectedResult = results[0];
        Assert.All(results, result => Assert.Equal(expectedResult, result));
    }

    #endregion
}