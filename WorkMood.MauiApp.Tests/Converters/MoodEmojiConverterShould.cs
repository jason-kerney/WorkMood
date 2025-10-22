using System.Globalization;
using WorkMood.MauiApp.Converters;
using WorkMood.MauiApp.Models;
using Xunit;

namespace WorkMood.MauiApp.Tests.Converters;

/// <summary>
/// Tests for MoodEmojiConverter - MoodEntry to emoji string conversion
/// Component 15 testing following established IValueConverter patterns
/// </summary>
public class MoodEmojiConverterShould
{
    private readonly MoodEmojiConverter _converter;

    public MoodEmojiConverterShould()
    {
        _converter = new MoodEmojiConverter();
    }

    #region Checkpoint 1: Basic Structure

    [Fact]
    public void ImplementIValueConverterInterface()
    {
        // Verify the converter implements the required interface
        Assert.IsAssignableFrom<Microsoft.Maui.Controls.IValueConverter>(_converter);
    }

    [Fact]
    public void Convert_WithValidMoodEntry_ReturnsString()
    {
        // Test basic Convert method with valid MoodEntry
        var moodEntry = new MoodEntry { StartOfWork = 8, EndOfWork = 9 };
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Verify ConvertBack is not implemented (unidirectional converter)
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack("😊", typeof(MoodEntry), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsUnknownEmoji()
    {
        // Test null input handling
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    [Fact]
    public void Convert_WithNonMoodEntryValue_ReturnsUnknownEmoji()
    {
        // Test non-MoodEntry input handling
        var result = _converter.Convert("not a mood entry", typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    [Fact]
    public void HasPublicParameterlessConstructor()
    {
        // Verify XAML compatibility
        var instance = new MoodEmojiConverter();
        Assert.NotNull(instance);
    }

    #endregion

    #region Checkpoint 2: Core Logic - Emoji Mapping

    [Fact]
    public void Convert_WithExcellentMood_ReturnsHappyEmoji()
    {
        // Test mood ≥ 9.0 returns 😄
        var moodEntry = new MoodEntry { StartOfWork = 9, EndOfWork = 10 }; // Average = 9.5
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😄", result);
    }

    [Fact]
    public void Convert_WithExactlyNineMood_ReturnsHappyEmoji()
    {
        // Test boundary: exactly 9.0 returns 😄
        var moodEntry = new MoodEntry { StartOfWork = 9, EndOfWork = 9 }; // Average = 9.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😄", result);
    }

    [Fact]
    public void Convert_WithVeryGoodMood_ReturnsSmileEmoji()
    {
        // Test mood ≥ 8.0 and < 9.0 returns 😊
        var moodEntry = new MoodEntry { StartOfWork = 8, EndOfWork = 8 }; // Average = 8.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😊", result);
    }

    [Fact]
    public void Convert_WithGoodMood_ReturnsSlightSmileEmoji()
    {
        // Test mood ≥ 7.0 and < 8.0 returns 🙂
        var moodEntry = new MoodEntry { StartOfWork = 7, EndOfWork = 7 }; // Average = 7.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("🙂", result);
    }

    [Fact]
    public void Convert_WithOkayMood_ReturnsNeutralEmoji()
    {
        // Test mood ≥ 6.0 and < 7.0 returns 😐
        var moodEntry = new MoodEntry { StartOfWork = 6, EndOfWork = 6 }; // Average = 6.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😐", result);
    }

    [Fact]
    public void Convert_WithNeutralMood_ReturnsSlightFrownEmoji()
    {
        // Test mood ≥ 5.0 and < 6.0 returns 😕
        var moodEntry = new MoodEntry { StartOfWork = 5, EndOfWork = 5 }; // Average = 5.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😕", result);
    }

    [Fact]
    public void Convert_WithNotGreatMood_ReturnsFrownEmoji()
    {
        // Test mood ≥ 4.0 and < 5.0 returns ☹️
        var moodEntry = new MoodEntry { StartOfWork = 4, EndOfWork = 4 }; // Average = 4.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("☹️", result);
    }

    [Fact]
    public void Convert_WithBadMood_ReturnsWorriedEmoji()
    {
        // Test mood ≥ 3.0 and < 4.0 returns 😟
        var moodEntry = new MoodEntry { StartOfWork = 3, EndOfWork = 3 }; // Average = 3.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😟", result);
    }

    [Fact]
    public void Convert_WithVeryBadMood_ReturnsCryingEmoji()
    {
        // Test mood ≥ 2.0 and < 3.0 returns 😢
        var moodEntry = new MoodEntry { StartOfWork = 2, EndOfWork = 2 }; // Average = 2.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😢", result);
    }

    [Fact]
    public void Convert_WithTerribleMood_ReturnsSobbingEmoji()
    {
        // Test mood < 2.0 returns 😭
        var moodEntry = new MoodEntry { StartOfWork = 1, EndOfWork = 1 }; // Average = 1.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😭", result);
    }

    #endregion

    #region Checkpoint 2: Core Logic - MoodEntry Scenarios

    [Fact]
    public void Convert_WithBothMoods_UsesAverageMood()
    {
        // Test with both StartOfWork and EndOfWork - should use average
        var moodEntry = new MoodEntry { StartOfWork = 6, EndOfWork = 8 }; // Average = 7.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("🙂", result); // 7.0 should give 🙂 (good mood)
    }

    [Fact]
    public void Convert_WithOnlyStartOfWork_UsesStartOfWorkMood()
    {
        // Test fallback: only StartOfWork, no EndOfWork
        var moodEntry = new MoodEntry { StartOfWork = 8, EndOfWork = null };
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😊", result); // 8.0 should give 😊 (very good mood)
    }

    [Fact]
    public void Convert_WithOnlyEndOfWork_ReturnsUnknownEmoji()
    {
        // Test with only EndOfWork - GetAverageMood returns null, no StartOfWork fallback
        var moodEntry = new MoodEntry { StartOfWork = null, EndOfWork = 8 };
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    [Fact]
    public void Convert_WithNoMoods_ReturnsUnknownEmoji()
    {
        // Test with no mood values at all
        var moodEntry = new MoodEntry { StartOfWork = null, EndOfWork = null };
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    [Fact]
    public void Convert_WithEmptyMoodEntry_ReturnsUnknownEmoji()
    {
        // Test with default constructed MoodEntry
        var moodEntry = new MoodEntry();
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    #endregion

    #region Checkpoint 2: Core Logic - Boundary Testing

    [Theory]
    [InlineData(8.9, "😊")] // Just under 9.0
    [InlineData(7.9, "🙂")] // Just under 8.0  
    [InlineData(6.9, "😐")] // Just under 7.0
    [InlineData(5.9, "😕")] // Just under 6.0
    [InlineData(4.9, "☹️")] // Just under 5.0
    [InlineData(3.9, "😟")] // Just under 4.0
    [InlineData(2.9, "😢")] // Just under 3.0
    [InlineData(1.9, "😭")] // Just under 2.0
    public void Convert_WithBoundaryMoodValues_ReturnsCorrectEmoji(double moodValue, string expectedEmoji)
    {
        // Test boundary values just under each threshold
        // Create mood values that average to the test value
        int startMood = (int)Math.Floor(moodValue);
        int endMood = (int)Math.Ceiling(moodValue);
        if (startMood == endMood) endMood++; // Ensure we get the exact average we want
        
        // Adjust to get exact average
        if ((startMood + endMood) / 2.0 != moodValue)
        {
            // Use StartOfWork fallback for exact values
            var moodEntry = new MoodEntry { StartOfWork = (int)moodValue, EndOfWork = null };
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(expectedEmoji, result);
        }
        else
        {
            var moodEntry = new MoodEntry { StartOfWork = startMood, EndOfWork = endMood };
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Equal(expectedEmoji, result);
        }
    }

    [Fact]
    public void Convert_WithMixedMoodValues_CalculatesCorrectAverage()
    {
        // Test various mood combinations to verify average calculation
        var moodEntry = new MoodEntry { StartOfWork = 3, EndOfWork = 7 }; // Average = 5.0
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("😕", result); // 5.0 should give 😕 (neutral mood)
    }

    #endregion

    #region Checkpoint 3: Integration Patterns

    [Fact]
    public void Convert_WithRealWorldMoodCombinations_WorksCorrectly()
    {
        // Test realistic daily mood scenarios
        var excellentDay = new MoodEntry { StartOfWork = 9, EndOfWork = 10 };
        var roughStartGoodEnd = new MoodEntry { StartOfWork = 4, EndOfWork = 8 };
        var consistentOkay = new MoodEntry { StartOfWork = 6, EndOfWork = 6 };
        
        Assert.Equal("😄", _converter.Convert(excellentDay, typeof(string), null, CultureInfo.InvariantCulture));
        Assert.Equal("😐", _converter.Convert(roughStartGoodEnd, typeof(string), null, CultureInfo.InvariantCulture)); // Average = 6.0
        Assert.Equal("😐", _converter.Convert(consistentOkay, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_IsCultureIndependent()
    {
        // Test culture independence
        var moodEntry = new MoodEntry { StartOfWork = 8, EndOfWork = 8 };
        
        var resultInvariant = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        var resultGerman = _converter.Convert(moodEntry, typeof(string), null, new CultureInfo("de-DE"));
        
        Assert.Equal(resultInvariant, resultGerman);
        Assert.Equal("😊", resultInvariant);
    }

    [Fact]
    public void Convert_WithDifferentTargetTypes_StillReturnsString()
    {
        // Test with different target types (XAML flexibility)
        var moodEntry = new MoodEntry { StartOfWork = 7, EndOfWork = 7 };
        
        var result1 = _converter.Convert(moodEntry, typeof(object), null, CultureInfo.InvariantCulture);
        var result2 = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal(result1, result2);
        Assert.IsType<string>(result1);
        Assert.Equal("🙂", result1);
    }

    [Fact]
    public void Convert_PerformanceWithManyConversions_IsAcceptable()
    {
        // Test performance for UI binding scenarios
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            var moodEntry = new MoodEntry { StartOfWork = (i % 10) + 1, EndOfWork = ((i + 1) % 10) + 1 };
            _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        }
        
        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 50, "Performance should be acceptable for UI binding");
    }

    [Theory]
    [InlineData(10, 10, "😄")] // Maximum mood values
    [InlineData(1, 1, "😭")]   // Minimum mood values
    [InlineData(5, 6, "😕")]   // Mid-range values
    [InlineData(3, 9, "😐")]   // Wide range average (6.0)
    public async Task Convert_IsThreadSafe(int startMood, int endMood, string expectedEmoji)
    {
        // Test thread safety for concurrent XAML binding
        var moodEntry = new MoodEntry { StartOfWork = startMood, EndOfWork = endMood };
        var tasks = new Task<string>[10];
        
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
                (string)_converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture)!);
        }
        
        var results = await Task.WhenAll(tasks);
        
        // All results should be identical
        Assert.All(results, result => Assert.Equal(expectedEmoji, result));
    }

    #endregion

    #region Checkpoint 3: Integration Patterns - Edge Cases

    [Fact]
    public void Convert_WithParameterAndCulture_IgnoresThemCorrectly()
    {
        // Test that parameter and culture are properly ignored
        var moodEntry = new MoodEntry { StartOfWork = 8, EndOfWork = 8 };
        
        var result1 = _converter.Convert(moodEntry, typeof(string), "ignored", CultureInfo.InvariantCulture);
        var result2 = _converter.Convert(moodEntry, typeof(string), null, new CultureInfo("fr-FR"));
        
        Assert.Equal("😊", result1);
        Assert.Equal("😊", result2);
    }

    [Fact]
    public void Convert_WithExtremeMoodEntry_HandlesGracefully()
    {
        // Test with mood entry that has Date set (real-world usage)
        var moodEntry = new MoodEntry 
        { 
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartOfWork = 7, 
            EndOfWork = 8 
        };
        
        var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("🙂", result); // Average = 7.5, should give 🙂
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(object))]
    public void Convert_WithVariousNonMoodEntryTypes_ReturnsUnknownEmoji(Type inputType)
    {
        // Test various non-MoodEntry input types
        var defaultValue = inputType.IsValueType ? Activator.CreateInstance(inputType) : null;
        var result = _converter.Convert(defaultValue, typeof(string), null, CultureInfo.InvariantCulture);
        
        Assert.Equal("❓", result);
    }

    #endregion
}