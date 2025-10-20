# NullableMoodConverter - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview

**File**: `MauiApp/Converters/MoodConverters.cs`  
**Type**: MAUI Value Converter (IValueConverter)  
**LOC**: 17 lines  
**Current Coverage**: 0% (estimated)

### Purpose

Specialized converter for mood value formatting that transforms integer mood values into string representations while providing a fallback to an em dash ("—") for null or non-integer values. Designed for UI display where mood values must be consistently formatted and empty states clearly indicated.

### Dependencies

- **System.Globalization.CultureInfo** - Culture-aware conversion support
- **Microsoft.Maui.Controls.IValueConverter** - MAUI value converter interface

### Key Responsibilities

1. **Integer mood conversion** - Transforms int values to string representation
2. **Null handling** - Returns em dash ("—") for null input values
3. **Type validation** - Returns em dash ("—") for non-integer input types
4. **Consistent formatting** - Ensures uniform mood value display in UI
5. **Fallback behavior** - Graceful degradation for invalid inputs

### Current Architecture Assessment

**Testability Score: 9/10** ✅ **EXCELLENT TESTABILITY**

**Design Strengths:**

1. **Clear conversion logic** - Simple int-to-string with fallback pattern
2. **Consistent fallback** - Single fallback value ("—") for all invalid cases
3. **Type-safe approach** - Explicit type checking before conversion
4. **Stateless design** - No instance variables or dependencies
5. **Deterministic behavior** - Predictable output for all input types
6. **Interface compliance** - Proper IValueConverter implementation

**Minor Design Consideration:**

1. **ConvertBack not implemented** - Throws NotImplementedException (acceptable for display converter)

**No Design Issues** - This converter represents excellent implementation for specialized mood formatting.

## Usage Scenarios Analysis

### Typical UI Binding Patterns

- **Mood display**: `Text="{Binding MoodValue, Converter={StaticResource NullableMoodConverter}}"`
- **List formatting**: `Text="{Binding SelectedMood, Converter={StaticResource NullableMoodConverter}}"`
- **Label content**: `Text="{Binding CurrentMood, Converter={StaticResource NullableMoodConverter}}"`
- **Summary views**: `Text="{Binding AverageMood, Converter={StaticResource NullableMoodConverter}}"`

### Business Logic Applications

- **Mood value presentation** - Format mood integers for user display
- **Empty state indication** - Show em dash when no mood selected
- **Data validation feedback** - Indicate invalid mood data gracefully
- **Consistent formatting** - Ensure uniform mood representation across UI

## Comprehensive Test Plan

### Test Structure

```
NullableMoodConverterTests/
├── Convert_WithIntegerValues/
│   ├── Should_ReturnStringRepresentation_WhenValueIsPositiveInteger()
│   ├── Should_ReturnStringRepresentation_WhenValueIsNegativeInteger()
│   ├── Should_ReturnStringRepresentation_WhenValueIsZero()
│   ├── Should_ReturnStringRepresentation_WhenValueIsMaxInteger()
│   ├── Should_ReturnStringRepresentation_WhenValueIsMinInteger()
│   └── Should_HandleLargeIntegers_Correctly()
├── Convert_WithNullValues/
│   ├── Should_ReturnEmDash_WhenValueIsNull()
│   ├── Should_ReturnEmDash_WhenValueIsExplicitNull()
│   └── Should_HandleNullReference_Consistently()
├── Convert_WithNonIntegerTypes/
│   ├── Should_ReturnEmDash_WhenValueIsString()
│   ├── Should_ReturnEmDash_WhenValueIsDouble()
│   ├── Should_ReturnEmDash_WhenValueIsBoolean()
│   ├── Should_ReturnEmDash_WhenValueIsObject()
│   ├── Should_ReturnEmDash_WhenValueIsDateTime()
│   ├── Should_ReturnEmDash_WhenValueIsCollection()
│   └── Should_ReturnEmDash_WhenValueIsComplexType()
├── Convert_WithNullableIntegers/
│   ├── Should_ReturnStringRepresentation_WhenNullableIntHasValue()
│   ├── Should_ReturnEmDash_WhenNullableIntIsNull()
│   └── Should_HandleNullableIntegerEdgeCases()
├── Convert_WithSpecialValues/
│   ├── Should_ReturnEmDash_WhenValueIsStringInteger()
│   ├── Should_ReturnEmDash_WhenValueIsFloatingPoint()
│   ├── Should_ReturnEmDash_WhenValueIsNumericString()
│   └── Should_ReturnEmDash_WhenValueIsEmptyString()
├── ConvertBack/
│   ├── Should_ThrowNotImplementedException_Always()
│   ├── Should_ThrowForAnyStringInput()
│   └── Should_ProvideConsistentException()
├── ParameterHandling/
│   ├── Should_IgnoreParameter_ForIntegerValue()
│   ├── Should_IgnoreParameter_ForNullValue()
│   ├── Should_IgnoreTargetType_InAllScenarios()
│   ├── Should_IgnoreCulture_InAllScenarios()
│   └── Should_HandleNullParameters_Gracefully()
├── OutputConsistency/
│   ├── Should_AlwaysReturnString()
│   ├── Should_ReturnConsistentString_ForSameInteger()
│   ├── Should_ReturnConsistentEmDash_ForNonIntegers()
│   └── Should_MaintainDeterministicBehavior()
└── FallbackBehavior/
    ├── Should_UseEmDashAsUniversalFallback()
    ├── Should_HandleTypeConversionFailures()
    ├── Should_HandleInvalidCastingAttempts()
    └── Should_ProvideConsistentFallbackFormat()
```

