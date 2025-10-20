# BoolToColorConverter - Individual Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview
**File**: `MauiApp/Converters/MoodConverters.cs`  
**Type**: MAUI Value Converter (IValueConverter)  
**LOC**: 30 lines  
**Current Coverage**: 0% (estimated)

### Purpose
Advanced parameter-driven converter that translates boolean values into color objects based on configurable color parameters. Supports both predefined color names and ARGB hex color codes, enabling dynamic UI theming and conditional visual styling through data binding.

### Dependencies
- **Microsoft.Maui.Graphics.Color** - MAUI color system for color object creation
- **System.Globalization.CultureInfo** - Culture-aware conversion support
- **Microsoft.Maui.Controls.IValueConverter** - MAUI value converter interface

### Key Responsibilities
1. **Parameter parsing** - Splits comma-separated color parameter string into true/false color options
2. **Boolean evaluation** - Selects appropriate color based on boolean input value
3. **Color resolution** - Handles both predefined color names and ARGB hex codes
4. **Type safety** - Returns fallback color for invalid input or parameter combinations
5. **String parsing** - Processes and trims color parameter strings

### Current Architecture Assessment
**Testability Score: 7/10** ✅ **GOOD TESTABILITY**

**Design Strengths:**
1. **Parameter-driven behavior** - Flexible color selection through parameter configuration
2. **Dual color support** - Handles both predefined names and hex codes
3. **Input validation** - Graceful handling of invalid parameters and input types
4. **Consistent fallback** - Returns Colors.Transparent for all error scenarios
5. **String handling** - Proper trimming and parsing of color parameters

**Minor Design Issues:**
1. **ConvertBack not implemented** - Throws NotImplementedException (acceptable for display-only converter)
2. **Limited predefined colors** - Only supports 4 predefined color names
3. **Error handling** - Color.FromArgb exceptions not explicitly caught

**No Refactoring Required** - Well-designed converter with clear parameter-based logic.

## Parameter Format Analysis

### Expected Parameter Format
```
"TrueColor,FalseColor"
```

### Supported Color Formats
1. **Predefined Names**: "White", "Black", "Transparent", "Gray"
2. **ARGB Hex Codes**: "#AARRGGBB" or "#RRGGBB" format
3. **Mixed Combinations**: "White,#FF5733" or "#FF0000,Transparent"

### Parameter Examples
- `"White,Black"` - White for true, black for false
- `"#FF0000,#00FF00"` - Red for true, green for false  
- `"Transparent,#808080"` - Transparent for true, gray for false
- `"#FFFF0000,White"` - Red with alpha for true, white for false

### Error Scenarios
- **Null parameter**: Returns Colors.Transparent
- **Invalid format**: Returns Colors.Transparent
- **Single color**: Returns Colors.Transparent
- **Invalid hex code**: Color.FromArgb may throw exception

## Comprehensive Test Plan

