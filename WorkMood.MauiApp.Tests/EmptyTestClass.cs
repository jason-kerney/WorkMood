using Xunit;
using FluentAssertions;

namespace WorkMood.MauiApp.Tests;

public class EmptyTestClass
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        actual.Should().Be(expected);
    }
}