### Test Implementation Examples

#### Integer Values Tests

```csharp
[Test]
public void Convert_Should_ReturnStringRepresentation_WhenValueIsPositiveInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var positiveIntegers = new[] { 1, 5, 10, 42, 100, 999 };

    foreach (var value in positiveIntegers)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(value.ToString()));
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnStringRepresentation_WhenValueIsNegativeInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var negativeIntegers = new[] { -1, -5, -10, -42, -100, -999 };

    foreach (var value in negativeIntegers)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(value.ToString()));
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnStringRepresentation_WhenValueIsZero()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var value = 0;

    // Act
    var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo("0"));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_ReturnStringRepresentation_WhenValueIsMaxInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var value = int.MaxValue;

    // Act
    var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(int.MaxValue.ToString()));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_ReturnStringRepresentation_WhenValueIsMinInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var value = int.MinValue;

    // Act
    var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(int.MinValue.ToString()));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_HandleLargeIntegers_Correctly()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var largeIntegers = new[] 
    { 
        1000000, 
        -1000000, 
        2147483647,  // int.MaxValue
        -2147483648  // int.MinValue
    };

    foreach (var value in largeIntegers)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(value.ToString()));
        Assert.That(result, Is.InstanceOf<string>());
    }
}
```

#### Null Values Tests

```csharp
[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsNull()
{
    // Arrange
    var converter = new NullableMoodConverter();
    object? value = null;

    // Act
    var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo("—"));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsExplicitNull()
{
    // Arrange
    var converter = new NullableMoodConverter();

    // Act
    var result = converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo("—"));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_HandleNullReference_Consistently()
{
    // Arrange
    var converter = new NullableMoodConverter();
    object? nullReference = null;
    string? nullString = null;
    int? nullInt = null;
    
    var nullValues = new object?[] { nullReference, nullString, nullInt, null };

    foreach (var nullValue in nullValues)
    {
        // Act
        var result = converter.Convert(nullValue, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"));
        Assert.That(result, Is.InstanceOf<string>());
    }
}
```

#### Non-Integer Types Tests

```csharp
[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsString()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var stringValues = new[] 
    { 
        "test", 
        "5", 
        "42", 
        "-10", 
        "abc", 
        "", 
        " ", 
        "null" 
    };

    foreach (var value in stringValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"String value '{value}' should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsDouble()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var doubleValues = new[] { 1.0, 5.5, -3.14, 0.0, double.MaxValue, double.MinValue };

    foreach (var value in doubleValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Double value {value} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsBoolean()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var booleanValues = new[] { true, false };

    foreach (var value in booleanValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Boolean value {value} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsObject()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var objectValues = new object[]
    {
        new object(),
        new List<int>(),
        new Dictionary<string, int>(),
        new Exception("test"),
        new { Name = "Test", Value = 42 }
    };

    foreach (var value in objectValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Object value {value.GetType().Name} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsDateTime()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var dateTimeValues = new[]
    {
        DateTime.Now,
        DateTime.Today,
        DateTime.MinValue,
        DateTime.MaxValue,
        new DateTime(2023, 1, 1)
    };

    foreach (var value in dateTimeValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"DateTime value {value} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsCollection()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var collectionValues = new object[]
    {
        new List<int>(),                    // Empty list
        new List<int> { 1, 2, 3 },         // Non-empty list
        new int[0],                        // Empty array
        new[] { 1, 2, 3 },                 // Non-empty array
        new Dictionary<string, int>()       // Empty dictionary
    };

    foreach (var value in collectionValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Collection value {value.GetType().Name} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsComplexType()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var complexTypes = new object[]
    {
        new Uri("https://example.com"),
        new TimeSpan(1, 2, 3),
        new Guid(),
        CultureInfo.InvariantCulture
    };

    foreach (var value in complexTypes)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Complex type {value.GetType().Name} should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}
```

