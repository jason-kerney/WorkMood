# InvertedBoolConverter - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview
**File**: `MauiApp/Converters/InvertedBoolConverter.cs`  
**Type**: MAUI Value Converter (IValueConverter)  
**LOC**: 25 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Simple value converter that inverts boolean values for UI data binding scenarios. Provides bidirectional boolean inversion for XAML binding contexts where the logical state needs to be reversed for UI presentation (e.g., showing content when `IsLoading` is false).

### Dependencies
- **System.Globalization.CultureInfo** - Culture-aware conversion support
- **Microsoft.Maui.Controls.IValueConverter** - MAUI value converter interface

### Key Responsibilities
1. **Forward conversion** - Inverts boolean values from source to target
2. **Backward conversion** - Inverts boolean values from target back to source
3. **Null value handling** - Returns false for non-boolean input values
4. **Type safety** - Handles type checking and casting safely

### Current Architecture Assessment
**Testability Score: 10/10** ✅ **EXCELLENT TESTABILITY**

**Design Strengths:**
1. **Pure functions** - No side effects, deterministic behavior
2. **Stateless design** - No instance variables or dependencies  
3. **Simple logic** - Clear boolean inversion with minimal complexity
4. **Consistent behavior** - Both Convert and ConvertBack use identical logic
5. **Null safety** - Graceful handling of non-boolean input values
6. **Interface compliance** - Properly implements IValueConverter contract

**No Design Issues** - This converter is already well-designed for testing with no refactoring required.

## XAML Usage Analysis

### Current Usage Pattern
Found in `Pages/Visualization.xaml`:
```xml
<converters:InvertedBoolConverter x:Key="InvertedBoolConverter"/>
<!-- Used to hide content while loading -->
<ScrollView IsVisible="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}">
```

### Typical Use Cases
- **Loading state management** - Hide content while loading (`IsVisible="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}"`)
- **Enabled/disabled toggle** - Enable controls when conditions are false
- **Visibility inversion** - Show elements when state is opposite of source boolean
- **Selection inversion** - Invert checkbox or toggle selection states

## Comprehensive Test Plan

### Test Structure
```
InvertedBoolConverterTests/
├── Convert/
│   ├── Should_ReturnFalse_WhenValueIsTrue()
│   ├── Should_ReturnTrue_WhenValueIsFalse()
│   ├── Should_ReturnFalse_WhenValueIsNull()
│   ├── Should_ReturnFalse_WhenValueIsNotBoolean()
│   ├── Should_ReturnFalse_WhenValueIsString()
│   ├── Should_ReturnFalse_WhenValueIsInteger()
│   ├── Should_ReturnFalse_WhenValueIsObject()
│   └── Should_IgnoreTargetTypeAndParameterAndCulture()
├── ConvertBack/
│   ├── Should_ReturnFalse_WhenValueIsTrue()
│   ├── Should_ReturnTrue_WhenValueIsFalse()
│   ├── Should_ReturnFalse_WhenValueIsNull()
│   ├── Should_ReturnFalse_WhenValueIsNotBoolean()
│   ├── Should_ReturnFalse_WhenValueIsString()
│   ├── Should_ReturnFalse_WhenValueIsInteger()
│   ├── Should_ReturnFalse_WhenValueIsObject()
│   └── Should_IgnoreTargetTypeAndParameterAndCulture()
├── ParameterHandling/
│   ├── Should_IgnoreParameter_InConvert()
│   ├── Should_IgnoreParameter_InConvertBack()
│   ├── Should_IgnoreTargetType_InConvert()
│   ├── Should_IgnoreTargetType_InConvertBack()
│   ├── Should_IgnoreCulture_InConvert()
│   └── Should_IgnoreCulture_InConvertBack()
├── TypeSafety/
│   ├── Should_HandleNullValueGracefully_InConvert()
│   ├── Should_HandleNullValueGracefully_InConvertBack()
│   ├── Should_HandleInvalidTypeGracefully_InConvert()
│   ├── Should_HandleInvalidTypeGracefully_InConvertBack()
│   ├── Should_NotThrowException_ForAnyInputType()
│   └── Should_AlwaysReturnBoolean_ForAnyInput()
└── BidirectionalConsistency/
    ├── Should_BeSymmetric_TrueToFalseToTrue()
    ├── Should_BeSymmetric_FalseToTrueToFalse()
    ├── Should_BeConsistent_WithMultipleConversions()
    └── Should_MaintainInversion_AcrossBothDirections()
```

### Test Implementation Examples

#### Convert Method Tests
```csharp
[Test]
public void Convert_Should_ReturnFalse_WhenValueIsTrue()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void Convert_Should_ReturnTrue_WhenValueIsFalse()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = false;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(true));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void Convert_Should_ReturnFalse_WhenValueIsNull()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    object? value = null;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void Convert_Should_ReturnFalse_WhenValueIsNotBoolean()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = "not a boolean";
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void Convert_Should_ReturnFalse_WhenValueIsInteger()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = 42;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void Convert_Should_IgnoreTargetTypeAndParameterAndCulture()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(string); // Different target type
    var parameter = "some parameter"; // Non-null parameter
    var culture = new CultureInfo("fr-FR"); // Different culture

    // Act
    var result = converter.Convert(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false)); // Still inverts the boolean
    Assert.That(result, Is.InstanceOf<bool>());
}
```

