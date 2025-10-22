using System;
using System.Globalization;
using Xunit;
using WorkMood.MauiApp.Converters;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests.Converters
{
    public class MoodAverageConverterShould
    {
        private readonly MoodAverageConverter _converter;

        public MoodAverageConverterShould()
        {
            _converter = new MoodAverageConverter();
        }

        #region Checkpoint 1: Basic Structure Tests

        [Fact]
        public void BeAssignableFromIValueConverter()
        {
            // Assert
            Assert.IsAssignableFrom<Microsoft.Maui.Controls.IValueConverter>(_converter);
        }

        [Fact]
        public void HaveConvertMethodWithCorrectSignature()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 7 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsType<string>(result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack("7.5", typeof(MoodEntry), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_WithNullValue_ReturnsNotApplicable()
        {
            // Act
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Convert_WithNonMoodEntryType_ReturnsNotApplicable()
        {
            // Act
            var result = _converter.Convert("not a mood entry", typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Convert_WithParameter_IgnoresParameter()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 8 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), "ignored parameter", CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("8.0", result);
        }

        [Fact]
        public void Convert_WithDifferentCulture_ReturnsInvariantFormat()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 7 };
            var germanCulture = new CultureInfo("de-DE"); // Uses comma as decimal separator

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, germanCulture);

            // Assert
            Assert.Equal("7.0", result); // Should use dot regardless of culture
        }

        [Fact]
        public void Convert_WithTargetType_IgnoresTargetType()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 6 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(int), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("6.0", result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Checkpoint 2: Core Logic Tests

        [Fact]
        public void Convert_WithBothMoodValues_ReturnsAverageWithOneDecimal()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 6, EndOfWork = 8 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("7.0", result); // (6 + 8) / 2 = 7.0
        }

        [Fact]
        public void Convert_WithBothMoodValues_HalfDecimal_ReturnsFormattedAverage()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 5, EndOfWork = 8 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("6.5", result); // (5 + 8) / 2 = 6.5
        }

        [Fact]
        public void Convert_WithOnlyEndOfWork_ReturnsEndOfWorkValue()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 9 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("9.0", result);
        }

        [Fact]
        public void Convert_WithOnlyStartOfWork_ReturnsNotApplicable()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 7 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Convert_WithNoMoodValues_ReturnsNotApplicable()
        {
            // Arrange
            var moodEntry = new MoodEntry();

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Convert_WithMinimumMoodValues_ReturnsFormattedAverage()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 1, EndOfWork = 1 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("1.0", result);
        }

        [Fact]
        public void Convert_WithMaximumMoodValues_ReturnsFormattedAverage()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 10, EndOfWork = 10 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("10.0", result);
        }

        [Fact]
        public void Convert_WithOnlyEndOfWork_MinValue_ReturnsFormatted()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 1 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("1.0", result);
        }

        [Fact]
        public void Convert_WithOnlyEndOfWork_MaxValue_ReturnsFormatted()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 10 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("10.0", result);
        }

        [Fact]
        public void Convert_WithOnlyEndOfWork_MidValue_ReturnsFormatted()
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = 5 };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("5.0", result);
        }

        #endregion

        #region Checkpoint 3: String Formatting and Integration Tests

        [Theory]
        [InlineData(1, 2, "1.5")]
        [InlineData(3, 4, "3.5")]
        [InlineData(5, 6, "5.5")]
        [InlineData(7, 8, "7.5")]
        [InlineData(9, 10, "9.5")]
        public void Convert_WithVariousAverageValues_ReturnsCorrectDecimalFormat(int start, int end, string expected)
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = start, EndOfWork = end };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(1, "1.0")]
        [InlineData(2, "2.0")]
        [InlineData(3, "3.0")]
        [InlineData(4, "4.0")]
        [InlineData(5, "5.0")]
        [InlineData(6, "6.0")]
        [InlineData(7, "7.0")]
        [InlineData(8, "8.0")]
        [InlineData(9, "9.0")]
        [InlineData(10, "10.0")]
        public void Convert_WithEndOfWorkOnly_ReturnsOneDecimalFormat(int endOfWork, string expected)
        {
            // Arrange
            var moodEntry = new MoodEntry { EndOfWork = endOfWork };

            // Act
            var result = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Convert_ConsistentBehaviorAcrossMultipleCalls()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 6, EndOfWork = 8 };

            // Act
            var result1 = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
            var result2 = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
            var result3 = _converter.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("7.0", result1);
            Assert.Equal("7.0", result2);
            Assert.Equal("7.0", result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void Convert_WithMoodEntryFromRealWorldScenario_ReturnsProperlySingle()
        {
            // Arrange - Simulating a typical workday mood entry
            var workDayEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartOfWork = 6, // Monday morning feeling
                EndOfWork = 8    // Good productive day
            };

            // Act
            var result = _converter.Convert(workDayEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("7.0", result);
        }

        [Fact]
        public void Convert_WithMoodEntryFromRealWorldScenario_ReturnsProperlyHalf()
        {
            // Arrange - Simulating a mixed workday
            var mixedDayEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartOfWork = 5, // Rough start
                EndOfWork = 9    // Great finish
            };

            // Act
            var result = _converter.Convert(mixedDayEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("7.0", result); // (5 + 9) / 2 = 7.0
        }

        [Fact]
        public void Convert_WithIncompleteWorkDay_HandlesGracefully()
        {
            // Arrange - Work day where user only recorded start
            var incompleteEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartOfWork = 8 // Only recorded start, perhaps forgot to record end
            };

            // Act
            var result = _converter.Convert(incompleteEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("N/A", result);
        }

        [Fact]
        public void Convert_WithEndOfWorkOnlyEntry_HandlesPartialData()
        {
            // Arrange - User recorded end but missed start (common scenario)
            var partialEntry = new MoodEntry
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                EndOfWork = 7 // Only end recorded
            };

            // Act
            var result = _converter.Convert(partialEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("7.0", result);
        }

        [Fact]
        public void Convert_ThreadSafety_MultipleConvertersReturnSameResults()
        {
            // Arrange
            var converter1 = new MoodAverageConverter();
            var converter2 = new MoodAverageConverter();
            var moodEntry = new MoodEntry { StartOfWork = 4, EndOfWork = 6 };

            // Act
            var result1 = converter1.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);
            var result2 = converter2.Convert(moodEntry, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("5.0", result1);
            Assert.Equal("5.0", result2);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Convert_CultureIndependence_AlwaysUsesInvariantFormat()
        {
            // Arrange
            var moodEntry = new MoodEntry { StartOfWork = 3, EndOfWork = 8 };
            var cultures = new[]
            {
                CultureInfo.InvariantCulture,
                new CultureInfo("en-US"),
                new CultureInfo("de-DE"), // Uses comma separator
                new CultureInfo("fr-FR"), // Uses comma separator
                new CultureInfo("ja-JP")  // Different number formatting
            };

            // Act & Assert
            foreach (var culture in cultures)
            {
                var result = _converter.Convert(moodEntry, typeof(string), null, culture);
                Assert.Equal("5.5", result); // Should always be dot-separated regardless of culture
            }
        }

        #endregion
    }
}