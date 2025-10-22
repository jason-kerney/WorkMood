using System;
using System.Globalization;
using Xunit;
using WorkMood.MauiApp.Converters;

namespace WorkMood.MauiApp.Tests.Converters
{
    public class NullableMoodConverterShould
    {
        private readonly NullableMoodConverter _converter;

        public NullableMoodConverterShould()
        {
            _converter = new NullableMoodConverter();
        }

        #region Interface Compliance Tests

        [Fact]
        public void BeAssignableFromIValueConverter()
        {
            // Assert
            Assert.IsAssignableFrom<Microsoft.Maui.Controls.IValueConverter>(_converter);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack("5", typeof(int), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ConvertBack_ThrowsForAnyStringInput()
        {
            // Arrange
            var testInputs = new[] { "1", "42", "—", "test", "" };

            // Act & Assert
            foreach (var input in testInputs)
            {
                Assert.Throws<NotImplementedException>(() =>
                    _converter.ConvertBack(input, typeof(int), null, CultureInfo.InvariantCulture));
            }
        }

        [Fact]
        public void ConvertBack_ThrowsForNullInput()
        {
            // Act & Assert
            Assert.Throws<NotImplementedException>(() =>
                _converter.ConvertBack(null, typeof(int), null, CultureInfo.InvariantCulture));
        }

        #endregion

        #region Integer Values Tests

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenValueIsPositiveInteger()
        {
            // Arrange
            var positiveIntegers = new[] { 1, 5, 10, 42, 100, 999 };

            foreach (var value in positiveIntegers)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal(value.ToString(), result);
                Assert.IsType<string>(result);
            }
        }

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenValueIsNegativeInteger()
        {
            // Arrange
            var negativeIntegers = new[] { -1, -5, -10, -42, -100, -999 };

            foreach (var value in negativeIntegers)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal(value.ToString(), result);
                Assert.IsType<string>(result);
            }
        }

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenValueIsZero()
        {
            // Act
            var result = _converter.Convert(0, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("0", result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenValueIsMaxInteger()
        {
            // Act
            var result = _converter.Convert(int.MaxValue, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(int.MaxValue.ToString(), result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenValueIsMinInteger()
        {
            // Act
            var result = _converter.Convert(int.MinValue, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(int.MinValue.ToString(), result);
            Assert.IsType<string>(result);
        }

        [Theory]
        [InlineData(1000000)]
        [InlineData(-1000000)]
        [InlineData(2147483647)] // int.MaxValue
        [InlineData(-2147483648)] // int.MinValue
        [InlineData(1234567)]
        [InlineData(-9876543)]
        public void Convert_HandlesLargeIntegers_Correctly(int value)
        {
            // Act
            var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(value.ToString(), result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region Null Values Tests

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsNull()
        {
            // Act
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsExplicitNull()
        {
            // Arrange
            object? value = null;

            // Act
            var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        [Fact]
        public void Convert_HandlesNullReference_Consistently()
        {
            // Arrange
            object? nullRef = null;

            // Act
            var result1 = _converter.Convert(nullRef, typeof(string), null, CultureInfo.InvariantCulture);
            var result2 = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result1);
            Assert.Equal("—", result2);
            Assert.Equal(result1, result2);
        }

        #endregion

        #region Non-Integer Types Tests

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsString()
        {
            // Arrange
            var stringValues = new[] { "test", "42", "hello", "", "null" };

            foreach (var value in stringValues)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsDouble()
        {
            // Arrange
            var doubleValues = new[] { 1.5, -2.7, 0.0, double.MaxValue, double.MinValue };

            foreach (var value in doubleValues)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsBoolean()
        {
            // Act
            var resultTrue = _converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
            var resultFalse = _converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", resultTrue);
            Assert.Equal("—", resultFalse);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsObject()
        {
            // Arrange
            var obj = new object();

            // Act
            var result = _converter.Convert(obj, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsDateTime()
        {
            // Arrange
            var dateTime = DateTime.Now;

            // Act
            var result = _converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsCollection()
        {
            // Arrange
            var list = new[] { 1, 2, 3 };

            // Act
            var result = _converter.Convert(list, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsComplexType()
        {
            // Arrange
            var complexType = new { Property = "value", Number = 42 };

            // Act
            var result = _converter.Convert(complexType, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        #endregion

        #region Nullable Integer Tests

        [Fact]
        public void Convert_ReturnsStringRepresentation_WhenNullableIntHasValue()
        {
            // Arrange
            int? nullableInt = 42;

            // Act
            var result = _converter.Convert(nullableInt, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("42", result);
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenNullableIntIsNull()
        {
            // Arrange
            int? nullableInt = null;

            // Act
            var result = _converter.Convert(nullableInt, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(-999)]
        public void Convert_HandlesNullableIntegerEdgeCases(int? value)
        {
            // Act
            var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            if (value.HasValue)
            {
                Assert.Equal(value.Value.ToString(), result);
            }
            else
            {
                Assert.Equal("—", result);
            }
        }

        #endregion

        #region Special Values Tests

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsStringInteger()
        {
            // Arrange
            var stringIntegers = new[] { "42", "0", "-1", "999" };

            foreach (var value in stringIntegers)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsFloatingPoint()
        {
            // Arrange
            var floatingValues = new object[] { 42.0f, 42.0d, 42.0m };

            foreach (var value in floatingValues)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsNumericString()
        {
            // Arrange
            var numericStrings = new[] { "123", "0", "-456", "3.14", "42.0" };

            foreach (var value in numericStrings)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_ReturnsEmDash_WhenValueIsEmptyString()
        {
            // Act
            var result = _converter.Convert("", typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("—", result);
        }

        #endregion

        #region Parameter Handling Tests

        [Fact]
        public void Convert_IgnoresParameter_ForIntegerValue()
        {
            // Arrange
            var parameters = new object?[] { "ignored", 999, true, null };

            foreach (var parameter in parameters)
            {
                // Act
                var result = _converter.Convert(42, typeof(string), parameter, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("42", result);
            }
        }

        [Fact]
        public void Convert_IgnoresParameter_ForNullValue()
        {
            // Arrange
            var parameters = new object[] { "ignored", 999, true };

            foreach (var parameter in parameters)
            {
                // Act
                var result = _converter.Convert(null, typeof(string), parameter, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_IgnoresTargetType_InAllScenarios()
        {
            // Arrange
            var targetTypes = new[] { typeof(object), typeof(int), typeof(bool) };

            foreach (var targetType in targetTypes)
            {
                // Act
                var resultInt = _converter.Convert(42, targetType, null, CultureInfo.InvariantCulture);
                var resultNull = _converter.Convert(null, targetType, null, CultureInfo.InvariantCulture);

                // Assert
                Assert.Equal("42", resultInt);
                Assert.Equal("—", resultNull);
            }
        }

        [Fact]
        public void Convert_IgnoresCulture_InAllScenarios()
        {
            // Arrange
            var cultures = new[] 
            { 
                CultureInfo.InvariantCulture, 
                new CultureInfo("en-US"), 
                new CultureInfo("de-DE"),
                new CultureInfo("ja-JP")
            };

            foreach (var culture in cultures)
            {
                // Act
                var resultInt = _converter.Convert(42, typeof(string), null, culture);
                var resultNull = _converter.Convert(null, typeof(string), null, culture);

                // Assert
                Assert.Equal("42", resultInt);
                Assert.Equal("—", resultNull);
            }
        }

        [Fact]
        public void Convert_HandlesNullParameters_Gracefully()
        {
            // Act
            var resultInt = _converter.Convert(42, null!, null, null!);
            var resultNull = _converter.Convert(null, null!, null, null!);

            // Assert
            Assert.Equal("42", resultInt);
            Assert.Equal("—", resultNull);
        }

        #endregion

        #region Output Consistency Tests

        [Fact]
        public void Convert_AlwaysReturnsString()
        {
            // Arrange
            var testValues = new object?[] { 42, null, "text", 3.14, true };

            foreach (var value in testValues)
            {
                // Act
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                // Assert
                Assert.IsType<string>(result);
            }
        }

        [Fact]
        public void Convert_ReturnsConsistentString_ForSameInteger()
        {
            // Arrange
            var testInteger = 42;

            // Act
            var result1 = _converter.Convert(testInteger, typeof(string), null, CultureInfo.InvariantCulture);
            var result2 = _converter.Convert(testInteger, typeof(string), null, CultureInfo.InvariantCulture);
            var result3 = _converter.Convert(testInteger, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("42", result1);
            Assert.Equal("42", result2);
            Assert.Equal("42", result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void Convert_ReturnsConsistentEmDash_ForNonIntegers()
        {
            // Arrange
            var nonIntegerValues = new object?[] { null, "text", 3.14, true, new object() };

            // Act & Assert
            foreach (var value in nonIntegerValues)
            {
                var result1 = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
                var result2 = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

                Assert.Equal("—", result1);
                Assert.Equal("—", result2);
                Assert.Equal(result1, result2);
            }
        }

        [Fact]
        public void Convert_MaintainsDeterministicBehavior()
        {
            // Arrange
            var testScenarios = new[]
            {
                new { Value = (object?)42, Expected = "42" },
                new { Value = (object?)null, Expected = "—" },
                new { Value = (object?)"test", Expected = "—" },
                new { Value = (object?)0, Expected = "0" },
                new { Value = (object?)(-1), Expected = "-1" }
            };

            // Act & Assert - Run multiple times to ensure consistency
            for (int i = 0; i < 3; i++)
            {
                foreach (var scenario in testScenarios)
                {
                    var result = _converter.Convert(scenario.Value, typeof(string), null, CultureInfo.InvariantCulture);
                    Assert.Equal(scenario.Expected, result);
                }
            }
        }

        #endregion

        #region Fallback Behavior Tests

        [Fact]
        public void Convert_UsesEmDashAsUniversalFallback()
        {
            // Arrange
            var nonIntegerValues = new object?[]
            {
                null, "string", 3.14, true, false, DateTime.Now, 
                new object(), new[] { 1, 2, 3 }, 'c', (byte)255
            };

            // Act & Assert
            foreach (var value in nonIntegerValues)
            {
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_HandlesTypeConversionFailures()
        {
            // Arrange - Types that might cause confusion with integers
            var ambiguousValues = new object[] { (byte)42, (short)42, (long)42, (uint)42 };

            // Act & Assert
            foreach (var value in ambiguousValues)
            {
                var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);
                
                // These should all return em dash because they're not exactly int type
                Assert.Equal("—", result);
            }
        }

        [Fact]
        public void Convert_HandlesInvalidCastingAttempts()
        {
            // Arrange
            var objectWrappedInt = (object)42; // This should work
            var boxedInt = 42; // This should work
            var stringifiedInt = "42"; // This should NOT work

            // Act
            var result1 = _converter.Convert(objectWrappedInt, typeof(string), null, CultureInfo.InvariantCulture);
            var result2 = _converter.Convert(boxedInt, typeof(string), null, CultureInfo.InvariantCulture);
            var result3 = _converter.Convert(stringifiedInt, typeof(string), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal("42", result1); // object wrapping int should work
            Assert.Equal("42", result2); // direct int should work
            Assert.Equal("—", result3);  // string should fallback
        }

        [Fact]
        public void Convert_ProvidesConsistentFallbackFormat()
        {
            // Arrange
            var fallbackCausingValues = new object?[] { null, "text", 3.14, true };

            // Act
            var results = new string[fallbackCausingValues.Length];
            for (int i = 0; i < fallbackCausingValues.Length; i++)
            {
                var result = _converter.Convert(fallbackCausingValues[i], typeof(string), null, CultureInfo.InvariantCulture);
                results[i] = (string)result!;
            }

            // Assert
            foreach (var result in results)
            {
                Assert.Equal("—", result);
            }
            
            // All fallback results should be identical
            for (int i = 1; i < results.Length; i++)
            {
                Assert.Equal(results[0], results[i]);
            }
        }

        #endregion
    }
}