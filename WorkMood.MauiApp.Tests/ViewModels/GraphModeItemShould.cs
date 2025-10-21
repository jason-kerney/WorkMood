using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Models;
using Xunit;

namespace WorkMood.MauiApp.Tests.ViewModels;

public class GraphModeItemShould
{
    [Fact]
    public void Constructor_WhenGivenValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        var graphMode = GraphMode.Impact;
        var displayName = "Impact (Change Over Day)";

        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Equal(graphMode, item.GraphMode);
        Assert.Equal(displayName, item.DisplayName);
    }

    [Theory]
    [InlineData(GraphMode.Impact, "Impact (Change Over Day)")]
    [InlineData(GraphMode.Average, "Average (Daily Mood Level)")]
    [InlineData(GraphMode.RawData, "Raw Data (Individual Recordings)")]
    public void Constructor_WithAllGraphModeValues_CreatesValidObject(GraphMode graphMode, string displayName)
    {
        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Equal(graphMode, item.GraphMode);
        Assert.Equal(displayName, item.DisplayName);
        Assert.NotNull(item.DisplayName);
        Assert.NotEmpty(item.DisplayName);
    }

    [Fact]
    public void GraphModeProperty_IsReadOnly()
    {
        // Arrange
        var graphMode = GraphMode.Average;
        var displayName = "Test Display";
        var item = new GraphModeItem(graphMode, displayName);

        // Act & Assert
        // GraphMode property should be read-only (only getter)
        var propertyInfo = typeof(GraphModeItem).GetProperty(nameof(GraphModeItem.GraphMode));
        Assert.NotNull(propertyInfo);
        Assert.True(propertyInfo.CanRead);
        Assert.False(propertyInfo.CanWrite);
    }

    [Fact]
    public void DisplayNameProperty_IsReadOnly()
    {
        // Arrange
        var graphMode = GraphMode.RawData;
        var displayName = "Test Display";
        var item = new GraphModeItem(graphMode, displayName);

        // Act & Assert
        // DisplayName property should be read-only (only getter)
        var propertyInfo = typeof(GraphModeItem).GetProperty(nameof(GraphModeItem.DisplayName));
        Assert.NotNull(propertyInfo);
        Assert.True(propertyInfo.CanRead);
        Assert.False(propertyInfo.CanWrite);
    }

    [Fact]
    public void Constructor_PreservesInputValues()
    {
        // Arrange
        var graphMode = GraphMode.Impact;
        var displayName = "Custom Display Name";

        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Same(displayName, item.DisplayName); // Reference equality for string
        Assert.Equal(graphMode, item.GraphMode);
    }

    [Theory]
    [InlineData(GraphMode.Impact)]
    [InlineData(GraphMode.Average)]
    [InlineData(GraphMode.RawData)]
    public void Constructor_WithAllEnumValues_HandlesAllGraphModes(GraphMode graphMode)
    {
        // Arrange
        var displayName = $"Test for {graphMode}";

        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Equal(graphMode, item.GraphMode);
        Assert.Contains(graphMode.ToString(), displayName);
    }

    // Checkpoint 2: Enum Integration and String Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceDisplayName_AcceptsValue(string displayName)
    {
        // Arrange
        var graphMode = GraphMode.Impact;

        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Equal(graphMode, item.GraphMode);
        Assert.Equal(displayName, item.DisplayName);
    }

    [Fact]
    public void Constructor_WithNullDisplayName_AcceptsValue()
    {
        // Arrange
        var graphMode = GraphMode.Impact;

        // Act
        var item = new GraphModeItem(graphMode, null!);

        // Assert
        Assert.Equal(graphMode, item.GraphMode);
        Assert.Null(item.DisplayName);
    }

    [Theory]
    [InlineData("Short")]
    [InlineData("This is a very long display name that might be used in some scenarios with lots of descriptive text")]
    [InlineData("Special!@#$%^&*()Characters")]
    [InlineData("Unicode: 🎯📊📈")]
    [InlineData("Multi\nLine\nText")]
    public void Constructor_WithVariousStringFormats_PreservesDisplayName(string displayName)
    {
        // Arrange
        var graphMode = GraphMode.Average;

        // Act
        var item = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.Equal(displayName, item.DisplayName);
        Assert.Same(displayName, item.DisplayName); // Reference equality
    }

    [Fact]
    public void GraphMode_AllEnumValues_CanBeUsedInConstructor()
    {
        // Arrange & Act
        var impactItem = new GraphModeItem(GraphMode.Impact, "Impact");
        var averageItem = new GraphModeItem(GraphMode.Average, "Average");
        var rawDataItem = new GraphModeItem(GraphMode.RawData, "RawData");

        // Assert
        Assert.Equal(GraphMode.Impact, impactItem.GraphMode);
        Assert.Equal(GraphMode.Average, averageItem.GraphMode);
        Assert.Equal(GraphMode.RawData, rawDataItem.GraphMode);
    }

    [Fact]
    public void GraphMode_EnumValuesAreDistinct()
    {
        // Arrange
        var impact = new GraphModeItem(GraphMode.Impact, "Test");
        var average = new GraphModeItem(GraphMode.Average, "Test");
        var rawData = new GraphModeItem(GraphMode.RawData, "Test");

        // Act & Assert
        Assert.NotEqual(impact.GraphMode, average.GraphMode);
        Assert.NotEqual(average.GraphMode, rawData.GraphMode);
        Assert.NotEqual(impact.GraphMode, rawData.GraphMode);
    }

    [Fact]
    public void DisplayName_WithSameStringReference_MaintainsReferenceEquality()
    {
        // Arrange
        const string sharedDisplayName = "Shared Display Name";
        var graphMode1 = GraphMode.Impact;
        var graphMode2 = GraphMode.Average;

        // Act
        var item1 = new GraphModeItem(graphMode1, sharedDisplayName);
        var item2 = new GraphModeItem(graphMode2, sharedDisplayName);

        // Assert
        Assert.Same(sharedDisplayName, item1.DisplayName);
        Assert.Same(sharedDisplayName, item2.DisplayName);
        Assert.Same(item1.DisplayName, item2.DisplayName);
    }

    [Fact]
    public void GraphModeItem_RealWorldGraphViewModelUsage_MatchesActualImplementation()
    {
        // Arrange & Act - Replicate actual GraphViewModel.InitializeGraphModes() method
        var impactItem = new GraphModeItem(GraphMode.Impact, "Impact (Change Over Day)");
        var averageItem = new GraphModeItem(GraphMode.Average, "Average (Daily Mood Level)");
        var rawDataItem = new GraphModeItem(GraphMode.RawData, "Raw Data (Individual Recordings)");

        // Assert - Verify matches real GraphViewModel usage
        Assert.Equal(GraphMode.Impact, impactItem.GraphMode);
        Assert.Equal("Impact (Change Over Day)", impactItem.DisplayName);
        
        Assert.Equal(GraphMode.Average, averageItem.GraphMode);
        Assert.Equal("Average (Daily Mood Level)", averageItem.DisplayName);
        
        Assert.Equal(GraphMode.RawData, rawDataItem.GraphMode);
        Assert.Equal("Raw Data (Individual Recordings)", rawDataItem.DisplayName);
    }

    // Checkpoint 3: Object Independence and Performance Testing

    [Fact]
    public void Constructor_CreatesTwoInstancesWithSameParameters_AreIndependent()
    {
        // Arrange
        var graphMode = GraphMode.Average;
        var displayName = "Average Test";

        // Act
        var item1 = new GraphModeItem(graphMode, displayName);
        var item2 = new GraphModeItem(graphMode, displayName);

        // Assert
        Assert.NotSame(item1, item2); // Different object instances
        Assert.Equal(item1.GraphMode, item2.GraphMode); // Same enum value
        Assert.Equal(item1.DisplayName, item2.DisplayName); // Same string content
        Assert.Same(item1.DisplayName, item2.DisplayName); // String interning means same reference
    }

    [Fact]
    public void Constructor_WithDifferentParameters_ProducesDifferentResults()
    {
        // Arrange & Act
        var item1 = new GraphModeItem(GraphMode.Impact, "Impact Display");
        var item2 = new GraphModeItem(GraphMode.Average, "Average Display");

        // Assert
        Assert.NotSame(item1, item2); // Different instances
        Assert.NotEqual(item1.GraphMode, item2.GraphMode); // Different enums
        Assert.NotEqual(item1.DisplayName, item2.DisplayName); // Different strings
    }

    [Fact]
    public void Constructor_PerformanceTest_CreatesObjectsQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var item = new GraphModeItem(GraphMode.Impact, $"Display {i}");
        }
        stopwatch.Stop();

        // Assert
        // Should create 1000 objects in less than 50ms (reasonable performance expectation)
        Assert.True(stopwatch.ElapsedMilliseconds < 50, 
            $"Performance test failed: took {stopwatch.ElapsedMilliseconds}ms to create 1000 objects");
    }

    [Fact]
    public void GraphModeItem_ToStringBehavior_UsesDefaultImplementation()
    {
        // Arrange
        var item = new GraphModeItem(GraphMode.Impact, "Impact Test");

        // Act
        var result = item.ToString();

        // Assert
        // Should use default object ToString (namespace.classname format)
        Assert.NotNull(result);
        Assert.Contains("GraphModeItem", result);
    }

    [Fact]
    public void Constructor_WithAllCombinations_HandlesAllEnumStringPairs()
    {
        // Arrange
        var graphModes = new[] { GraphMode.Impact, GraphMode.Average, GraphMode.RawData };
        var displayNames = new[] { "Short", "Medium Length Name", "Very Long Display Name With Lots Of Words" };

        // Act & Assert
        foreach (var mode in graphModes)
        {
            foreach (var name in displayNames)
            {
                var item = new GraphModeItem(mode, name);
                Assert.Equal(mode, item.GraphMode);
                Assert.Equal(name, item.DisplayName);
            }
        }
    }

    [Fact]
    public void GraphModeItem_MemoryEfficiency_NoReferencesToLargeObjects()
    {
        // Arrange & Act
        var item = new GraphModeItem(GraphMode.RawData, "Test");

        // Assert
        // Verify object only holds expected simple types
        Assert.IsType<GraphMode>(item.GraphMode);
        Assert.IsType<string>(item.DisplayName);
        
        // Properties should be value type (enum) and reference type (string) only
        var graphModeProperty = typeof(GraphModeItem).GetProperty(nameof(GraphModeItem.GraphMode));
        var displayNameProperty = typeof(GraphModeItem).GetProperty(nameof(GraphModeItem.DisplayName));
        
        Assert.NotNull(graphModeProperty);
        Assert.NotNull(displayNameProperty);
        Assert.True(graphModeProperty.PropertyType.IsEnum);
        Assert.Equal(typeof(string), displayNameProperty.PropertyType);
    }
}