#### Nullable Integers Tests

```csharp
[Test]
public void Convert_Should_ReturnStringRepresentation_WhenNullableIntHasValue()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var nullableIntsWithValues = new int?[] { 1, 5, -3, 0, 42, int.MaxValue, int.MinValue };

    foreach (var value in nullableIntsWithValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(value.ToString()));
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenNullableIntIsNull()
{
    // Arrange
    var converter = new NullableMoodConverter();
    int? nullableInt = null;

    // Act
    var result = converter.Convert(nullableInt, typeof(string), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo("—"));
    Assert.That(result, Is.InstanceOf<string>());
}

[Test]
public void Convert_Should_HandleNullableIntegerEdgeCases()
{
    // Arrange
    var converter = new NullableMoodConverter();
    
    // Edge cases for nullable integers
    int? defaultNullableInt = new int?();    // Default value (null)
    int? explicitNullInt = null;             // Explicit null
    int? zeroInt = 0;                       // Zero value
    int? negativeInt = -1;                  // Negative value

    // Act & Assert
    var defaultResult = converter.Convert(defaultNullableInt, typeof(string), null, CultureInfo.InvariantCulture);
    Assert.That(defaultResult, Is.EqualTo("—"));

    var explicitResult = converter.Convert(explicitNullInt, typeof(string), null, CultureInfo.InvariantCulture);
    Assert.That(explicitResult, Is.EqualTo("—"));

    var zeroResult = converter.Convert(zeroInt, typeof(string), null, CultureInfo.InvariantCulture);
    Assert.That(zeroResult, Is.EqualTo("0"));

    var negativeResult = converter.Convert(negativeInt, typeof(string), null, CultureInfo.InvariantCulture);
    Assert.That(negativeResult, Is.EqualTo("-1"));
}
```

#### Special Values Tests

```csharp
[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsStringInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var stringIntegers = new[] { "1", "42", "-5", "0", "999" };

    foreach (var value in stringIntegers)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"String integer '{value}' should return em dash (not converted)");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsFloatingPoint()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var floatingPointValues = new object[] 
    { 
        1.0f,     // float
        1.0,      // double  
        1.0m,     // decimal
        5.5f,
        -3.14,
        0.0m
    };

    foreach (var value in floatingPointValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Floating point value {value} ({value.GetType().Name}) should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsNumericString()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var numericStrings = new[] 
    { 
        "1.5", 
        "3.14", 
        "-2.7", 
        "42.0", 
        "0.0",
        "1e10",
        "NaN",
        "Infinity",
        "-Infinity"
    };

    foreach (var value in numericStrings)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Numeric string '{value}' should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void Convert_Should_ReturnEmDash_WhenValueIsEmptyString()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var emptyStrings = new[] { "", " ", "\t", "\n", "\r\n" };

    foreach (var value in emptyStrings)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Empty/whitespace string '{value}' should return em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}
```

#### ConvertBack Tests

```csharp
[Test]
public void ConvertBack_Should_ThrowNotImplementedException_Always()
{
    // Arrange
    var converter = new NullableMoodConverter();

    // Act & Assert
    Assert.Throws<NotImplementedException>(() => 
        converter.ConvertBack("42", typeof(int), null, CultureInfo.InvariantCulture));
}

[Test]
public void ConvertBack_Should_ThrowForAnyStringInput()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var stringInputs = new[] { "1", "42", "—", "", "test", "null" };

    foreach (var input in stringInputs)
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack(input, typeof(int), null, CultureInfo.InvariantCulture),
            $"ConvertBack should throw NotImplementedException for string input: '{input}'");
    }
}

[Test]
public void ConvertBack_Should_ProvideConsistentException()
{
    // Arrange
    var converter = new NullableMoodConverter();

    // Act & Assert
    var exception = Assert.Throws<NotImplementedException>(() => 
        converter.ConvertBack("42", typeof(int), null, CultureInfo.InvariantCulture));
    
    Assert.That(exception.Message, Is.Not.Null);
    Assert.That(exception.Message, Is.Not.Empty);
}
```

