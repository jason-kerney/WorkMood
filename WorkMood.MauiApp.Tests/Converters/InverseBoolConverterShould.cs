using System.Globalization;
using WorkMood.MauiApp.Converters;
using Xunit;

namespace WorkMood.MauiApp.Tests.Converters;

/// <summary>
/// Tests for InverseBoolConverter - MAUI value converter that inverts boolean values
/// Component 11 of Master Test Execution Plan
/// </summary>
public class InverseBoolConverterShould
{
    #region Checkpoint 1: IValueConverter Interface Implementation and Basic Boolean Inversion

    [Fact]
    public void Implement_IValueConverter_Interface()
    {
        // Arrange & Act
        var converter = new InverseBoolConverter();

        // Assert
        Assert.IsAssignableFrom<Microsoft.Maui.Controls.IValueConverter>(converter);
    }

    [Fact]
    public void Convert_Should_ReturnFalse_WhenValueIsTrue()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act
        var result = converter.Convert(trueValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_Should_ReturnTrue_WhenValueIsFalse()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var falseValue = false;

        // Act
        var result = converter.Convert(falseValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_Should_ReturnFalse_WhenValueIsNull()
    {
        // Arrange
        var converter = new InverseBoolConverter();

        // Act
        var result = converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_Should_InvertBooleanValuesCorrectly()
    {
        // Arrange
        var converter = new InverseBoolConverter();

        // Act & Assert - Test both directions
        var trueResult = converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture);
        var falseResult = converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.False((bool)trueResult!);
        Assert.True((bool)falseResult!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnFalse_WhenValueIsTrue()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act
        var result = converter.ConvertBack(trueValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnTrue_WhenValueIsFalse()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var falseValue = false;

        // Act
        var result = converter.ConvertBack(falseValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.True((bool)result!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnFalse_WhenValueIsNull()
    {
        // Arrange
        var converter = new InverseBoolConverter();

        // Act
        var result = converter.ConvertBack(null, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    #endregion

    #region Checkpoint 2: Parameter Handling, Culture Info, Edge Cases, and Invalid Inputs

    [Fact]
    public void Convert_Should_ReturnFalse_WhenValueIsString()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var stringValue = "true";

        // Act
        var result = converter.Convert(stringValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_Should_ReturnFalse_WhenValueIsInteger()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var intValue = 1;

        // Act
        var result = converter.Convert(intValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_Should_ReturnFalse_WhenValueIsObject()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var objectValue = new object();

        // Act
        var result = converter.Convert(objectValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void Convert_Should_IgnoreTargetType()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act - Use different target types
        var stringTypeResult = converter.Convert(trueValue, typeof(string), null, CultureInfo.InvariantCulture);
        var objectTypeResult = converter.Convert(trueValue, typeof(object), null, CultureInfo.InvariantCulture);

        // Assert - Should always return inverted boolean regardless of target type
        Assert.IsType<bool>(stringTypeResult);
        Assert.IsType<bool>(objectTypeResult);
        Assert.False((bool)stringTypeResult!);
        Assert.False((bool)objectTypeResult!);
    }

    [Fact]
    public void Convert_Should_IgnoreParameter()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act - Use different parameters
        var nullParamResult = converter.Convert(trueValue, typeof(bool), null, CultureInfo.InvariantCulture);
        var stringParamResult = converter.Convert(trueValue, typeof(bool), "parameter", CultureInfo.InvariantCulture);
        var objectParamResult = converter.Convert(trueValue, typeof(bool), new object(), CultureInfo.InvariantCulture);

        // Assert - Should always return inverted boolean regardless of parameter
        Assert.False((bool)nullParamResult!);
        Assert.False((bool)stringParamResult!);
        Assert.False((bool)objectParamResult!);
    }

    [Fact]
    public void Convert_Should_IgnoreCultureInfo()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act - Use different cultures
        var invariantResult = converter.Convert(trueValue, typeof(bool), null, CultureInfo.InvariantCulture);
        var usResult = converter.Convert(trueValue, typeof(bool), null, new CultureInfo("en-US"));
        var frResult = converter.Convert(trueValue, typeof(bool), null, new CultureInfo("fr-FR"));
        var nullCultureResult = converter.Convert(trueValue, typeof(bool), null, null!);

        // Assert - Should always return inverted boolean regardless of culture
        Assert.False((bool)invariantResult!);
        Assert.False((bool)usResult!);
        Assert.False((bool)frResult!);
        Assert.False((bool)nullCultureResult!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnFalse_WhenValueIsString()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var stringValue = "false";

        // Act
        var result = converter.ConvertBack(stringValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnFalse_WhenValueIsInteger()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var intValue = 0;

        // Act
        var result = converter.ConvertBack(intValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertBack_Should_ReturnFalse_WhenValueIsObject()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var objectValue = new object();

        // Act
        var result = converter.ConvertBack(objectValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertBack_Should_IgnoreTargetTypeParameterAndCulture()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var trueValue = true;

        // Act - Use different parameters
        var result = converter.ConvertBack(trueValue, typeof(string), "param", new CultureInfo("es-ES"));

        // Assert - Should always return inverted boolean regardless of other parameters
        Assert.IsType<bool>(result);
        Assert.False((bool)result!);
    }

    #endregion

    #region Checkpoint 3: Bidirectional Testing and XAML Binding Compatibility

    [Fact]
    public void Convert_And_ConvertBack_Should_Be_Symmetric()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var originalTrue = true;
        var originalFalse = false;

        // Act - Convert then ConvertBack
        var convertedTrue = converter.Convert(originalTrue, typeof(bool), null, CultureInfo.InvariantCulture);
        var backToOriginalTrue = converter.ConvertBack(convertedTrue, typeof(bool), null, CultureInfo.InvariantCulture);

        var convertedFalse = converter.Convert(originalFalse, typeof(bool), null, CultureInfo.InvariantCulture);
        var backToOriginalFalse = converter.ConvertBack(convertedFalse, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert - Converting and converting back should return to original
        Assert.Equal(originalTrue, backToOriginalTrue);
        Assert.Equal(originalFalse, backToOriginalFalse);
    }

    [Fact]
    public void ConvertBack_And_Convert_Should_Be_Symmetric()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var originalTrue = true;
        var originalFalse = false;

        // Act - ConvertBack then Convert
        var convertedBackTrue = converter.ConvertBack(originalTrue, typeof(bool), null, CultureInfo.InvariantCulture);
        var backToOriginalTrue = converter.Convert(convertedBackTrue, typeof(bool), null, CultureInfo.InvariantCulture);

        var convertedBackFalse = converter.ConvertBack(originalFalse, typeof(bool), null, CultureInfo.InvariantCulture);
        var backToOriginalFalse = converter.Convert(convertedBackFalse, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert - Converting back and converting should return to original
        Assert.Equal(originalTrue, backToOriginalTrue);
        Assert.Equal(originalFalse, backToOriginalFalse);
    }

    [Fact]
    public void Convert_And_ConvertBack_Should_Have_Identical_Logic()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var testValues = new object?[] { true, false, null, "string", 42, new object() };

        // Act & Assert - Both methods should return identical results for same inputs
        foreach (var value in testValues)
        {
            var convertResult = converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture);
            var convertBackResult = converter.ConvertBack(value, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(convertResult, convertBackResult);
        }
    }

    [Fact]
    public void Should_Support_XAML_Binding_Scenarios()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        
        // Act & Assert - Test common XAML binding scenarios
        
        // Scenario 1: Button IsEnabled bound to inverted CanExecute
        var canExecute = true;
        var buttonIsEnabled = converter.Convert(canExecute, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False((bool)buttonIsEnabled!); // Button disabled when command can execute
        
        // Scenario 2: Visibility converter (inverted)
        var isVisible = false;
        var isHidden = converter.Convert(isVisible, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.True((bool)isHidden!); // Should be hidden when not visible
        
        // Scenario 3: Checkbox state inversion
        var checkboxChecked = true;
        var invertedState = converter.Convert(checkboxChecked, typeof(bool), null, CultureInfo.InvariantCulture);
        Assert.False((bool)invertedState!); // Inverted state should be false
    }

    [Fact]
    public void Should_Handle_Rapid_Successive_Conversions()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var iterations = 1000;

        // Act & Assert - Test performance and consistency with rapid conversions
        for (int i = 0; i < iterations; i++)
        {
            var input = i % 2 == 0; // Alternating true/false
            var converted = converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
            var convertedBack = converter.ConvertBack(converted, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(input, convertedBack);
        }
    }

    [Fact]
    public void Should_Be_Thread_Safe()
    {
        // Arrange
        var converter = new InverseBoolConverter();
        var results = new bool[100];
        var tasks = new Task[100];

        // Act - Run conversions in parallel
        for (int i = 0; i < 100; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() =>
            {
                var input = index % 2 == 0;
                var result = converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
                results[index] = (bool)result!;
            });
        }

        Task.WaitAll(tasks);

        // Assert - All results should be correct inversions
        for (int i = 0; i < 100; i++)
        {
            var expected = !(i % 2 == 0); // Inverted
            Assert.Equal(expected, results[i]);
        }
    }

    [Fact]
    public void Should_Maintain_Consistent_Behavior_Across_Multiple_Instances()
    {
        // Arrange
        var converter1 = new InverseBoolConverter();
        var converter2 = new InverseBoolConverter();
        var testValue = true;

        // Act
        var result1 = converter1.Convert(testValue, typeof(bool), null, CultureInfo.InvariantCulture);
        var result2 = converter2.Convert(testValue, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert - Multiple instances should behave identically
        Assert.Equal(result1, result2);
        Assert.False((bool)result1!);
        Assert.False((bool)result2!);
    }

    #endregion
}