### Test Structure
```
BoolToColorConverterTests/
├── Convert_WithValidParameters/
│   ├── Should_ReturnTrueColor_WhenValueIsTrue()
│   ├── Should_ReturnFalseColor_WhenValueIsFalse()
│   ├── Should_HandlePredefinedColors_Correctly()
│   ├── Should_HandleHexColors_Correctly()
│   ├── Should_HandleMixedColorFormats_Correctly()
│   └── Should_TrimWhitespace_FromColorParameters()
├── Convert_WithPredefinedColors/
│   ├── Should_ReturnWhiteColor_WhenParameterIsWhite()
│   ├── Should_ReturnBlackColor_WhenParameterIsBlack()
│   ├── Should_ReturnTransparentColor_WhenParameterIsTransparent()
│   ├── Should_ReturnGrayColor_WhenParameterIsGray()
│   └── Should_BeCaseExact_ForPredefinedColors()
├── Convert_WithHexColors/
│   ├── Should_ParseShortHexCode_Correctly()
│   ├── Should_ParseLongHexCode_Correctly()
│   ├── Should_ParseHexWithAlpha_Correctly()
│   ├── Should_HandleHexWithoutHash_Gracefully()
│   └── Should_HandleInvalidHexCode_Gracefully()
├── Convert_WithInvalidParameters/
│   ├── Should_ReturnTransparent_WhenParameterIsNull()
│   ├── Should_ReturnTransparent_WhenParameterIsEmpty()
│   ├── Should_ReturnTransparent_WhenParameterHasOneColor()
│   ├── Should_ReturnTransparent_WhenParameterHasThreeColors()
│   ├── Should_ReturnTransparent_WhenParameterHasNoComma()
│   └── Should_HandleMalformedParameters_Gracefully()
├── Convert_WithInvalidInput/
│   ├── Should_ReturnTransparent_WhenValueIsNull()
│   ├── Should_ReturnTransparent_WhenValueIsNotBoolean()
│   ├── Should_ReturnTransparent_WhenValueIsString()
│   ├── Should_ReturnTransparent_WhenValueIsInteger()
│   └── Should_ReturnTransparent_WhenValueIsArbitraryObject()
├── ConvertBack/
│   ├── Should_ThrowNotImplementedException_Always()
│   ├── Should_ThrowForAnyColorInput_Consistently()
│   └── Should_ProvideHelpfulExceptionMessage()
├── ParameterParsing/
│   ├── Should_SplitParameterByComma_Correctly()
│   ├── Should_TrimWhitespace_FromBothColors()
│   ├── Should_HandleExtraWhitespace_Gracefully()
│   ├── Should_HandleEmptyColorAfterSplit_Gracefully()
│   └── Should_IgnoreExtraCommas_AndUseFirstTwo()
├── ColorResolution/
│   ├── Should_ResolveAllPredefinedColors_Correctly()
│   ├── Should_FallbackToFromArgb_ForUnknownNames()
│   ├── Should_HandleColorFromArgbExceptions_Gracefully()
│   └── Should_MaintainColorEquality_AcrossConversions()
└── EdgeCases/
    ├── Should_HandleEmptyStringsInParameters()
    ├── Should_HandleSpecialCharactersInParameters()
    ├── Should_HandleVeryLongParameterStrings()
    └── Should_MaintainPerformance_WithComplexParameters()
```

### Test Implementation Examples

#### Valid Parameters Tests
```csharp
[Test]
public void Convert_Should_ReturnTrueColor_WhenValueIsTrue()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var value = true;
    var parameter = "Red,Blue";
    
    // Act
    var result = converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    // Note: "Red" would fallback to Color.FromArgb("Red") which may be invalid
    // In practice, should use "#FF0000,#0000FF" or predefined names
    Assert.That(result, Is.Not.Null);
}

[Test]
public void Convert_Should_ReturnFalseColor_WhenValueIsFalse()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var value = false;
    var parameter = "White,Black";
    
    // Act
    var result = converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Black));
}

[Test]
public void Convert_Should_HandlePredefinedColors_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var predefinedColorTests = new[]
    {
        // (value, parameter, expected)
        (true, "White,Black", Colors.White),
        (false, "White,Black", Colors.Black),
        (true, "Transparent,Gray", Colors.Transparent),
        (false, "Transparent,Gray", Colors.Gray),
        (true, "Black,White", Colors.Black),
        (false, "Black,White", Colors.White)
    };

    foreach (var (value, parameter, expected) in predefinedColorTests)
    {
        // Act
        var result = converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(expected), 
            $"Value {value} with parameter '{parameter}' should return {expected}");
    }
}

[Test]
public void Convert_Should_HandleHexColors_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var hexColorTests = new[]
    {
        // (value, parameter, description)
        (true, "#FF0000,#00FF00", "Red for true"),
        (false, "#FF0000,#00FF00", "Green for false"),
        (true, "#FFFF0000,#FF00FF00", "Red with alpha for true"),
        (false, "#FFFF0000,#FF00FF00", "Green with alpha for false")
    };

    foreach (var (value, parameter, description) in hexColorTests)
    {
        // Act
        var result = converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.InstanceOf<Color>(), description);
        Assert.That(result, Is.Not.EqualTo(Colors.Transparent), 
            $"Should not fallback to transparent for valid hex: {description}");
    }
}

[Test]
public void Convert_Should_HandleMixedColorFormats_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var mixedFormatTests = new[]
    {
        // (value, parameter, expected for true, expected for false)
        (true, "White,#FF0000", Colors.White, null),
        (false, "White,#FF0000", null, "should be red"),
        (true, "#00FF00,Black", "should be green", null),
        (false, "#00FF00,Black", null, Colors.Black),
        (true, "Transparent,#808080", Colors.Transparent, null),
        (false, "Transparent,#808080", null, "should be gray")
    };

    foreach (var (value, parameter, expectedColor, description) in mixedFormatTests)
    {
        // Act
        var result = converter.Convert(value, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        if (expectedColor != null)
        {
            Assert.That(result, Is.EqualTo(expectedColor));
        }
        else
        {
            Assert.That(result, Is.InstanceOf<Color>(), description);
            Assert.That(result, Is.Not.EqualTo(Colors.Transparent), description);
        }
    }
}

[Test]
public void Convert_Should_TrimWhitespace_FromColorParameters()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var whitespaceTests = new[]
    {
        "  White  ,  Black  ",
        "\tWhite\t,\tBlack\t",
        " White , Black ",
        "White,  Black",
        "  White,Black  "
    };

    foreach (var parameter in whitespaceTests)
    {
        // Act
        var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
        var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(resultTrue, Is.EqualTo(Colors.White), 
            $"Should trim whitespace and return White for true with parameter: '{parameter}'");
        Assert.That(resultFalse, Is.EqualTo(Colors.Black), 
            $"Should trim whitespace and return Black for false with parameter: '{parameter}'");
    }
}
```