#### ConvertBack Method Tests
```csharp
[Test]
public void ConvertBack_Should_ReturnFalse_WhenValueIsTrue()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.ConvertBack(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void ConvertBack_Should_ReturnTrue_WhenValueIsFalse()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = false;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.ConvertBack(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(true));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void ConvertBack_Should_ReturnFalse_WhenValueIsNull()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    object? value = null;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.ConvertBack(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}

[Test]
public void ConvertBack_Should_ReturnFalse_WhenValueIsNotBoolean()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = "not a boolean";
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var result = converter.ConvertBack(value, targetType, parameter, culture);

    // Assert
    Assert.That(result, Is.EqualTo(false));
    Assert.That(result, Is.InstanceOf<bool>());
}
```

#### Parameter Handling Tests
```csharp
[Test]
public void Convert_Should_IgnoreParameter()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var culture = CultureInfo.InvariantCulture;

    // Act & Assert - Test with different parameter values
    var resultWithNull = converter.Convert(value, targetType, null, culture);
    var resultWithString = converter.Convert(value, targetType, "parameter", culture);
    var resultWithObject = converter.Convert(value, targetType, new object(), culture);

    Assert.That(resultWithNull, Is.EqualTo(false));
    Assert.That(resultWithString, Is.EqualTo(false));
    Assert.That(resultWithObject, Is.EqualTo(false));
}

[Test]
public void Convert_Should_IgnoreTargetType()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act & Assert - Test with different target types
    var resultWithBool = converter.Convert(value, typeof(bool), parameter, culture);
    var resultWithString = converter.Convert(value, typeof(string), parameter, culture);
    var resultWithObject = converter.Convert(value, typeof(object), parameter, culture);

    Assert.That(resultWithBool, Is.EqualTo(false));
    Assert.That(resultWithString, Is.EqualTo(false));
    Assert.That(resultWithObject, Is.EqualTo(false));
}

[Test]
public void Convert_Should_IgnoreCulture()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;

    // Act & Assert - Test with different cultures
    var resultWithInvariant = converter.Convert(value, targetType, parameter, CultureInfo.InvariantCulture);
    var resultWithEnUs = converter.Convert(value, targetType, parameter, new CultureInfo("en-US"));
    var resultWithFrFr = converter.Convert(value, targetType, parameter, new CultureInfo("fr-FR"));

    Assert.That(resultWithInvariant, Is.EqualTo(false));
    Assert.That(resultWithEnUs, Is.EqualTo(false));
    Assert.That(resultWithFrFr, Is.EqualTo(false));
}
```

#### Type Safety Tests
```csharp
[Test]
public void Convert_Should_NotThrowException_ForAnyInputType()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    var testValues = new object?[]
    {
        null,
        true,
        false,
        "string",
        42,
        3.14,
        new object(),
        new DateTime(),
        new List<int>(),
        new Dictionary<string, int>()
    };

    // Act & Assert
    foreach (var testValue in testValues)
    {
        Assert.DoesNotThrow(() => 
        {
            var result = converter.Convert(testValue, targetType, parameter, culture);
            Assert.That(result, Is.InstanceOf<bool>());
        }, $"Convert should not throw for input type: {testValue?.GetType().Name ?? "null"}");
    }
}

[Test]
public void ConvertBack_Should_NotThrowException_ForAnyInputType()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    var testValues = new object?[]
    {
        null,
        true,
        false,
        "string",
        42,
        3.14,
        new object(),
        new DateTime(),
        new List<int>(),
        new Dictionary<string, int>()
    };

    // Act & Assert
    foreach (var testValue in testValues)
    {
        Assert.DoesNotThrow(() => 
        {
            var result = converter.ConvertBack(testValue, targetType, parameter, culture);
            Assert.That(result, Is.InstanceOf<bool>());
        }, $"ConvertBack should not throw for input type: {testValue?.GetType().Name ?? "null"}");
    }
}

[Test]
public void Convert_Should_AlwaysReturnBoolean_ForAnyInput()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    var testValues = new object?[]
    {
        null, true, false, "string", 42, 3.14, new object()
    };

    // Act & Assert
    foreach (var testValue in testValues)
    {
        var result = converter.Convert(testValue, targetType, parameter, culture);
        Assert.That(result, Is.InstanceOf<bool>(), 
            $"Convert should always return bool for input: {testValue?.GetType().Name ?? "null"}");
    }
}
```

