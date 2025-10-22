using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.Tests.Models
{
    public class ScheduleConfigShould
    {
        private readonly TimeSpan _defaultMorning = new(8, 20, 0);
        private readonly TimeSpan _defaultEvening = new(17, 20, 0);
        private readonly DateOnly _testDate = new(2025, 6, 15);
        private readonly TimeSpan _customMorning = new(9, 0, 0);
        private readonly TimeSpan _customEvening = new(18, 0, 0);

        #region Constructor Tests

        [Fact]
        public void DefaultConstructor_ShouldSetDefaultTimes()
        {
            // Act
            var config = new ScheduleConfig();

            // Assert
            Assert.Equal(_defaultMorning, config.MorningTime);
            Assert.Equal(_defaultEvening, config.EveningTime);
            Assert.NotNull(config.Overrides);
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldSetCustomTimes()
        {
            // Act
            var config = new ScheduleConfig(_customMorning, _customEvening);

            // Assert
            Assert.Equal(_customMorning, config.MorningTime);
            Assert.Equal(_customEvening, config.EveningTime);
            Assert.NotNull(config.Overrides);
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void ParameterizedConstructor_WithZeroTimes_ShouldSetZeroTimes()
        {
            // Arrange
            var zeroTime = TimeSpan.Zero;

            // Act
            var config = new ScheduleConfig(zeroTime, zeroTime);

            // Assert
            Assert.Equal(zeroTime, config.MorningTime);
            Assert.Equal(zeroTime, config.EveningTime);
        }

        [Fact]
        public void ParameterizedConstructor_WithMaxTimes_ShouldSetMaxTimes()
        {
            // Arrange
            var maxMorning = new TimeSpan(11, 59, 59);
            var maxEvening = new TimeSpan(23, 59, 59);

            // Act
            var config = new ScheduleConfig(maxMorning, maxEvening);

            // Assert
            Assert.Equal(maxMorning, config.MorningTime);
            Assert.Equal(maxEvening, config.EveningTime);
        }

        #endregion

        #region Property Assignment Tests

        [Fact]
        public void Properties_ShouldBeSettableAndRetrievable()
        {
            // Arrange
            var config = new ScheduleConfig();
            var newMorning = new TimeSpan(7, 30, 0);
            var newEvening = new TimeSpan(19, 30, 0);

            // Act
            config.MorningTime = newMorning;
            config.EveningTime = newEvening;

            // Assert
            Assert.Equal(newMorning, config.MorningTime);
            Assert.Equal(newEvening, config.EveningTime);
        }

        [Fact]
        public void OverridesProperty_ShouldBeSettableAndRetrievable()
        {
            // Arrange
            var config = new ScheduleConfig();
            var overrides = new List<ScheduleOverride>
            {
                new(_testDate, _customMorning, _customEvening)
            };

            // Act
            config.Overrides = overrides;

            // Assert
            Assert.Equal(overrides, config.Overrides);
            Assert.Single(config.Overrides);
        }

        #endregion

        #region GetEffectiveMorningTime Tests

        [Fact]
        public void GetEffectiveMorningTime_WithoutOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            var result = config.GetEffectiveMorningTime(_testDate);

            // Assert
            Assert.Equal(_defaultMorning, result);
        }

        [Fact]
        public void GetEffectiveMorningTime_WithOverride_ShouldReturnOverrideTime()
        {
            // Arrange
            var config = new ScheduleConfig();
            config.Overrides.Add(new ScheduleOverride(_testDate, _customMorning, _customEvening));

            // Act
            var result = config.GetEffectiveMorningTime(_testDate);

            // Assert
            Assert.Equal(_customMorning, result);
        }

        [Fact]
        public void GetEffectiveMorningTime_WithPartialOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();
            // Override with only evening time, no morning time
            config.Overrides.Add(new ScheduleOverride(_testDate, null, _customEvening));

            // Act
            var result = config.GetEffectiveMorningTime(_testDate);

            // Assert
            Assert.Equal(_defaultMorning, result);
        }

        [Fact]
        public void GetEffectiveMorningTime_WithMultipleOverrides_ShouldReturnFirstMatch()
        {
            // Arrange
            var config = new ScheduleConfig();
            var firstOverrideTime = new TimeSpan(6, 0, 0);
            var secondOverrideTime = new TimeSpan(10, 0, 0);
            config.Overrides.Add(new ScheduleOverride(_testDate, firstOverrideTime));
            config.Overrides.Add(new ScheduleOverride(_testDate, secondOverrideTime));

            // Act
            var result = config.GetEffectiveMorningTime(_testDate);

            // Assert
            Assert.Equal(firstOverrideTime, result);
        }

        [Fact]
        public void GetEffectiveMorningTime_WithDifferentDate_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();
            var differentDate = new DateOnly(2025, 12, 25);
            config.Overrides.Add(new ScheduleOverride(_testDate, _customMorning));

            // Act
            var result = config.GetEffectiveMorningTime(differentDate);

            // Assert
            Assert.Equal(_defaultMorning, result);
        }

        #endregion

        #region GetEffectiveEveningTime Tests

        [Fact]
        public void GetEffectiveEveningTime_WithoutOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            var result = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(_defaultEvening, result);
        }

        [Fact]
        public void GetEffectiveEveningTime_WithOverride_ShouldReturnOverrideTime()
        {
            // Arrange
            var config = new ScheduleConfig();
            config.Overrides.Add(new ScheduleOverride(_testDate, _customMorning, _customEvening));

            // Act
            var result = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(_customEvening, result);
        }

        [Fact]
        public void GetEffectiveEveningTime_WithPartialOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();
            // Override with only morning time, no evening time
            config.Overrides.Add(new ScheduleOverride(_testDate, _customMorning, null));

            // Act
            var result = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(_defaultEvening, result);
        }

        [Fact]
        public void GetEffectiveEveningTime_WithCustomDefaultTimes_ShouldReturnCustomDefault()
        {
            // Arrange
            var customDefaultEvening = new TimeSpan(16, 0, 0);
            var config = new ScheduleConfig(_customMorning, customDefaultEvening);

            // Act
            var result = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(customDefaultEvening, result);
        }

        #endregion

        #region Today Methods Tests

        [Fact]
        public void GetEffectiveMorningTimeToday_ShouldUseCurrentDate()
        {
            // Arrange
            var config = new ScheduleConfig();
            var today = DateOnly.FromDateTime(DateTime.Today);
            config.Overrides.Add(new ScheduleOverride(today, _customMorning));

            // Act
            var result = config.GetEffectiveMorningTimeToday();

            // Assert
            Assert.Equal(_customMorning, result);
        }

        [Fact]
        public void GetEffectiveMorningTimeToday_WithoutTodayOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();
            // Add override for different date, not today
            config.Overrides.Add(new ScheduleOverride(_testDate, _customMorning));

            // Act
            var result = config.GetEffectiveMorningTimeToday();

            // Assert
            Assert.Equal(_defaultMorning, result);
        }

        [Fact]
        public void GetEffectiveEveningTimeToday_ShouldUseCurrentDate()
        {
            // Arrange
            var config = new ScheduleConfig();
            var today = DateOnly.FromDateTime(DateTime.Today);
            config.Overrides.Add(new ScheduleOverride(today, null, _customEvening));

            // Act
            var result = config.GetEffectiveEveningTimeToday();

            // Assert
            Assert.Equal(_customEvening, result);
        }

        [Fact]
        public void GetEffectiveEveningTimeToday_WithoutTodayOverride_ShouldReturnDefault()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            var result = config.GetEffectiveEveningTimeToday();

            // Assert
            Assert.Equal(_defaultEvening, result);
        }

        [Fact]
        public void TodayMethods_ShouldBeConsistentWithDateMethods()
        {
            // Arrange
            var config = new ScheduleConfig();
            var today = DateOnly.FromDateTime(DateTime.Today);
            config.Overrides.Add(new ScheduleOverride(today, _customMorning, _customEvening));

            // Act
            var todayMorning = config.GetEffectiveMorningTimeToday();
            var dateMorning = config.GetEffectiveMorningTime(today);
            var todayEvening = config.GetEffectiveEveningTimeToday();
            var dateEvening = config.GetEffectiveEveningTime(today);

            // Assert
            Assert.Equal(dateMorning, todayMorning);
            Assert.Equal(dateEvening, todayEvening);
        }

        #endregion

        #region SetOverride Tests

        [Fact]
        public void SetOverride_WithBothTimes_ShouldAddOverride()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            config.SetOverride(_testDate, _customMorning, _customEvening);

            // Assert
            Assert.Single(config.Overrides);
            var addedOverride = config.Overrides.First();
            Assert.Equal(_testDate, addedOverride.Date);
            Assert.Equal(_customMorning, addedOverride.MorningTime);
            Assert.Equal(_customEvening, addedOverride.EveningTime);
        }

        [Fact]
        public void SetOverride_WithOnlyMorningTime_ShouldAddOverride()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            config.SetOverride(_testDate, _customMorning, null);

            // Assert
            Assert.Single(config.Overrides);
            var addedOverride = config.Overrides.First();
            Assert.Equal(_testDate, addedOverride.Date);
            Assert.Equal(_customMorning, addedOverride.MorningTime);
            Assert.Null(addedOverride.EveningTime);
        }

        [Fact]
        public void SetOverride_WithOnlyEveningTime_ShouldAddOverride()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            config.SetOverride(_testDate, null, _customEvening);

            // Assert
            Assert.Single(config.Overrides);
            var addedOverride = config.Overrides.First();
            Assert.Equal(_testDate, addedOverride.Date);
            Assert.Null(addedOverride.MorningTime);
            Assert.Equal(_customEvening, addedOverride.EveningTime);
        }

        [Fact]
        public void SetOverride_WithNoTimes_ShouldNotAddOverride()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            config.SetOverride(_testDate, null, null);

            // Assert
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void SetOverride_WithExistingDate_ShouldReplaceOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            var originalMorning = new TimeSpan(6, 0, 0);
            var newMorning = new TimeSpan(10, 0, 0);
            
            config.SetOverride(_testDate, originalMorning, _customEvening);

            // Act
            config.SetOverride(_testDate, newMorning, _customEvening);

            // Assert
            Assert.Single(config.Overrides);
            var updatedOverride = config.Overrides.First();
            Assert.Equal(_testDate, updatedOverride.Date);
            Assert.Equal(newMorning, updatedOverride.MorningTime);
            Assert.Equal(_customEvening, updatedOverride.EveningTime);
        }

        [Fact]
        public void SetOverride_WithExistingDateToNoTimes_ShouldRemoveOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            config.SetOverride(_testDate, _customMorning, _customEvening);
            Assert.Single(config.Overrides);

            // Act
            config.SetOverride(_testDate, null, null);

            // Assert
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void SetOverride_WithMultipleDates_ShouldMaintainSeparateOverrides()
        {
            // Arrange
            var config = new ScheduleConfig();
            var date1 = new DateOnly(2025, 1, 1);
            var date2 = new DateOnly(2025, 12, 31);
            var time1 = new TimeSpan(6, 0, 0);
            var time2 = new TimeSpan(22, 0, 0);

            // Act
            config.SetOverride(date1, time1);
            config.SetOverride(date2, null, time2);

            // Assert
            Assert.Equal(2, config.Overrides.Count);
            Assert.Contains(config.Overrides, o => o.Date == date1 && o.MorningTime == time1);
            Assert.Contains(config.Overrides, o => o.Date == date2 && o.EveningTime == time2);
        }

        #endregion

        #region RemoveOverride Tests

        [Fact]
        public void RemoveOverride_WithExistingDate_ShouldRemoveOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            config.SetOverride(_testDate, _customMorning, _customEvening);
            Assert.Single(config.Overrides);

            // Act
            config.RemoveOverride(_testDate);

            // Assert
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void RemoveOverride_WithNonExistentDate_ShouldNotAffectOtherOverrides()
        {
            // Arrange
            var config = new ScheduleConfig();
            var existingDate = new DateOnly(2025, 1, 1);
            var nonExistentDate = new DateOnly(2025, 12, 31);
            config.SetOverride(existingDate, _customMorning);

            // Act
            config.RemoveOverride(nonExistentDate);

            // Assert
            Assert.Single(config.Overrides);
            Assert.Equal(existingDate, config.Overrides.First().Date);
        }

        [Fact]
        public void RemoveOverride_WithMultipleOverrides_ShouldOnlyRemoveSpecifiedDate()
        {
            // Arrange
            var config = new ScheduleConfig();
            var date1 = new DateOnly(2025, 1, 1);
            var date2 = new DateOnly(2025, 12, 31);
            config.SetOverride(date1, _customMorning);
            config.SetOverride(date2, _customEvening);

            // Act
            config.RemoveOverride(date1);

            // Assert
            Assert.Single(config.Overrides);
            Assert.Equal(date2, config.Overrides.First().Date);
        }

        [Fact]
        public void RemoveOverride_WithEmptyOverrides_ShouldNotThrow()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act & Assert - Should not throw
            config.RemoveOverride(_testDate);
            Assert.Empty(config.Overrides);
        }

        #endregion

        #region GetOverride Tests

        [Fact]
        public void GetOverride_WithExistingDate_ShouldReturnOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            config.SetOverride(_testDate, _customMorning, _customEvening);

            // Act
            var result = config.GetOverride(_testDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testDate, result.Date);
            Assert.Equal(_customMorning, result.MorningTime);
            Assert.Equal(_customEvening, result.EveningTime);
        }

        [Fact]
        public void GetOverride_WithNonExistentDate_ShouldReturnNull()
        {
            // Arrange
            var config = new ScheduleConfig();
            var nonExistentDate = new DateOnly(2025, 12, 31);

            // Act
            var result = config.GetOverride(nonExistentDate);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetOverride_WithEmptyOverrides_ShouldReturnNull()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act
            var result = config.GetOverride(_testDate);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetOverride_WithMultipleOverrides_ShouldReturnCorrectOne()
        {
            // Arrange
            var config = new ScheduleConfig();
            var date1 = new DateOnly(2025, 1, 1);
            var date2 = new DateOnly(2025, 12, 31);
            var time1 = new TimeSpan(6, 0, 0);
            var time2 = new TimeSpan(22, 0, 0);
            
            config.SetOverride(date1, time1);
            config.SetOverride(date2, null, time2);

            // Act
            var result1 = config.GetOverride(date1);
            var result2 = config.GetOverride(date2);

            // Assert
            Assert.NotNull(result1);
            Assert.Equal(date1, result1.Date);
            Assert.Equal(time1, result1.MorningTime);

            Assert.NotNull(result2);
            Assert.Equal(date2, result2.Date);
            Assert.Equal(time2, result2.EveningTime);
        }

        #endregion

        #region CleanupOldOverrides Tests

        [Fact]
        public void CleanupOldOverrides_ShouldRemoveOldOverrides()
        {
            // Arrange
            var config = new ScheduleConfig();
            var oldDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-40));
            var recentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-20));
            
            config.SetOverride(oldDate, _customMorning);
            config.SetOverride(recentDate, _customEvening);

            // Act
            config.CleanupOldOverrides();

            // Assert
            Assert.Single(config.Overrides);
            Assert.Equal(recentDate, config.Overrides.First().Date);
        }

        [Fact]
        public void CleanupOldOverrides_WithRecentOverrides_ShouldKeepAll()
        {
            // Arrange
            var config = new ScheduleConfig();
            var recentDate1 = DateOnly.FromDateTime(DateTime.Today.AddDays(-10));
            var recentDate2 = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
            
            config.SetOverride(recentDate1, _customMorning);
            config.SetOverride(recentDate2, _customEvening);

            // Act
            config.CleanupOldOverrides();

            // Assert
            Assert.Equal(2, config.Overrides.Count);
        }

        [Fact]
        public void CleanupOldOverrides_WithEmptyOverrides_ShouldNotThrow()
        {
            // Arrange
            var config = new ScheduleConfig();

            // Act & Assert - Should not throw
            config.CleanupOldOverrides();
            Assert.Empty(config.Overrides);
        }

        [Fact]
        public void CleanupOldOverrides_WithExactlyCutoffDate_ShouldKeep()
        {
            // Arrange
            var config = new ScheduleConfig();
            var cutoffDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            
            config.SetOverride(cutoffDate, _customMorning);

            // Act
            config.CleanupOldOverrides();

            // Assert - Exactly 30 days ago should NOT be removed (< condition, not <=)
            Assert.Single(config.Overrides);
        }

        [Fact]
        public void CleanupOldOverrides_WithBoundaryDates_ShouldHandleCorrectly()
        {
            // Arrange
            var config = new ScheduleConfig();
            var justOld = DateOnly.FromDateTime(DateTime.Today.AddDays(-31));
            var justRecent = DateOnly.FromDateTime(DateTime.Today.AddDays(-29));
            
            config.SetOverride(justOld, _customMorning);
            config.SetOverride(justRecent, _customEvening);

            // Act
            config.CleanupOldOverrides();

            // Assert
            Assert.Single(config.Overrides);
            Assert.Equal(justRecent, config.Overrides.First().Date);
        }

        #endregion

        #region Business Logic Integration Tests

        [Fact]
        public void CompleteWorkflow_ShouldHandleAllOperations()
        {
            // Arrange
            var config = new ScheduleConfig(_customMorning, _customEvening);
            var today = DateOnly.FromDateTime(DateTime.Today);
            var date1 = today.AddDays(-10); // Recent date within cleanup window
            var date2 = today.AddDays(-5);  // More recent date within cleanup window
            var overrideMorning = new TimeSpan(7, 0, 0);
            var overrideEvening = new TimeSpan(19, 0, 0);

            // Act & Assert - Set overrides
            config.SetOverride(date1, overrideMorning);
            config.SetOverride(date2, null, overrideEvening);
            Assert.Equal(2, config.Overrides.Count);

            // Act & Assert - Get effective times
            Assert.Equal(overrideMorning, config.GetEffectiveMorningTime(date1));
            Assert.Equal(_customEvening, config.GetEffectiveEveningTime(date1)); // No evening override
            Assert.Equal(_customMorning, config.GetEffectiveMorningTime(date2)); // No morning override
            Assert.Equal(overrideEvening, config.GetEffectiveEveningTime(date2));

            // Act & Assert - Remove override
            config.RemoveOverride(date1);
            Assert.Single(config.Overrides);
            Assert.Equal(_customMorning, config.GetEffectiveMorningTime(date1)); // Back to default

            // Act & Assert - Cleanup (recent override should remain)
            config.CleanupOldOverrides();
            Assert.Single(config.Overrides);
        }

        [Fact]
        public void OverridePrecedence_ShouldWorkCorrectly()
        {
            // Arrange
            var config = new ScheduleConfig();
            var overrideTime = new TimeSpan(10, 0, 0);
            
            // Act & Assert - No override
            Assert.Equal(_defaultMorning, config.GetEffectiveMorningTime(_testDate));
            
            // Act & Assert - With override
            config.SetOverride(_testDate, overrideTime);
            Assert.Equal(overrideTime, config.GetEffectiveMorningTime(_testDate));
            
            // Act & Assert - Remove override
            config.RemoveOverride(_testDate);
            Assert.Equal(_defaultMorning, config.GetEffectiveMorningTime(_testDate));
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        public void EdgeCase_MinimumDate_ShouldWorkCorrectly()
        {
            // Arrange
            var config = new ScheduleConfig();
            var minDate = DateOnly.MinValue;

            // Act
            config.SetOverride(minDate, _customMorning);
            var result = config.GetEffectiveMorningTime(minDate);

            // Assert
            Assert.Equal(_customMorning, result);
        }

        [Fact]
        public void EdgeCase_MaximumDate_ShouldWorkCorrectly()
        {
            // Arrange
            var config = new ScheduleConfig();
            var maxDate = DateOnly.MaxValue;

            // Act
            config.SetOverride(maxDate, null, _customEvening);
            var result = config.GetEffectiveEveningTime(maxDate);

            // Assert
            Assert.Equal(_customEvening, result);
        }

        [Fact]
        public void EdgeCase_ZeroTimeSpan_ShouldBeValidOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            var zeroTime = TimeSpan.Zero;

            // Act
            config.SetOverride(_testDate, zeroTime, zeroTime);
            var morningResult = config.GetEffectiveMorningTime(_testDate);
            var eveningResult = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(zeroTime, morningResult);
            Assert.Equal(zeroTime, eveningResult);
        }

        [Fact]
        public void EdgeCase_MaxTimeSpan_ShouldBeValidOverride()
        {
            // Arrange
            var config = new ScheduleConfig();
            var maxTime = new TimeSpan(23, 59, 59);

            // Act
            config.SetOverride(_testDate, maxTime, maxTime);
            var morningResult = config.GetEffectiveMorningTime(_testDate);
            var eveningResult = config.GetEffectiveEveningTime(_testDate);

            // Assert
            Assert.Equal(maxTime, morningResult);
            Assert.Equal(maxTime, eveningResult);
        }

        [Fact]
        public void LargeOverrideCollection_ShouldPerformCorrectly()
        {
            // Arrange
            var config = new ScheduleConfig();
            var targetDate = new DateOnly(2025, 6, 15);
            var targetTime = new TimeSpan(12, 0, 0);

            // Add many overrides
            for (int i = 1; i <= 100; i++)
            {
                var date = new DateOnly(2025, 1, 1).AddDays(i);
                var time = new TimeSpan(8 + (i % 12), 0, 0);
                config.SetOverride(date, time);
            }

            // Add specific override we're testing
            config.SetOverride(targetDate, targetTime);

            // Act
            var result = config.GetEffectiveMorningTime(targetDate);

            // Assert
            Assert.Equal(targetTime, result);
            Assert.True(config.Overrides.Count > 100);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void RealWorldScenario_FlexibleSchedule_ShouldHandleCorrectly()
        {
            // Arrange - Flexible work schedule
            var config = new ScheduleConfig(new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));
            var earlyMeetingDay = new DateOnly(2025, 6, 15);
            var lateWorkDay = new DateOnly(2025, 6, 16);
            var halfDay = new DateOnly(2025, 6, 17);

            // Act - Set various overrides
            config.SetOverride(earlyMeetingDay, new TimeSpan(7, 30, 0)); // Early start
            config.SetOverride(lateWorkDay, null, new TimeSpan(20, 0, 0)); // Late end
            config.SetOverride(halfDay, null, new TimeSpan(12, 0, 0)); // Half day

            // Assert - Verify effective times
            Assert.Equal(new TimeSpan(7, 30, 0), config.GetEffectiveMorningTime(earlyMeetingDay));
            Assert.Equal(new TimeSpan(17, 0, 0), config.GetEffectiveEveningTime(earlyMeetingDay)); // Default evening

            Assert.Equal(new TimeSpan(9, 0, 0), config.GetEffectiveMorningTime(lateWorkDay)); // Default morning
            Assert.Equal(new TimeSpan(20, 0, 0), config.GetEffectiveEveningTime(lateWorkDay));

            Assert.Equal(new TimeSpan(9, 0, 0), config.GetEffectiveMorningTime(halfDay)); // Default morning
            Assert.Equal(new TimeSpan(12, 0, 0), config.GetEffectiveEveningTime(halfDay));
        }

        [Fact]
        public void RealWorldScenario_ScheduleMaintenanceWorkflow_ShouldWorkCorrectly()
        {
            // Arrange - Simulate schedule maintenance over time
            var config = new ScheduleConfig();
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Act - Add overrides over time
            for (int daysBack = 50; daysBack >= 0; daysBack--)
            {
                var date = today.AddDays(-daysBack);
                if (daysBack % 7 == 0) // Weekly special schedule
                {
                    config.SetOverride(date, new TimeSpan(10, 0, 0));
                }
            }

            var overridesBeforeCleanup = config.Overrides.Count;

            // Act - Cleanup old overrides
            config.CleanupOldOverrides();

            // Assert - Old overrides removed, recent ones kept
            var overridesAfterCleanup = config.Overrides.Count;
            Assert.True(overridesAfterCleanup < overridesBeforeCleanup);
            Assert.True(config.Overrides.All(o => o.Date >= today.AddDays(-30)));
        }

        #endregion
    }
}