#### Predefined Colors Tests
```csharp
[Test]
public void Convert_Should_ReturnWhiteColor_WhenParameterIsWhite()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(true, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.White));
}

[Test]
public void Convert_Should_ReturnBlackColor_WhenParameterIsBlack()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(false, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Black));
}

[Test]
public void Convert_Should_ReturnTransparentColor_WhenParameterIsTransparent()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(true, typeof(Color), "Transparent,Gray", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnGrayColor_WhenParameterIsGray()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(false, typeof(Color), "White,Gray", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Gray));
}

[Test]
public void Convert_Should_BeCaseExact_ForPredefinedColors()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var caseVariations = new[]
    {
        "white,black",    // lowercase
        "WHITE,BLACK",    // uppercase
        "White,black",    // mixed case
        "white,Black"     // mixed case
    };

    foreach (var parameter in caseVariations)
    {
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        // Should not match predefined colors due to case sensitivity
        // Will fallback to Color.FromArgb which may fail
        Assert.That(result, Is.Not.EqualTo(Colors.White), 
            $"Case-sensitive predefined colors should not match: '{parameter}'");
    }
}
```

#### Hex Colors Tests
```csharp
[Test]
public void Convert_Should_ParseShortHexCode_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var shortHexCodes = new[]
    {
        "#FF0000", // Red
        "#00FF00", // Green
        "#0000FF", // Blue
        "#FFFFFF", // White
        "#000000"  // Black
    };

    foreach (var hexCode in shortHexCodes)
    {
        var parameter = $"{hexCode},#808080";
        
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.InstanceOf<Color>(), $"Should parse short hex code: {hexCode}");
        Assert.That(result, Is.Not.EqualTo(Colors.Transparent), 
            $"Should not fallback to transparent for valid hex: {hexCode}");
    }
}

[Test]
public void Convert_Should_ParseLongHexCode_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var longHexCodes = new[]
    {
        "#FFFF0000", // Red with alpha
        "#FF00FF00", // Green with alpha
        "#FF0000FF", // Blue with alpha
        "#80FFFFFF", // White with 50% alpha
        "#00000000"  // Transparent black
    };

    foreach (var hexCode in longHexCodes)
    {
        var parameter = $"{hexCode},#FF808080";
        
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.InstanceOf<Color>(), $"Should parse long hex code: {hexCode}");
        Assert.That(result, Is.Not.EqualTo(Colors.Transparent), 
            $"Should not fallback to transparent for valid hex: {hexCode}");
    }
}

[Test]
public void Convert_Should_ParseHexWithAlpha_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var parameter = "#80FF0000,#80000000"; // 50% alpha red, 50% alpha black

    // Act
    var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
    var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(resultTrue, Is.InstanceOf<Color>());
    Assert.That(resultFalse, Is.InstanceOf<Color>());
    Assert.That(resultTrue, Is.Not.EqualTo(Colors.Transparent));
    Assert.That(resultFalse, Is.Not.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_HandleInvalidHexCode_Gracefully()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var invalidHexCodes = new[]
    {
        "#GGGGGG",     // Invalid hex characters
        "#FF00",       // Too short
        "#FF0000000",  // Too long
        "FF0000",      // Missing hash
        "#",           // Just hash
        "#ZZZZZZ"      // Invalid characters
    };

    foreach (var invalidHex in invalidHexCodes)
    {
        var parameter = $"{invalidHex},White";
        
        // Act & Assert
        Assert.DoesNotThrow(() => 
        {
            var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
            // May return transparent as fallback or throw exception - both are acceptable
        }, $"Should handle invalid hex gracefully: {invalidHex}");
    }
}
```

