using Xunit;
using System;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.Tests.Services;

/// <summary>
/// Tests for MorningReminderData class.
/// Tests data transfer object functionality, property behavior, and data integrity.
/// </summary>
public class MorningReminderDataShould
{
    #region Checkpoint 1: Basic Data Transfer Object Structure and Property Validation

    [Fact]
    public void BePublicClass_WhenAccessingMorningReminderData()
    {
        // Arrange & Act
        var type = typeof(MorningReminderData);

        // Assert
        Assert.True(type.IsClass);
        Assert.True(type.IsPublic);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsSealed);
    }

    [Fact]
    public void HaveParameterlessConstructor_WhenCreatingInstance()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new MorningReminderData());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void CreateInstanceSuccessfully_WhenUsingDefaultConstructor()
    {
        // Arrange & Act
        var morningReminderData = new MorningReminderData();

        // Assert
        Assert.NotNull(morningReminderData);
        Assert.IsType<MorningReminderData>(morningReminderData);
    }

    [Fact]
    public void HaveMorningTimeProperty_WithGetterAndSetter()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var testDateTime = new DateTime(2025, 10, 21, 7, 30, 0);

        // Act
        morningReminderData.MorningTime = testDateTime;

        // Assert
        Assert.Equal(testDateTime, morningReminderData.MorningTime);
    }

    [Fact]
    public void HaveTimeSinceMorningProperty_WithGetterAndSetter()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var testTimeSpan = TimeSpan.FromHours(2.5);

        // Act
        morningReminderData.TimeSinceMorning = testTimeSpan;

        // Assert
        Assert.Equal(testTimeSpan, morningReminderData.TimeSinceMorning);
    }

    [Fact]
    public void HaveCallCountProperty_WithGetterAndSetter()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var testCallCount = 42;

        // Act
        morningReminderData.CallCount = testCallCount;

        // Assert
        Assert.Equal(testCallCount, morningReminderData.CallCount);
    }

    [Fact]
    public void InitializeWithDefaultValues_WhenCreatedWithDefaultConstructor()
    {
        // Arrange & Act
        var morningReminderData = new MorningReminderData();

        // Assert
        Assert.Equal(default(DateTime), morningReminderData.MorningTime);
        Assert.Equal(default(TimeSpan), morningReminderData.TimeSinceMorning);
        Assert.Equal(default(int), morningReminderData.CallCount);
    }

    [Fact]
    public void AllowMultiplePropertyAssignments_WhenModifyingProperties()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var firstDateTime = new DateTime(2025, 10, 21, 7, 0, 0);
        var secondDateTime = new DateTime(2025, 10, 21, 8, 0, 0);

        // Act
        morningReminderData.MorningTime = firstDateTime;
        morningReminderData.MorningTime = secondDateTime;

        // Assert
        Assert.Equal(secondDateTime, morningReminderData.MorningTime);
    }

    #endregion

    #region Checkpoint 2: Business Logic and Data Integrity Testing

    [Fact]
    public void HandleMinimumDateTime_WhenAssigningMorningTime()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var minDateTime = DateTime.MinValue;

        // Act
        morningReminderData.MorningTime = minDateTime;

        // Assert
        Assert.Equal(minDateTime, morningReminderData.MorningTime);
    }

    [Fact]
    public void HandleMaximumDateTime_WhenAssigningMorningTime()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var maxDateTime = DateTime.MaxValue;

        // Act
        morningReminderData.MorningTime = maxDateTime;

        // Assert
        Assert.Equal(maxDateTime, morningReminderData.MorningTime);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(0, 30, 0)]
    [InlineData(0, 0, 45)]
    [InlineData(23, 59, 59)]
    [InlineData(2, 15, 30)]
    public void HandleVariousTimeSpanValues_WhenAssigningTimeSinceMorning(int hours, int minutes, int seconds)
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var timeSpan = new TimeSpan(hours, minutes, seconds);

        // Act
        morningReminderData.TimeSinceMorning = timeSpan;

        // Assert
        Assert.Equal(timeSpan, morningReminderData.TimeSinceMorning);
    }

    [Fact]
    public void HandleNegativeTimeSpan_WhenAssigningTimeSinceMorning()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var negativeTimeSpan = TimeSpan.FromHours(-1);

        // Act
        morningReminderData.TimeSinceMorning = negativeTimeSpan;

        // Assert
        Assert.Equal(negativeTimeSpan, morningReminderData.TimeSinceMorning);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void HandlePositiveCallCount_WhenAssigningCallCount(int callCount)
    {
        // Arrange
        var morningReminderData = new MorningReminderData();

        // Act
        morningReminderData.CallCount = callCount;

        // Assert
        Assert.Equal(callCount, morningReminderData.CallCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-42)]
    [InlineData(int.MinValue)]
    public void HandleNegativeCallCount_WhenAssigningCallCount(int callCount)
    {
        // Arrange
        var morningReminderData = new MorningReminderData();

        // Act
        morningReminderData.CallCount = callCount;

        // Assert
        Assert.Equal(callCount, morningReminderData.CallCount);
    }

    [Fact]
    public void MaintainIndependentPropertyValues_WhenSettingAllProperties()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();
        var testDateTime = new DateTime(2025, 10, 21, 8, 15, 30);
        var testTimeSpan = TimeSpan.FromMinutes(45);
        var testCallCount = 7;

        // Act
        morningReminderData.MorningTime = testDateTime;
        morningReminderData.TimeSinceMorning = testTimeSpan;
        morningReminderData.CallCount = testCallCount;

        // Assert
        Assert.Equal(testDateTime, morningReminderData.MorningTime);
        Assert.Equal(testTimeSpan, morningReminderData.TimeSinceMorning);
        Assert.Equal(testCallCount, morningReminderData.CallCount);
    }

    #endregion

    #region Checkpoint 3: Integration Patterns, Edge Cases, and Object Behavior

    [Fact]
    public void BeReferenceType_WhenComparingInstances()
    {
        // Arrange
        var firstInstance = new MorningReminderData();
        var secondInstance = new MorningReminderData();

        // Act & Assert
        Assert.NotSame(firstInstance, secondInstance);
        Assert.False(ReferenceEquals(firstInstance, secondInstance));
    }

    [Fact]
    public void AllowPropertyModificationAfterConstruction_WhenUpdatingValues()
    {
        // Arrange
        var morningReminderData = new MorningReminderData
        {
            MorningTime = new DateTime(2025, 10, 21, 6, 30, 0),
            TimeSinceMorning = TimeSpan.FromHours(1),
            CallCount = 5
        };

        // Act
        morningReminderData.MorningTime = new DateTime(2025, 10, 21, 7, 30, 0);
        morningReminderData.TimeSinceMorning = TimeSpan.FromHours(2);
        morningReminderData.CallCount = 10;

        // Assert
        Assert.Equal(new DateTime(2025, 10, 21, 7, 30, 0), morningReminderData.MorningTime);
        Assert.Equal(TimeSpan.FromHours(2), morningReminderData.TimeSinceMorning);
        Assert.Equal(10, morningReminderData.CallCount);
    }

    [Fact]
    public void SupportObjectInitializerSyntax_WhenCreatingInstance()
    {
        // Arrange
        var testDateTime = new DateTime(2025, 10, 21, 9, 0, 0);
        var testTimeSpan = TimeSpan.FromMinutes(30);
        var testCallCount = 3;

        // Act
        var morningReminderData = new MorningReminderData
        {
            MorningTime = testDateTime,
            TimeSinceMorning = testTimeSpan,
            CallCount = testCallCount
        };

        // Assert
        Assert.Equal(testDateTime, morningReminderData.MorningTime);
        Assert.Equal(testTimeSpan, morningReminderData.TimeSinceMorning);
        Assert.Equal(testCallCount, morningReminderData.CallCount);
    }

    [Fact]
    public void HandleConcurrentPropertyAccess_WhenAccessingMultipleProperties()
    {
        // Arrange
        var morningReminderData = new MorningReminderData
        {
            MorningTime = new DateTime(2025, 10, 21, 7, 45, 0),
            TimeSinceMorning = TimeSpan.FromMinutes(75),
            CallCount = 12
        };

        // Act & Assert - Multiple property access should work consistently
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(new DateTime(2025, 10, 21, 7, 45, 0), morningReminderData.MorningTime);
            Assert.Equal(TimeSpan.FromMinutes(75), morningReminderData.TimeSinceMorning);
            Assert.Equal(12, morningReminderData.CallCount);
        }
    }

    [Fact]
    public void MaintainPropertyIndependence_WhenModifyingOneProperty()
    {
        // Arrange
        var morningReminderData = new MorningReminderData
        {
            MorningTime = new DateTime(2025, 10, 21, 6, 0, 0),
            TimeSinceMorning = TimeSpan.FromHours(1),
            CallCount = 4
        };
        var originalTimeSinceMorning = morningReminderData.TimeSinceMorning;
        var originalCallCount = morningReminderData.CallCount;

        // Act - Modify only MorningTime
        morningReminderData.MorningTime = new DateTime(2025, 10, 21, 8, 0, 0);

        // Assert - Other properties should remain unchanged
        Assert.Equal(new DateTime(2025, 10, 21, 8, 0, 0), morningReminderData.MorningTime);
        Assert.Equal(originalTimeSinceMorning, morningReminderData.TimeSinceMorning);
        Assert.Equal(originalCallCount, morningReminderData.CallCount);
    }

    [Fact]
    public void InheritFromObjectClass_WhenExaminingInheritance()
    {
        // Arrange & Act
        var type = typeof(MorningReminderData);

        // Assert
        Assert.Equal(typeof(object), type.BaseType);
    }

    [Fact]
    public void SupportToStringMethod_WhenCallingToString()
    {
        // Arrange
        var morningReminderData = new MorningReminderData();

        // Act
        var result = morningReminderData.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        // Default Object.ToString() returns the type name
        Assert.Contains("MorningReminderData", result);
    }

    [Fact]
    public void HandleComplexScenario_WithRealisticMorningReminderData()
    {
        // Arrange - Simulate a realistic morning reminder scenario
        var morningTime = new DateTime(2025, 10, 21, 7, 0, 0); // 7:00 AM
        var currentTime = new DateTime(2025, 10, 21, 9, 30, 0); // 9:30 AM
        var timeSinceMorning = currentTime - morningTime; // 2.5 hours
        var callCount = 3; // Third reminder call

        // Act
        var morningReminderData = new MorningReminderData
        {
            MorningTime = morningTime,
            TimeSinceMorning = timeSinceMorning,
            CallCount = callCount
        };

        // Assert
        Assert.Equal(morningTime, morningReminderData.MorningTime);
        Assert.Equal(TimeSpan.FromHours(2.5), morningReminderData.TimeSinceMorning);
        Assert.Equal(3, morningReminderData.CallCount);
    }

    #endregion
}