#### Parameter Handling Tests

```csharp
[Test]
public void ParameterHandling_Should_IgnoreParameter_ForIntegerValue()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var value = 42;
    var parameters = new object?[] { null, "parameter", 123, new object() };

    foreach (var parameter in parameters)
    {
        // Act
        var result = converter.Convert(value, typeof(string), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("42"), 
            $"Parameter {parameter} should be ignored for integer value");
    }
}

[Test]
public void ParameterHandling_Should_IgnoreParameter_ForNullValue()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var parameters = new object?[] { null, "parameter", 123, new object() };

    foreach (var parameter in parameters)
    {
        // Act
        var result = converter.Convert(null, typeof(string), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Parameter {parameter} should be ignored for null value");
    }
}

[Test]
public void ParameterHandling_Should_IgnoreTargetType_InAllScenarios()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var targetTypes = new[] { typeof(string), typeof(object), typeof(int), typeof(bool) };

    foreach (var targetType in targetTypes)
    {
        // Act
        var resultForInt = converter.Convert(42, targetType, null, CultureInfo.InvariantCulture);
        var resultForNull = converter.Convert(null, targetType, null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(resultForInt, Is.EqualTo("42"), 
            $"Target type {targetType.Name} should be ignored for integer");
        Assert.That(resultForNull, Is.EqualTo("—"), 
            $"Target type {targetType.Name} should be ignored for null");
    }
}

[Test]
public void ParameterHandling_Should_IgnoreCulture_InAllScenarios()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var cultures = new[]
    {
        CultureInfo.InvariantCulture,
        new CultureInfo("en-US"),
        new CultureInfo("fr-FR"),
        new CultureInfo("ja-JP")
    };

    foreach (var culture in cultures)
    {
        // Act
        var resultForInt = converter.Convert(42, typeof(string), null, culture);
        var resultForNull = converter.Convert(null, typeof(string), null, culture);

        // Assert
        Assert.That(resultForInt, Is.EqualTo("42"), 
            $"Culture {culture.Name} should be ignored for integer");
        Assert.That(resultForNull, Is.EqualTo("—"), 
            $"Culture {culture.Name} should be ignored for null");
    }
}

[Test]
public void ParameterHandling_Should_HandleNullParameters_Gracefully()
{
    // Arrange
    var converter = new NullableMoodConverter();

    // Act & Assert
    Assert.DoesNotThrow(() => 
    {
        var resultForInt = converter.Convert(42, null!, null, null!);
        var resultForNull = converter.Convert(null, null!, null, null!);
        
        Assert.That(resultForInt, Is.EqualTo("42"));
        Assert.That(resultForNull, Is.EqualTo("—"));
    });
}
```

#### Output Consistency Tests

```csharp
[Test]
public void OutputConsistency_Should_AlwaysReturnString()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var testValues = new object?[]
    {
        null, 42, "test", true, 3.14, new object()
    };

    foreach (var value in testValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.InstanceOf<string>(), 
            $"Result should always be string for input: {value?.GetType().Name ?? "null"}");
    }
}

[Test]
public void OutputConsistency_Should_ReturnConsistentString_ForSameInteger()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var value = 42;

    // Act - Multiple conversions of same value
    var results = new List<object?>();
    for (int i = 0; i < 10; i++)
    {
        results.Add(converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture));
    }

    // Assert
    Assert.That(results.All(r => r!.Equals("42")), Is.True);
    Assert.That(results.All(r => r is string), Is.True);
}

[Test]
public void OutputConsistency_Should_ReturnConsistentEmDash_ForNonIntegers()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var nonIntegerValues = new object?[] { null, "test", 3.14, true };

    foreach (var value in nonIntegerValues)
    {
        // Act - Multiple conversions of same value
        var results = new List<object?>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture));
        }

        // Assert
        Assert.That(results.All(r => r!.Equals("—")), Is.True, 
            $"Non-integer value {value?.GetType().Name ?? "null"} should consistently return em dash");
        Assert.That(results.All(r => r is string), Is.True);
    }
}

[Test]
public void OutputConsistency_Should_MaintainDeterministicBehavior()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var testCases = new object?[]
    {
        null, 42, -5, 0, "test", 3.14, true
    };

    foreach (var testCase in testCases)
    {
        // Act - Multiple conversions should be identical
        var firstResult = converter.Convert(testCase, typeof(string), null, CultureInfo.InvariantCulture);
        var secondResult = converter.Convert(testCase, typeof(string), null, CultureInfo.InvariantCulture);
        var thirdResult = converter.Convert(testCase, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(secondResult, Is.EqualTo(firstResult), 
            $"Results should be deterministic for input: {testCase?.GetType().Name ?? "null"}");
        Assert.That(thirdResult, Is.EqualTo(firstResult), 
            $"Results should be deterministic for input: {testCase?.GetType().Name ?? "null"}");
    }
}
```