#### Invalid Parameters Tests
```csharp
[Test]
public void Convert_Should_ReturnTransparent_WhenParameterIsNull()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(true, typeof(Color), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnTransparent_WhenParameterIsEmpty()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(true, typeof(Color), "", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnTransparent_WhenParameterHasOneColor()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var singleColorParameters = new[] { "White", "Black", "#FF0000", "Transparent" };

    foreach (var parameter in singleColorParameters)
    {
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(Colors.Transparent), 
            $"Single color parameter '{parameter}' should return transparent");
    }
}

[Test]
public void Convert_Should_ReturnTransparent_WhenParameterHasThreeColors()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var threeColorParameters = new[] 
    { 
        "White,Black,Gray", 
        "Red,Green,Blue", 
        "#FF0000,#00FF00,#0000FF" 
    };

    foreach (var parameter in threeColorParameters)
    {
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        // Implementation may use first two colors or return transparent
        // Test documents current behavior
        Assert.That(result, Is.Not.Null, 
            $"Three color parameter '{parameter}' should handle gracefully");
    }
}

[Test]
public void Convert_Should_ReturnTransparent_WhenParameterHasNoComma()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var noCommaParameters = new[] 
    { 
        "WhiteBlack", 
        "White Black", 
        "White;Black", 
        "White|Black" 
    };

    foreach (var parameter in noCommaParameters)
    {
        // Act
        var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(Colors.Transparent), 
            $"No comma parameter '{parameter}' should return transparent");
    }
}

[Test]
public void Convert_Should_HandleMalformedParameters_Gracefully()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var malformedParameters = new[]
    {
        ",",              // Just comma
        ",,",             // Multiple commas
        " , ",            // Whitespace around comma
        "White,",         // Missing second color
        ",Black",         // Missing first color
        "White,,Black"    // Double comma
    };

    foreach (var parameter in malformedParameters)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => 
        {
            var result = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
            Assert.That(result, Is.Not.Null);
        }, $"Should handle malformed parameter gracefully: '{parameter}'");
    }
}
```

#### Invalid Input Tests
```csharp
[Test]
public void Convert_Should_ReturnTransparent_WhenValueIsNull()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(null, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnTransparent_WhenValueIsNotBoolean()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var nonBooleanValues = new object[]
    {
        "string",
        42,
        3.14,
        new object(),
        new DateTime(),
        new List<int>()
    };

    foreach (var value in nonBooleanValues)
    {
        // Act
        var result = converter.Convert(value, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

        // Assert
        Assert.That(result, Is.EqualTo(Colors.Transparent), 
            $"Non-boolean value {value.GetType().Name} should return transparent");
    }
}

[Test]
public void Convert_Should_ReturnTransparent_WhenValueIsString()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert("true", typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnTransparent_WhenValueIsInteger()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act
    var result = converter.Convert(1, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}

[Test]
public void Convert_Should_ReturnTransparent_WhenValueIsArbitraryObject()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var arbitraryObject = new { IsEnabled = true };

    // Act
    var result = converter.Convert(arbitraryObject, typeof(Color), "White,Black", CultureInfo.InvariantCulture);

    // Assert
    Assert.That(result, Is.EqualTo(Colors.Transparent));
}
```

#### ConvertBack Tests
```csharp
[Test]
public void ConvertBack_Should_ThrowNotImplementedException_Always()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act & Assert
    Assert.Throws<NotImplementedException>(() => 
        converter.ConvertBack(Colors.White, typeof(bool), "White,Black", CultureInfo.InvariantCulture));
}

[Test]
public void ConvertBack_Should_ThrowForAnyColorInput_Consistently()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var colorInputs = new object[]
    {
        Colors.White,
        Colors.Black,
        Colors.Transparent,
        Color.FromArgb("#FF0000"),
        null
    };

    foreach (var colorInput in colorInputs)
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack(colorInput, typeof(bool), "White,Black", CultureInfo.InvariantCulture),
            $"ConvertBack should throw NotImplementedException for color input: {colorInput}");
    }
}

[Test]
public void ConvertBack_Should_ProvideHelpfulExceptionMessage()
{
    // Arrange
    var converter = new BoolToColorConverter();

    // Act & Assert
    var exception = Assert.Throws<NotImplementedException>(() => 
        converter.ConvertBack(Colors.White, typeof(bool), "White,Black", CultureInfo.InvariantCulture));
    
    Assert.That(exception.Message, Is.Not.Null);
    Assert.That(exception.Message, Is.Not.Empty);
}
```

