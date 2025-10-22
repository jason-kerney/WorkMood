using System;
using Xunit;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests.Models
{
    public class ScheduleOverrideShould
    {
        private readonly DateOnly _testDate = new(2025, 6, 15);
        private readonly TimeSpan _testMorning = new(9, 0, 0);
        private readonly TimeSpan _testEvening = new(18, 0, 0);

        #region Constructor Tests

        [Fact]
        public void DefaultConstructor_ShouldCreateEmptyOverride()
        {
            // Act
            var scheduleOverride = new ScheduleOverride();

            // Assert
            Assert.Equal(default(DateOnly), scheduleOverride.Date);
            Assert.Null(scheduleOverride.MorningTime);
            Assert.Null(scheduleOverride.EveningTime);
            Assert.False(scheduleOverride.HasOverride);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldSetProperties()
        {
            // Act
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Assert
            Assert.Equal(_testDate, scheduleOverride.Date);
            Assert.Equal(_testMorning, scheduleOverride.MorningTime);
            Assert.Equal(_testEvening, scheduleOverride.EveningTime);
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void ParameterizedConstructor_WithPartialTimes_ShouldSetSpecifiedTimes()
        {
            // Arrange & Act
            var morningOnlyOverride = new ScheduleOverride(_testDate, _testMorning);
            var eveningOnlyOverride = new ScheduleOverride(_testDate, eveningTime: _testEvening);

            // Assert
            Assert.Equal(_testDate, morningOnlyOverride.Date);
            Assert.Equal(_testMorning, morningOnlyOverride.MorningTime);
            Assert.Null(morningOnlyOverride.EveningTime);

            Assert.Equal(_testDate, eveningOnlyOverride.Date);
            Assert.Null(eveningOnlyOverride.MorningTime);
            Assert.Equal(_testEvening, eveningOnlyOverride.EveningTime);
        }

        [Fact]
        public void ParameterizedConstructor_WithDateOnly_ShouldSetDateWithNullTimes()
        {
            // Act
            var scheduleOverride = new ScheduleOverride(_testDate);

            // Assert
            Assert.Equal(_testDate, scheduleOverride.Date);
            Assert.Null(scheduleOverride.MorningTime);
            Assert.Null(scheduleOverride.EveningTime);
            Assert.False(scheduleOverride.HasOverride);
        }

        #endregion

        #region Property Assignment Tests

        [Fact]
        public void Properties_ShouldBeSettableAndRetrievable()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride();
            var date = new DateOnly(2025, 3, 20);
            var morning = new TimeSpan(8, 30, 0);
            var evening = new TimeSpan(17, 45, 0);

            // Act
            scheduleOverride.Date = date;
            scheduleOverride.MorningTime = morning;
            scheduleOverride.EveningTime = evening;

            // Assert
            Assert.Equal(date, scheduleOverride.Date);
            Assert.Equal(morning, scheduleOverride.MorningTime);
            Assert.Equal(evening, scheduleOverride.EveningTime);
        }

        [Fact]
        public void Properties_ShouldHandleNullTimeValues()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Act
            scheduleOverride.MorningTime = null;
            scheduleOverride.EveningTime = null;

            // Assert
            Assert.Null(scheduleOverride.MorningTime);
            Assert.Null(scheduleOverride.EveningTime);
            Assert.False(scheduleOverride.HasOverride);
        }

        #endregion

        #region HasOverride Property Tests

        [Fact]
        public void HasOverride_WithBothTimes_ShouldReturnTrue()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Act & Assert
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void HasOverride_WithOnlyMorningTime_ShouldReturnTrue()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning);

            // Act & Assert
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void HasOverride_WithOnlyEveningTime_ShouldReturnTrue()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, eveningTime: _testEvening);

            // Act & Assert
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void HasOverride_WithNoTimes_ShouldReturnFalse()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate);

            // Act & Assert
            Assert.False(scheduleOverride.HasOverride);
        }

        [Fact]
        public void HasOverride_WithDefaultConstruction_ShouldReturnFalse()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride();

            // Act & Assert
            Assert.False(scheduleOverride.HasOverride);
        }

        [Theory]
        [InlineData(true, false)]  // Only morning
        [InlineData(false, true)]  // Only evening
        [InlineData(true, true)]   // Both times
        [InlineData(false, false)] // Neither time
        public void HasOverride_WithVariousTimeCombinations_ShouldReturnCorrectResult(bool hasMorning, bool hasEvening)
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride();
            scheduleOverride.Date = _testDate;
            scheduleOverride.MorningTime = hasMorning ? _testMorning : null;
            scheduleOverride.EveningTime = hasEvening ? _testEvening : null;

            // Act
            var result = scheduleOverride.HasOverride;

            // Assert
            var expected = hasMorning || hasEvening;
            Assert.Equal(expected, result);
        }

        #endregion

        #region AppliesToDate Tests

        [Fact]
        public void AppliesToDate_WithMatchingDate_ShouldReturnTrue()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning);

            // Act
            var result = scheduleOverride.AppliesToDate(_testDate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AppliesToDate_WithDifferentDate_ShouldReturnFalse()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning);
            var differentDate = new DateOnly(2025, 12, 25);

            // Act
            var result = scheduleOverride.AppliesToDate(differentDate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToDate_WithSameDateDifferentTimes_ShouldReturnTrue()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Act
            var result = scheduleOverride.AppliesToDate(_testDate);

            // Assert - Date is only factor, not times
            Assert.True(result);
        }

        [Theory]
        [InlineData(2025, 1, 1)]
        [InlineData(2025, 6, 15)]
        [InlineData(2025, 12, 31)]
        [InlineData(2024, 2, 29)] // Leap year
        [InlineData(2026, 7, 4)]
        public void AppliesToDate_WithVariousDates_ShouldMatchCorrectly(int year, int month, int day)
        {
            // Arrange
            var targetDate = new DateOnly(year, month, day);
            var scheduleOverride = new ScheduleOverride(targetDate, _testMorning);

            // Act
            var matchingResult = scheduleOverride.AppliesToDate(targetDate);
            var nonMatchingResult = scheduleOverride.AppliesToDate(_testDate);

            // Assert
            Assert.True(matchingResult);
            if (targetDate != _testDate)
            {
                Assert.False(nonMatchingResult);
            }
        }

        [Fact]
        public void AppliesToDate_WithDefaultConstructedOverride_ShouldMatchDefaultDate()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride();
            var defaultDate = default(DateOnly);

            // Act
            var result = scheduleOverride.AppliesToDate(defaultDate);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region AppliesToToday Tests

        [Fact]
        public void AppliesToToday_WithTodayDate_ShouldReturnTrue()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var scheduleOverride = new ScheduleOverride(today, _testMorning);

            // Act
            var result = scheduleOverride.AppliesToToday();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AppliesToToday_WithYesterdayDate_ShouldReturnFalse()
        {
            // Arrange
            var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var scheduleOverride = new ScheduleOverride(yesterday, _testMorning);

            // Act
            var result = scheduleOverride.AppliesToToday();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToToday_WithTomorrowDate_ShouldReturnFalse()
        {
            // Arrange
            var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var scheduleOverride = new ScheduleOverride(tomorrow, _testMorning);

            // Act
            var result = scheduleOverride.AppliesToToday();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToToday_WithFutureDateRange_ShouldReturnFalse()
        {
            // Arrange - Test multiple future dates
            var futureDates = new[]
            {
                DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                DateOnly.FromDateTime(DateTime.Today.AddYears(1))
            };

            foreach (var futureDate in futureDates)
            {
                var scheduleOverride = new ScheduleOverride(futureDate, _testMorning);

                // Act
                var result = scheduleOverride.AppliesToToday();

                // Assert
                Assert.False(result);
            }
        }

        [Fact]
        public void AppliesToToday_ConsistentWithAppliesToDate()
        {
            // Arrange
            var today = DateOnly.FromDateTime(DateTime.Today);
            var scheduleOverride = new ScheduleOverride(today, _testMorning);

            // Act
            var appliesToTodayResult = scheduleOverride.AppliesToToday();
            var appliesToDateResult = scheduleOverride.AppliesToDate(today);

            // Assert - Both methods should return the same result for today
            Assert.Equal(appliesToDateResult, appliesToTodayResult);
        }

        #endregion

        #region Business Logic Integration Tests

        [Fact]
        public void ValidOverride_ShouldHaveAtLeastOneTimeAndApplyToDate()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning);

            // Act & Assert
            Assert.True(scheduleOverride.HasOverride);
            Assert.True(scheduleOverride.AppliesToDate(_testDate));
        }

        [Fact]
        public void InvalidOverride_WithNoTimes_ShouldNotHaveOverride()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate);

            // Act & Assert
            Assert.False(scheduleOverride.HasOverride);
            Assert.True(scheduleOverride.AppliesToDate(_testDate)); // Still applies to date, just no time override
        }

        [Fact]
        public void CompleteOverride_WithBothTimes_ShouldBeFullyValid()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Act & Assert
            Assert.True(scheduleOverride.HasOverride);
            Assert.True(scheduleOverride.AppliesToDate(_testDate));
            Assert.Equal(_testDate, scheduleOverride.Date);
            Assert.Equal(_testMorning, scheduleOverride.MorningTime);
            Assert.Equal(_testEvening, scheduleOverride.EveningTime);
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        public void EdgeCase_MinimumDate_ShouldWorkCorrectly()
        {
            // Arrange
            var minDate = DateOnly.MinValue;
            var scheduleOverride = new ScheduleOverride(minDate, _testMorning);

            // Act & Assert
            Assert.Equal(minDate, scheduleOverride.Date);
            Assert.True(scheduleOverride.AppliesToDate(minDate));
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void EdgeCase_MaximumDate_ShouldWorkCorrectly()
        {
            // Arrange
            var maxDate = DateOnly.MaxValue;
            var scheduleOverride = new ScheduleOverride(maxDate, _testEvening);

            // Act & Assert
            Assert.Equal(maxDate, scheduleOverride.Date);
            Assert.True(scheduleOverride.AppliesToDate(maxDate));
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void EdgeCase_ZeroTimeSpan_ShouldBeValidOverride()
        {
            // Arrange
            var zeroTime = TimeSpan.Zero;
            var scheduleOverride = new ScheduleOverride(_testDate, zeroTime);

            // Act & Assert
            Assert.Equal(zeroTime, scheduleOverride.MorningTime);
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void EdgeCase_MaxTimeSpan_ShouldBeValidOverride()
        {
            // Arrange
            var maxTime = new TimeSpan(23, 59, 59);
            var scheduleOverride = new ScheduleOverride(_testDate, eveningTime: maxTime);

            // Act & Assert
            Assert.Equal(maxTime, scheduleOverride.EveningTime);
            Assert.True(scheduleOverride.HasOverride);
        }

        [Fact]
        public void RealWorldScenario_FlexibleWorkDay_ShouldHandleCorrectly()
        {
            // Arrange - Flexible work day with late start
            var flexDate = new DateOnly(2025, 8, 15);
            var lateStart = new TimeSpan(10, 30, 0);  // 10:30 AM start
            var normalEnd = new TimeSpan(18, 30, 0);  // 6:30 PM end
            
            var flexOverride = new ScheduleOverride(flexDate, lateStart, normalEnd);

            // Act & Assert
            Assert.True(flexOverride.HasOverride);
            Assert.True(flexOverride.AppliesToDate(flexDate));
            Assert.Equal(lateStart, flexOverride.MorningTime);
            Assert.Equal(normalEnd, flexOverride.EveningTime);
        }

        [Fact]
        public void RealWorldScenario_HalfDay_ShouldHandleCorrectly()
        {
            // Arrange - Half day with only morning
            var halfDayDate = new DateOnly(2025, 12, 24); // Christmas Eve
            var earlyEnd = new TimeSpan(12, 0, 0);  // Noon end, no morning override
            
            var halfDayOverride = new ScheduleOverride(halfDayDate, eveningTime: earlyEnd);

            // Act & Assert
            Assert.True(halfDayOverride.HasOverride);
            Assert.True(halfDayOverride.AppliesToDate(halfDayDate));
            Assert.Null(halfDayOverride.MorningTime);
            Assert.Equal(earlyEnd, halfDayOverride.EveningTime);
        }

        #endregion

        #region Value Object Behavior Tests

        [Fact]
        public void ValueObjectBehavior_ShouldBeImmutableOnceSet()
        {
            // Arrange
            var originalOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);
            var originalDate = originalOverride.Date;
            var originalMorning = originalOverride.MorningTime;
            var originalEvening = originalOverride.EveningTime;

            // Act - Create new instance with same values
            var equivalentOverride = new ScheduleOverride(_testDate, _testMorning, _testEvening);

            // Assert - Properties should match
            Assert.Equal(originalDate, equivalentOverride.Date);
            Assert.Equal(originalMorning, equivalentOverride.MorningTime);
            Assert.Equal(originalEvening, equivalentOverride.EveningTime);
            Assert.Equal(originalOverride.HasOverride, equivalentOverride.HasOverride);
        }

        [Fact]
        public void PropertyModification_ShouldUpdateComputedProperties()
        {
            // Arrange
            var scheduleOverride = new ScheduleOverride(_testDate);
            Assert.False(scheduleOverride.HasOverride); // Initially no override

            // Act - Add morning time
            scheduleOverride.MorningTime = _testMorning;

            // Assert
            Assert.True(scheduleOverride.HasOverride);

            // Act - Remove morning time, add evening time
            scheduleOverride.MorningTime = null;
            scheduleOverride.EveningTime = _testEvening;

            // Assert
            Assert.True(scheduleOverride.HasOverride);

            // Act - Remove evening time
            scheduleOverride.EveningTime = null;

            // Assert
            Assert.False(scheduleOverride.HasOverride);
        }

        #endregion
    }
}