#### Fallback Behavior Tests

```csharp
[Test]
public void FallbackBehavior_Should_UseEmDashAsUniversalFallback()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var nonIntegerValues = new object?[]
    {
        null, "test", 3.14, true, false, new object(), 
        new List<int>(), DateTime.Now, 'A', 1.5f, 1.5m
    };

    foreach (var value in nonIntegerValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Non-integer value {value?.GetType().Name ?? "null"} should use em dash fallback");
    }
}

[Test]
public void FallbackBehavior_Should_HandleTypeConversionFailures()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var unconvertibleValues = new object[]
    {
        new Exception("test"),
        new Uri("https://example.com"),
        new { Anonymous = "object" },
        CultureInfo.InvariantCulture
    };

    foreach (var value in unconvertibleValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Unconvertible value {value.GetType().Name} should gracefully fall back to em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void FallbackBehavior_Should_HandleInvalidCastingAttempts()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var invalidCastValues = new object[]
    {
        "42",         // String that looks like int but isn't int type
        42.0,         // Double that has int value but wrong type
        42L,          // Long that has int value but wrong type
        (short)42,    // Short that has int value but wrong type
        (byte)42      // Byte that has int value but wrong type
    };

    foreach (var value in invalidCastValues)
    {
        // Act
        var result = converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo("—"), 
            $"Invalid cast value {value} ({value.GetType().Name}) should fall back to em dash");
        Assert.That(result, Is.InstanceOf<string>());
    }
}

[Test]
public void FallbackBehavior_Should_ProvideConsistentFallbackFormat()
{
    // Arrange
    var converter = new NullableMoodConverter();
    var fallbackValues = new object?[] { null, "test", 3.14, true, new object() };

    // Act
    var fallbackResults = fallbackValues
        .Select(v => converter.Convert(v, typeof(string), null, CultureInfo.InvariantCulture))
        .ToList();

    // Assert
    Assert.That(fallbackResults.All(r => r!.Equals("—")), Is.True);
    Assert.That(fallbackResults.All(r => r is string), Is.True);
    Assert.That(fallbackResults.Distinct().Count(), Is.EqualTo(1), 
        "All fallback results should be identical");
}
```

### Test Fixtures Required

- **NullableMoodConverterTestFixture** - Standard test fixture with converter instance
- **IntegerTestDataGenerator** - Generate comprehensive integer test cases
- **NonIntegerTestDataGenerator** - Generate various non-integer type test cases
- **NullableTypeTestDataGenerator** - Generate nullable type scenarios

## Success Criteria

- [ ] **Integer conversion** - All int types converted to string representation
- [ ] **Null handling** - All null inputs return em dash ("—")
- [ ] **Type validation** - All non-integer types return em dash fallback
- [ ] **Nullable integer support** - Nullable int? handled correctly (value → string, null → em dash)
- [ ] **Parameter independence** - Conversion ignores parameters, target type, culture
- [ ] **ConvertBack behavior** - NotImplementedException thrown consistently
- [ ] **Output consistency** - Always returns string, deterministic behavior
- [ ] **Fallback consistency** - Universal em dash fallback for all invalid inputs

## Implementation Priority

**MEDIUM PRIORITY** - Specialized mood formatting converter with excellent testability and clear business logic.

## Dependencies for Testing

- **NUnit** - Standard testing framework
- **System.Globalization** - CultureInfo for parameter testing
- **Various .NET types** - For comprehensive type validation testing

## Implementation Estimate

**Effort: Low (0.5 days)**

- Straightforward integer vs non-integer testing pattern
- Clear fallback behavior testing
- Excellent foundation for specialized converter testing
- Good template for type-specific converter patterns

This converter demonstrates testing approaches for type-specific conversion with universal fallback, making it essential for specialized data formatting patterns.