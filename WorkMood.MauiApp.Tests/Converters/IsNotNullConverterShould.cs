using System.Globalization;
using WorkMood.MauiApp.Converters;
using Xunit;

namespace WorkMood.MauiApp.Tests.Converters
{
    /// <summary>
    /// Tests for IsNotNullConverter - MAUI value converter for null checking
    /// Follows established IValueConverter testing patterns for MAUI converters
    /// </summary>
    public class IsNotNullConverterShould
    {
        private readonly IsNotNullConverter _converter;

        public IsNotNullConverterShould()
        {
            _converter = new IsNotNullConverter();
        }

        #region Convert Method Tests - Checkpoint 1: Basic Null Checking Logic

        [Fact]
        public void Convert_WithNullValue_ShouldReturnFalse()
        {
            // Arrange
            object? value = null;
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False((bool)result!);
        }

        [Fact]
        public void Convert_WithNonNullValue_ShouldReturnTrue()
        {
            // Arrange
            var value = "test";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithEmptyString_ShouldReturnTrue()
        {
            // Arrange - empty string is not null
            var value = "";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithZeroInteger_ShouldReturnTrue()
        {
            // Arrange - zero is not null
            var value = 0;
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithFalseBool_ShouldReturnTrue()
        {
            // Arrange - false boolean is not null
            var value = false;
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        #endregion

        #region Type Variety Tests - Checkpoint 2: Universal Type Support

        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(false)]
        public void Convert_WithVariousNonNullTypes_ShouldReturnTrue(object value)
        {
            // Arrange
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithComplexObject_ShouldReturnTrue()
        {
            // Arrange
            var value = new { Name = "Test", Value = 42 };
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithArray_ShouldReturnTrue()
        {
            // Arrange
            var value = new[] { 1, 2, 3 };
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithEmptyArray_ShouldReturnTrue()
        {
            // Arrange - empty array is not null
            var value = new int[0];
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        #endregion

        #region Parameter and Culture Independence Tests - Checkpoint 3: Interface Compliance

        [Fact]
        public void Convert_WithNullParameter_ShouldIgnoreParameter()
        {
            // Arrange
            var value = "test";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithNonNullParameter_ShouldIgnoreParameter()
        {
            // Arrange
            var value = "test";
            var targetType = typeof(bool);
            var parameter = "ignored parameter";
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        [InlineData("ja-JP")]
        public void Convert_WithVariousCultures_ShouldBeCultureIndependent(string cultureName)
        {
            // Arrange
            var value = "test";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = new CultureInfo(cultureName);

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithNullCulture_ShouldStillWork()
        {
            // Arrange
            var value = "test";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            CultureInfo? culture = null;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture!);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        public void Convert_WithVariousTargetTypes_ShouldIgnoreTargetType(Type targetType)
        {
            // Arrange
            var value = "test";
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        #endregion

        #region ConvertBack Method Tests

        [Fact]
        public void ConvertBack_ShouldThrowNotImplementedException()
        {
            // Arrange
            var value = true;
            var targetType = typeof(object);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => 
                _converter.ConvertBack(value, targetType, parameter, culture));
        }

        [Fact]
        public void ConvertBack_WithNullValue_ShouldThrowNotImplementedException()
        {
            // Arrange
            object? value = null;
            var targetType = typeof(object);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => 
                _converter.ConvertBack(value, targetType, parameter, culture));
        }

        #endregion

        #region Real-World XAML Binding Scenarios

        [Fact]
        public void Convert_VisibilityBinding_WithValidData_ShouldReturnTrue()
        {
            // Arrange - simulating IsVisible="{Binding DataProperty, Converter={StaticResource IsNotNullConverter}}"
            var userData = new { Name = "John", Email = "john@example.com" };
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(userData, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_VisibilityBinding_WithNullData_ShouldReturnFalse()
        {
            // Arrange - simulating IsVisible="{Binding DataProperty, Converter={StaticResource IsNotNullConverter}}"
            object? userData = null;
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(userData, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False((bool)result!);
        }

        [Fact]
        public void Convert_EnabledBinding_WithSelectedItem_ShouldReturnTrue()
        {
            // Arrange - simulating IsEnabled="{Binding SelectedItem, Converter={StaticResource IsNotNullConverter}}"
            var selectedItem = new { Id = 1, Name = "Selected Item" };
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(selectedItem, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_ValidationBinding_WithErrorMessage_ShouldReturnTrue()
        {
            // Arrange - simulating IsVisible="{Binding ErrorMessage, Converter={StaticResource IsNotNullConverter}}"
            var errorMessage = "Validation failed";
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(errorMessage, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        #endregion

        #region Edge Cases and Robustness

        [Fact]
        public void Convert_WithDBNull_ShouldReturnTrue()
        {
            // Arrange - DBNull.Value is not null in .NET
            var value = DBNull.Value;
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(value, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!);
        }

        [Fact]
        public void Convert_WithNullableHasValueFalse_ShouldReturnTrue()
        {
            // Arrange - nullable with no value is still not null as an object
            int? nullableInt = null;
            object? boxedNullable = nullableInt; // This boxes to null
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(boxedNullable, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.False((bool)result!); // Boxed nullable with no value becomes null
        }

        [Fact]
        public void Convert_WithNullableHasValueTrue_ShouldReturnTrue()
        {
            // Arrange - nullable with value
            int? nullableInt = 42;
            object? boxedNullable = nullableInt; // This boxes to the value
            var targetType = typeof(bool);
            var parameter = (object?)null;
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = _converter.Convert(boxedNullable, targetType, parameter, culture);

            // Assert
            Assert.IsType<bool>(result);
            Assert.True((bool)result!); // Boxed nullable with value is not null
        }

        #endregion
    }
}