#### Bidirectional Consistency Tests
```csharp
[Test]
public void BidirectionalConsistency_Should_BeSymmetric_TrueToFalseToTrue()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var originalValue = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var convertedValue = converter.Convert(originalValue, targetType, parameter, culture);
    var backConvertedValue = converter.ConvertBack(convertedValue, targetType, parameter, culture);

    // Assert
    Assert.That(convertedValue, Is.EqualTo(false));
    Assert.That(backConvertedValue, Is.EqualTo(originalValue));
}

[Test]
public void BidirectionalConsistency_Should_BeSymmetric_FalseToTrueToFalse()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var originalValue = false;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act
    var convertedValue = converter.Convert(originalValue, targetType, parameter, culture);
    var backConvertedValue = converter.ConvertBack(convertedValue, targetType, parameter, culture);

    // Assert
    Assert.That(convertedValue, Is.EqualTo(true));
    Assert.That(backConvertedValue, Is.EqualTo(originalValue));
}

[Test]
public void BidirectionalConsistency_Should_BeConsistent_WithMultipleConversions()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var originalValue = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act - Multiple round trips
    var value = originalValue;
    for (int i = 0; i < 10; i++)
    {
        value = (bool)converter.Convert(value, targetType, parameter, culture);
        value = (bool)converter.ConvertBack(value, targetType, parameter, culture);
    }

    // Assert
    Assert.That(value, Is.EqualTo(originalValue));
}

[Test]
public void BidirectionalConsistency_Should_MaintainInversion_AcrossBothDirections()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var targetType = typeof(bool);
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act & Assert - Test both directions maintain inversion
    Assert.That(converter.Convert(true, targetType, parameter, culture), Is.EqualTo(false));
    Assert.That(converter.Convert(false, targetType, parameter, culture), Is.EqualTo(true));
    Assert.That(converter.ConvertBack(true, targetType, parameter, culture), Is.EqualTo(false));
    Assert.That(converter.ConvertBack(false, targetType, parameter, culture), Is.EqualTo(true));
}
```

#### Edge Case Tests
```csharp
[TestCase(typeof(bool))]
[TestCase(typeof(string))]
[TestCase(typeof(object))]
[TestCase(typeof(int))]
[TestCase(null)]
public void Convert_Should_HandleDifferentTargetTypes(Type? targetType)
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var parameter = (object?)null;
    var culture = CultureInfo.InvariantCulture;

    // Act & Assert
    Assert.DoesNotThrow(() => 
    {
        var result = converter.Convert(value, targetType!, parameter, culture);
        Assert.That(result, Is.EqualTo(false));
    });
}

[TestCase(null)]
[TestCase("")]
[TestCase("parameter")]
[TestCase(42)]
[TestCase(new object[] { "test" })]
public void Convert_Should_HandleDifferentParameters(object? parameter)
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var culture = CultureInfo.InvariantCulture;

    // Act & Assert
    Assert.DoesNotThrow(() => 
    {
        var result = converter.Convert(value, targetType, parameter, culture);
        Assert.That(result, Is.EqualTo(false));
    });
}

[Test]
public void Convert_Should_HandleNullCulture()
{
    // Arrange
    var converter = new InvertedBoolConverter();
    var value = true;
    var targetType = typeof(bool);
    var parameter = (object?)null;
    CultureInfo? culture = null;

    // Act & Assert
    Assert.DoesNotThrow(() => 
    {
        var result = converter.Convert(value, targetType, parameter, culture!);
        Assert.That(result, Is.EqualTo(false));
    });
}
```

### Test Fixtures Required
- **InvertedBoolConverterTestFixture** - Standard test fixture with converter instance
- **ParameterTestDataGenerator** - Generate various parameter test cases
- **TypeSafetyTestDataGenerator** - Generate various input type test cases
- **CultureTestDataGenerator** - Generate various culture test cases

## Success Criteria
- [ ] **Convert method validation** - All boolean and non-boolean input scenarios tested
- [ ] **ConvertBack method validation** - All boolean and non-boolean input scenarios tested  
- [ ] **Parameter independence** - Conversion behavior independent of parameters, target type, culture
- [ ] **Type safety** - Graceful handling of all input types without exceptions
- [ ] **Bidirectional consistency** - Symmetric conversion behavior verified
- [ ] **Edge case handling** - Null values, invalid types, unusual parameters handled correctly

## Implementation Priority
**MEDIUM PRIORITY** - Simple converter with excellent testability. Good foundation for establishing converter testing patterns before tackling more complex converters.

## Dependencies for Testing
- **NUnit** - Standard testing framework
- **System.Globalization** - CultureInfo testing scenarios
- **MAUI Test Framework** - Value converter testing utilities

## Implementation Estimate
**Effort: Low (1 day)**
- Straightforward testing with no refactoring required
- Comprehensive test coverage for all scenarios and edge cases
- Good template for other converter testing approaches
- Bidirectional consistency testing patterns

This converter represents the simplest pattern in the Converters tier and provides an excellent foundation for establishing testing methodologies before moving to more complex converters with business logic.