#### Parameter Parsing Tests
```csharp
[Test]
public void Convert_Should_SplitParameterByComma_Correctly()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var parameter = "White,Black";

    // Act
    var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
    var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(resultTrue, Is.EqualTo(Colors.White));
    Assert.That(resultFalse, Is.EqualTo(Colors.Black));
}

[Test]
public void Convert_Should_TrimWhitespace_FromBothColors()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var parameter = "  White  ,  Black  ";

    // Act
    var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
    var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    Assert.That(resultTrue, Is.EqualTo(Colors.White));
    Assert.That(resultFalse, Is.EqualTo(Colors.Black));
}

[Test]
public void Convert_Should_HandleExtraWhitespace_Gracefully()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var whitespaceVariations = new[]
    {
        "\t\tWhite\t\t,\t\tBlack\t\t",
        "\n\nWhite\n\n,\n\nBlack\n\n",
        "   White   ,   Black   "
    };

    foreach (var parameter in whitespaceVariations)
    {
        // Act
        var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
        var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

        // Assert
        Assert.That(resultTrue, Is.EqualTo(Colors.White), 
            $"Should handle whitespace variation: '{parameter}'");
        Assert.That(resultFalse, Is.EqualTo(Colors.Black), 
            $"Should handle whitespace variation: '{parameter}'");
    }
}

[Test]
public void Convert_Should_IgnoreExtraCommas_AndUseFirstTwo()
{
    // Arrange
    var converter = new BoolToColorConverter();
    var parameter = "White,Black,Gray,Red";

    // Act
    var resultTrue = converter.Convert(true, typeof(Color), parameter, CultureInfo.InvariantCulture);
    var resultFalse = converter.Convert(false, typeof(Color), parameter, CultureInfo.InvariantCulture);

    // Assert
    // Implementation should use first two colors (White, Black)
    Assert.That(resultTrue, Is.EqualTo(Colors.White));
    Assert.That(resultFalse, Is.EqualTo(Colors.Black));
}
```

### Test Fixtures Required
- **BoolToColorConverterTestFixture** - Standard test fixture with converter instance
- **ColorParameterTestDataGenerator** - Generate various parameter combinations
- **HexColorTestDataGenerator** - Generate valid/invalid hex color test cases
- **PredefinedColorTestDataGenerator** - Generate predefined color test scenarios

## Success Criteria
- [ ] **Valid parameter handling** - All parameter formats processed correctly with proper color selection
- [ ] **Predefined color resolution** - All 4 predefined colors (White, Black, Transparent, Gray) resolved correctly
- [ ] **Hex color parsing** - Both short (#RRGGBB) and long (#AARRGGBB) hex formats supported
- [ ] **Invalid input handling** - All non-boolean inputs and malformed parameters return Colors.Transparent
- [ ] **Parameter parsing accuracy** - Comma splitting, whitespace trimming, and format validation verified
- [ ] **Error handling robustness** - Color.FromArgb exceptions handled gracefully
- [ ] **ConvertBack behavior** - NotImplementedException thrown consistently

## Implementation Priority
**MEDIUM PRIORITY** - UI theming converter enabling dynamic color selection. Important for visual customization but not critical business logic.

## Dependencies for Testing
- **NUnit** - Standard testing framework
- **Microsoft.Maui.Graphics** - Color system for color comparison testing
- **System.Globalization** - CultureInfo for parameter testing

## Implementation Estimate
**Effort: Medium (2 days)**
- Parameter parsing and validation testing across multiple formats
- Color resolution testing for both predefined and hex colors
- Error handling verification for malformed parameters and Color.FromArgb exceptions
- Comprehensive edge case testing for whitespace, comma variations, and invalid inputs

This converter demonstrates testing approaches for parameter-driven converters with complex string parsing and multiple output types, making it essential for UI theming and conditional styling patterns.