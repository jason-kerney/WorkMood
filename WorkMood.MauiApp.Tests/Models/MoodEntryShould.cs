using System;
using Xunit;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests.Models
{
    public class MoodEntryShould
    {
        #region Constructor Tests

        [Fact]
        public void DefaultConstructor_ShouldSetDateToToday()
        {
            // Arrange & Act
            var entry = new MoodEntry();
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            // Assert
            Assert.Equal(today, entry.Date);
        }

        [Fact]
        public void DefaultConstructor_ShouldSetTimestamps()
        {
            // Arrange
            var beforeCreation = DateTime.Now;
            
            // Act
            var entry = new MoodEntry();
            var afterCreation = DateTime.Now;
            
            // Assert
            Assert.True(entry.CreatedAt >= beforeCreation);
            Assert.True(entry.CreatedAt <= afterCreation);
            Assert.True(entry.LastModified >= beforeCreation);
            Assert.True(entry.LastModified <= afterCreation);
        }

        [Fact]
        public void DefaultConstructor_ShouldLeaveStartOfWorkAndEndOfWorkNull()
        {
            // Act
            var entry = new MoodEntry();
            
            // Assert
            Assert.Null(entry.StartOfWork);
            Assert.Null(entry.EndOfWork);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldSetSpecifiedDate()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            
            // Act
            var entry = new MoodEntry(testDate);
            
            // Assert
            Assert.Equal(testDate, entry.Date);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldSetTimestamps()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            var beforeCreation = DateTime.Now;
            
            // Act
            var entry = new MoodEntry(testDate);
            var afterCreation = DateTime.Now;
            
            // Assert
            Assert.True(entry.CreatedAt >= beforeCreation);
            Assert.True(entry.CreatedAt <= afterCreation);
            Assert.True(entry.LastModified >= beforeCreation);
            Assert.True(entry.LastModified <= afterCreation);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldLeaveStartOfWorkAndEndOfWorkNull()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            
            // Act
            var entry = new MoodEntry(testDate);
            
            // Assert
            Assert.Null(entry.StartOfWork);
            Assert.Null(entry.EndOfWork);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Date_ShouldAllowGetAndSet()
        {
            // Arrange
            var entry = new MoodEntry();
            var newDate = new DateOnly(2024, 12, 25);
            
            // Act
            entry.Date = newDate;
            
            // Assert
            Assert.Equal(newDate, entry.Date);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void StartOfWork_ShouldAllowValidValues(int moodValue)
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.StartOfWork = moodValue;
            
            // Assert
            Assert.Equal(moodValue, entry.StartOfWork);
        }

        [Fact]
        public void StartOfWork_ShouldAllowNull()
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.StartOfWork = null;
            
            // Assert
            Assert.Null(entry.StartOfWork);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void EndOfWork_ShouldAllowValidValues(int moodValue)
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.EndOfWork = moodValue;
            
            // Assert
            Assert.Equal(moodValue, entry.EndOfWork);
        }

        [Fact]
        public void EndOfWork_ShouldAllowNull()
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.EndOfWork = null;
            
            // Assert
            Assert.Null(entry.EndOfWork);
        }

        [Fact]
        public void CreatedAt_ShouldAllowGetAndSet()
        {
            // Arrange
            var entry = new MoodEntry();
            var newDateTime = new DateTime(2024, 10, 15, 14, 30, 0);
            
            // Act
            entry.CreatedAt = newDateTime;
            
            // Assert
            Assert.Equal(newDateTime, entry.CreatedAt);
        }

        [Fact]
        public void LastModified_ShouldAllowGetAndSet()
        {
            // Arrange
            var entry = new MoodEntry();
            var newDateTime = new DateTime(2024, 10, 15, 14, 30, 0);
            
            // Act
            entry.LastModified = newDateTime;
            
            // Assert
            Assert.Equal(newDateTime, entry.LastModified);
        }

        #endregion

        #region Value Property (Computed) Tests

        [Fact]
        public void Value_ShouldReturnNull_WhenStartOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = 8
            };
            
            // Act & Assert
            Assert.Null(entry.Value);
        }

        [Fact]
        public void Value_ShouldReturnZero_WhenOnlyStartOfWorkIsSet()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 7,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.Equal(0.0, entry.Value);
        }

        [Theory]
        [InlineData(5, 8, 3.0)]  // Improved mood
        [InlineData(8, 5, -3.0)] // Declined mood
        [InlineData(6, 6, 0.0)]  // No change
        [InlineData(1, 10, 9.0)] // Maximum improvement
        [InlineData(10, 1, -9.0)] // Maximum decline
        public void Value_ShouldCalculateCorrectly_WhenBothMoodsAreSet(int startOfWork, int endOfWork, double expectedValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.Equal(expectedValue, entry.Value);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void IsValid_ShouldReturnTrue_WhenBothMoodsAreNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.True(entry.IsValid());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void IsValid_ShouldReturnTrue_WhenStartOfWorkIsValid(int moodValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = moodValue,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.True(entry.IsValid());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(100)]
        public void IsValid_ShouldReturnFalse_WhenStartOfWorkIsInvalid(int invalidMoodValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = invalidMoodValue,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.False(entry.IsValid());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void IsValid_ShouldReturnTrue_WhenEndOfWorkIsValid(int moodValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 5,
                EndOfWork = moodValue
            };
            
            // Act & Assert
            Assert.True(entry.IsValid());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(100)]
        public void IsValid_ShouldReturnFalse_WhenEndOfWorkIsInvalid(int invalidMoodValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 5,
                EndOfWork = invalidMoodValue
            };
            
            // Act & Assert
            Assert.False(entry.IsValid());
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 8)]
        [InlineData(10, 10)]
        public void IsValid_ShouldReturnTrue_WhenBothMoodsAreValid(int startOfWork, int endOfWork)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.True(entry.IsValid());
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(5, 0)]
        [InlineData(11, 5)]
        [InlineData(5, 11)]
        [InlineData(-1, -1)]
        [InlineData(12, 12)]
        public void IsValid_ShouldReturnFalse_WhenEitherMoodIsInvalid(int startOfWork, int endOfWork)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.False(entry.IsValid());
        }

        #endregion

        #region ShouldSave Tests

        [Fact]
        public void ShouldSave_ShouldReturnFalse_WhenStartOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = 8
            };
            
            // Act & Assert
            Assert.False(entry.ShouldSave());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(100)]
        public void ShouldSave_ShouldReturnFalse_WhenStartOfWorkIsInvalid(int invalidMoodValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = invalidMoodValue,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.False(entry.ShouldSave());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void ShouldSave_ShouldReturnTrue_WhenStartOfWorkIsValidAndEndOfWorkIsNull(int validStartOfWork)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = validStartOfWork,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.True(entry.ShouldSave());
        }

        [Theory]
        [InlineData(5, 1)]
        [InlineData(5, 5)]
        [InlineData(5, 10)]
        public void ShouldSave_ShouldReturnTrue_WhenBothMoodsAreValid(int startOfWork, int endOfWork)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.True(entry.ShouldSave());
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(5, 11)]
        [InlineData(5, -1)]
        public void ShouldSave_ShouldReturnFalse_WhenStartOfWorkIsValidButEndOfWorkIsInvalid(int validStartOfWork, int invalidEndOfWork)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = validStartOfWork,
                EndOfWork = invalidEndOfWork
            };
            
            // Act & Assert
            Assert.False(entry.ShouldSave());
        }

        #endregion

        #region PrepareForSave Tests

        [Fact]
        public void PrepareForSave_ShouldNotModifyEntry_WhenShouldSaveReturnsFalse()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = null
            };
            var originalLastModified = entry.LastModified;
            
            // Act
            entry.PrepareForSave();
            
            // Assert
            Assert.Equal(originalLastModified, entry.LastModified);
            Assert.Null(entry.StartOfWork);
            Assert.Null(entry.EndOfWork);
        }

        [Fact]
        public void PrepareForSave_ShouldUpdateLastModified_WhenShouldSaveReturnsTrue()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 5,
                EndOfWork = null
            };
            var originalLastModified = entry.LastModified;
            
            // Add slight delay to ensure time difference
            System.Threading.Thread.Sleep(1);
            
            // Act
            entry.PrepareForSave();
            
            // Assert
            Assert.True(entry.LastModified > originalLastModified);
        }

        [Fact]
        public void PrepareForSave_ShouldNotSetEndOfWork_WhenUseAutoSaveDefaultsIsFalse()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 7,
                EndOfWork = null
            };
            
            // Act
            entry.PrepareForSave(useAutoSaveDefaults: false);
            
            // Assert
            Assert.Null(entry.EndOfWork);
        }

        [Fact]
        public void PrepareForSave_ShouldSetEndOfWorkToStartOfWork_WhenUseAutoSaveDefaultsIsTrue()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 7,
                EndOfWork = null
            };
            
            // Act
            entry.PrepareForSave(useAutoSaveDefaults: true);
            
            // Assert
            Assert.Equal(7, entry.EndOfWork);
        }

        [Fact]
        public void PrepareForSave_ShouldNotOverrideEndOfWork_WhenAlreadySet()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 7,
                EndOfWork = 9
            };
            
            // Act
            entry.PrepareForSave(useAutoSaveDefaults: true);
            
            // Assert
            Assert.Equal(9, entry.EndOfWork);
        }

        [Fact]
        public void PrepareForSave_ShouldNotSetEndOfWork_WhenStartOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = null
            };
            
            // Act
            entry.PrepareForSave(useAutoSaveDefaults: true);
            
            // Assert
            Assert.Null(entry.EndOfWork);
        }

        #endregion

        #region GetAverageMood Tests

        [Fact]
        public void GetAverageMood_ShouldReturnNull_WhenStartOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = 8
            };
            
            // Act & Assert
            Assert.Null(entry.GetAverageMood());
        }

        [Fact]
        public void GetAverageMood_ShouldReturnNull_WhenEndOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = 6,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.Null(entry.GetAverageMood());
        }

        [Fact]
        public void GetAverageMood_ShouldReturnNull_WhenBothMoodsAreNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.Null(entry.GetAverageMood());
        }

        [Theory]
        [InlineData(1, 1, 1.0)]
        [InlineData(1, 9, 5.0)]
        [InlineData(5, 7, 6.0)]
        [InlineData(10, 10, 10.0)]
        [InlineData(3, 8, 5.5)]
        public void GetAverageMood_ShouldCalculateCorrectly_WhenBothMoodsAreSet(int startOfWork, int endOfWork, double expectedAverage)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.Equal(expectedAverage, entry.GetAverageMood());
        }

        #endregion

        #region GetAdjustedAverageMood Tests

        [Fact]
        public void GetAdjustedAverageMood_ShouldReturnNull_WhenStartOfWorkIsNull()
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = null,
                EndOfWork = 8
            };
            
            // Act & Assert
            Assert.Null(entry.GetAdjustedAverageMood());
        }

        [Theory]
        [InlineData(1, -4.0)]   // (1 + 1) / 2 - 5 = -4
        [InlineData(5, 0.0)]    // (5 + 5) / 2 - 5 = 0  
        [InlineData(10, 5.0)]   // (10 + 10) / 2 - 5 = 5
        public void GetAdjustedAverageMood_ShouldUseStartOfWorkForBoth_WhenEndOfWorkIsNull(int startOfWork, double expectedAdjusted)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = null
            };
            
            // Act & Assert
            Assert.Equal(expectedAdjusted, entry.GetAdjustedAverageMood());
        }

        [Theory]
        [InlineData(1, 1, -4.0)]    // (1 + 1) / 2 - 5 = -4
        [InlineData(1, 9, 0.0)]     // (1 + 9) / 2 - 5 = 0
        [InlineData(5, 7, 1.0)]     // (5 + 7) / 2 - 5 = 1
        [InlineData(10, 10, 5.0)]   // (10 + 10) / 2 - 5 = 5
        [InlineData(3, 8, 0.5)]     // (3 + 8) / 2 - 5 = 0.5
        public void GetAdjustedAverageMood_ShouldCalculateCorrectly_WhenBothMoodsAreSet(int startOfWork, int endOfWork, double expectedAdjusted)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = startOfWork,
                EndOfWork = endOfWork
            };
            
            // Act & Assert
            Assert.Equal(expectedAdjusted, entry.GetAdjustedAverageMood());
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_ShouldShowNotRecorded_WhenBothMoodsAreNull()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            var entry = new MoodEntry(testDate)
            {
                StartOfWork = null,
                EndOfWork = null
            };
            
            // Act
            var result = entry.ToString();
            
            // Assert
            Assert.Equal("2024-10-15: Start of Work Not recorded, End of Work Not recorded", result);
        }

        [Fact]
        public void ToString_ShouldShowStartOfWorkOnly_WhenEndOfWorkIsNull()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            var entry = new MoodEntry(testDate)
            {
                StartOfWork = 7,
                EndOfWork = null
            };
            
            // Act
            var result = entry.ToString();
            
            // Assert
            Assert.Equal("2024-10-15: Start of Work 7, End of Work Not recorded", result);
        }

        [Fact]
        public void ToString_ShouldShowEndOfWorkOnly_WhenStartOfWorkIsNull()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            var entry = new MoodEntry(testDate)
            {
                StartOfWork = null,
                EndOfWork = 9
            };
            
            // Act
            var result = entry.ToString();
            
            // Assert
            Assert.Equal("2024-10-15: Start of Work Not recorded, End of Work 9", result);
        }

        [Fact]
        public void ToString_ShouldShowBothMoods_WhenBothAreSet()
        {
            // Arrange
            var testDate = new DateOnly(2024, 10, 15);
            var entry = new MoodEntry(testDate)
            {
                StartOfWork = 6,
                EndOfWork = 8
            };
            
            // Act
            var result = entry.ToString();
            
            // Assert
            Assert.Equal("2024-10-15: Start of Work 6, End of Work 8", result);
        }

        [Fact]
        public void ToString_ShouldFormatDateCorrectly_ForDifferentDates()
        {
            // Arrange
            var testDate = new DateOnly(2025, 1, 1);
            var entry = new MoodEntry(testDate)
            {
                StartOfWork = 5,
                EndOfWork = 5
            };
            
            // Act
            var result = entry.ToString();
            
            // Assert
            Assert.Equal("2025-01-01: Start of Work 5, End of Work 5", result);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void MoodEntry_ShouldHandleMinimumDate()
        {
            // Arrange & Act
            var entry = new MoodEntry(DateOnly.MinValue);
            
            // Assert
            Assert.Equal(DateOnly.MinValue, entry.Date);
        }

        [Fact]
        public void MoodEntry_ShouldHandleMaximumDate()
        {
            // Arrange & Act
            var entry = new MoodEntry(DateOnly.MaxValue);
            
            // Assert
            Assert.Equal(DateOnly.MaxValue, entry.Date);
        }

        [Fact]
        public void MoodEntry_ShouldHandleMinimumDateTime()
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.CreatedAt = DateTime.MinValue;
            entry.LastModified = DateTime.MinValue;
            
            // Assert
            Assert.Equal(DateTime.MinValue, entry.CreatedAt);
            Assert.Equal(DateTime.MinValue, entry.LastModified);
        }

        [Fact]
        public void MoodEntry_ShouldHandleMaximumDateTime()
        {
            // Arrange
            var entry = new MoodEntry();
            
            // Act
            entry.CreatedAt = DateTime.MaxValue;
            entry.LastModified = DateTime.MaxValue;
            
            // Assert
            Assert.Equal(DateTime.MaxValue, entry.CreatedAt);
            Assert.Equal(DateTime.MaxValue, entry.LastModified);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void MoodEntry_ShouldHandleExtremeMoodValues_InValidation(int extremeValue)
        {
            // Arrange
            var entry = new MoodEntry
            {
                StartOfWork = extremeValue,
                EndOfWork = extremeValue
            };
            
            // Act & Assert
            Assert.False(entry.IsValid());
            Assert.False(entry.ShouldSave());
        }

        #endregion

        #region Business Logic Integration Tests

        [Fact]
        public void BusinessFlow_ShouldWorkCorrectly_ForCompleteEntry()
        {
            // Arrange - Simulate a complete daily entry
            var entry = new MoodEntry();
            
            // Act - Set morning mood
            entry.StartOfWork = 6;
            
            // Assert - Should be saveable with morning mood only
            Assert.True(entry.ShouldSave());
            Assert.True(entry.IsValid());
            Assert.Equal(0.0, entry.Value); // No change yet
            Assert.Null(entry.GetAverageMood()); // Need both for average
            Assert.Equal(1.0, entry.GetAdjustedAverageMood()); // (6+6)/2 - 5 = 1
            
            // Act - Set evening mood
            entry.EndOfWork = 8;
            
            // Assert - Should show improvement
            Assert.True(entry.ShouldSave());
            Assert.True(entry.IsValid());
            Assert.Equal(2.0, entry.Value); // Improved by 2
            Assert.Equal(7.0, entry.GetAverageMood()); // (6+8)/2 = 7
            Assert.Equal(2.0, entry.GetAdjustedAverageMood()); // 7 - 5 = 2
        }

        [Fact]
        public void BusinessFlow_ShouldWorkCorrectly_ForAutoSaveScenario()
        {
            // Arrange - End of day with only morning mood
            var entry = new MoodEntry
            {
                StartOfWork = 7,
                EndOfWork = null
            };
            
            // Act - Prepare for auto-save
            entry.PrepareForSave(useAutoSaveDefaults: true);
            
            // Assert - Should have auto-filled evening mood
            Assert.Equal(7, entry.EndOfWork);
            Assert.Equal(0.0, entry.Value); // No change
            Assert.Equal(7.0, entry.GetAverageMood()); // Same mood all day
            Assert.Equal(2.0, entry.GetAdjustedAverageMood()); // 7 - 5 = 2
        }

        [Fact]
        public void BusinessFlow_ShouldPreventSaving_ForInvalidEntry()
        {
            // Arrange - Invalid mood values
            var entry = new MoodEntry
            {
                StartOfWork = 15, // Invalid - out of range
                EndOfWork = 8
            };
            
            // Act & Assert - Should not be saveable
            Assert.False(entry.IsValid());
            Assert.False(entry.ShouldSave());
            
            // PrepareForSave should do nothing
            var originalLastModified = entry.LastModified;
            entry.PrepareForSave(useAutoSaveDefaults: true);
            Assert.Equal(originalLastModified, entry.LastModified);
        }

        #endregion
    }
}