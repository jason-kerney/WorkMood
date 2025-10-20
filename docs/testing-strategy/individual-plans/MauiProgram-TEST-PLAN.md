# MauiProgram Refactoring & Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## Class Overview
**File**: `MauiApp/MauiProgram.cs`  
**Type**: Static Class (Application Bootstrap) → **Will become thin wrapper**  
**Primary Purpose**: Application initialization and dependency injection container configuration  
**Key Functionality**: MAUI app builder configuration, service registration, logging setup, command-line argument parsing

**New Architecture**: Extract testable logic into `MauiAppRunner` class for improved testability and maintainability.

## Current Testability Assessment

**MauiProgram Overall Testability Score: 5/10** 
- ✅ Pure static methods with clear inputs/outputs
- ✅ Well-defined responsibility separation (DI configuration)  
- ✅ Command-line parsing logic is deterministic
- ⚠️ Heavy dependency on MAUI framework (MauiApp.CreateBuilder())
- ⚠️ Service registration makes unit testing complex
- ❌ Main CreateMauiApp method returns framework type (not easily mockable)

**Target Architecture Testability Score: 9/10**
- ✅ Dependency injection through constructor
- ✅ Framework dependencies abstracted through interfaces
- ✅ Service registration logic fully testable
- ✅ Command-line parsing remains pure function
- ✅ Clear separation of concerns

## Refactoring Strategy: Extract MauiAppRunner

### Phase 1: Create MauiAppRunner Class

**Step 1: Create New Class Structure**
Create `MauiApp/Infrastructure/MauiAppRunner.cs` with the following structure:

```csharp
using Microsoft.Extensions.Logging;

namespace WorkMood.MauiApp.Infrastructure;

public class MauiAppRunner
{
    private readonly ILoggingService _loggingService;
    private readonly MauiAppBuilder _builder;
    
    public MauiAppRunner(ILoggingService loggingService, MauiAppBuilder builder)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }
    
    public MauiApp BuildApplication()
    {
        // Logic extracted from MauiProgram.CreateMauiApp will go here
        return ConfigureMauiApp();
    }
    
    private MauiApp ConfigureMauiApp()
    {
        // All service registration logic from CreateMauiApp
        return _builder.Build();
    }
}
```

**Step 2: Extract CreateMauiApp Logic**
Move the complete contents of `MauiProgram.CreateMauiApp()` method into `MauiAppRunner.ConfigureMauiApp()`:

- All service registrations (core services, shims, infrastructure, ViewModels, Pages)
- MoodDispatcherService factory configuration
- Any additional builder configuration

**Step 3: Identify Constructor Dependencies**
Based on analysis of the original `CreateMauiApp` method, the constructor needs:
- `ILoggingService` - Early logging service setup
- `MauiAppBuilder` - The MAUI application builder
- Any configuration objects created during initialization

**Step 4: Update MauiProgram to Use MauiAppRunner**
Modify `MauiProgram.CreateMauiApp()` to become a thin wrapper:

```csharp
public static MauiApp CreateMauiApp(string[]? args = null)
{
    var (loggingEnabled, logLevel) = ParseLoggingConfiguration(args ?? Array.Empty<string>());
    
    var builder = CreateBuilder();
    var loggingService = CreateEarlyLoggingService(loggingEnabled, logLevel);
    
    var appRunner = new MauiAppRunner(loggingService, builder);
    return appRunner.BuildApplication();
}

private static MauiAppBuilder CreateBuilder()
{
    return MauiApp.CreateBuilder();
}

private static ILoggingService CreateEarlyLoggingService(bool enabled, LogLevel level)
{
    // Early logging setup logic
}
```

### Phase 2: Apply Shim Pattern (If Needed)

**Step 5: Identify Framework Dependencies in MauiAppRunner**
After extraction, analyze `MauiAppRunner` for hard dependencies:
- Direct `MauiAppBuilder` usage → Create `IMauiAppBuilderShim` if needed
- Service registration patterns → May need `IServiceCollectionShim`
- MAUI-specific calls → Abstract through appropriate shims

**Step 6: Create Shims for Testability**
If framework dependencies prevent testing, create:

```csharp
public interface IMauiAppBuilderShim
{
    IMauiAppBuilderShim ConfigureFonts(Action<IFontCollection> configureDelegate);
    IMauiAppBuilderShim UseMauiApp<TApp>() where TApp : class;
    IMauiAppBuilderShim ConfigureLogging(Action<ILoggingBuilder> configure);
    IServiceCollection Services { get; }
    MauiApp Build();
}
```

**Step 7: Update MauiAppRunner Constructor**
If shims are needed, update constructor:

```csharp
public MauiAppRunner(ILoggingService loggingService, IMauiAppBuilderShim builder)
{
    // Constructor with shim abstraction
}
```

### Phase 3: Test MauiAppRunner

**Step 8: Create Comprehensive Test Suite**
Focus testing entirely on `MauiAppRunner` class since it contains all the logic.

## MauiAppRunner Testing Strategy

### 1. Constructor Dependency Tests
**Focus**: Verify proper dependency injection and null handling

```csharp
[Test]
public void Constructor_WithValidDependencies_ShouldCreateInstance()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    
    // Act & Assert
    Assert.DoesNotThrow(() => new MauiAppRunner(mockLoggingService.Object, mockBuilder.Object));
}

[Test]
public void Constructor_WithNullLoggingService_ShouldThrowArgumentNullException()
{
    // Arrange
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    
    // Act & Assert
    var ex = Assert.Throws<ArgumentNullException>(() => new MauiAppRunner(null, mockBuilder.Object));
    Assert.That(ex.ParamName, Is.EqualTo("loggingService"));
}

[Test]
public void Constructor_WithNullBuilder_ShouldThrowArgumentNullException()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    
    // Act & Assert
    var ex = Assert.Throws<ArgumentNullException>(() => new MauiAppRunner(mockLoggingService.Object, null));
    Assert.That(ex.ParamName, Is.EqualTo("builder"));
}
```

### 2. Service Registration Tests
**Focus**: Verify all services are properly registered through mocked builder

```csharp
[Test]
public void BuildApplication_RegistersCoreServices_AsSingletons()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    var mockServices = new Mock<IServiceCollection>();
    var mockApp = new Mock<MauiApp>();
    
    mockBuilder.Setup(b => b.Services).Returns(mockServices.Object);
    mockBuilder.Setup(b => b.Build()).Returns(mockApp.Object);
    
    var runner = new MauiAppRunner(mockLoggingService.Object, mockBuilder.Object);
    
    // Act
    var result = runner.BuildApplication();
    
    // Assert
    mockServices.Verify(s => s.AddSingleton<IDataArchiveService, DataArchiveService>(), Times.Once);
    mockServices.Verify(s => s.AddSingleton<IMoodDataService, MoodDataService>(), Times.Once);
    mockServices.Verify(s => s.AddSingleton<IScheduleConfigService, ScheduleConfigService>(), Times.Once);
}

[Test]
public void BuildApplication_RegistersViewModels_AsTransient()
{
    // Arrange - same setup as above
    
    // Act
    var result = runner.BuildApplication();
    
    // Assert
    mockServices.Verify(s => s.AddTransient<AboutPageViewModel>(), Times.Once);
    mockServices.Verify(s => s.AddTransient<GraphicsPageViewModel>(), Times.Once);
    mockServices.Verify(s => s.AddTransient<MainPageViewModel>(), Times.Once);
    // Verify all ViewModels are registered as transient
}

[Test]
public void BuildApplication_RegistersShimServices_WithCorrectLifetime()
{
    // Arrange - same setup as above
    
    // Act
    var result = runner.BuildApplication();
    
    // Assert
    mockServices.Verify(s => s.AddSingleton<IFileShimFactory, FileShimFactory>(), Times.Once);
    mockServices.Verify(s => s.AddSingleton<IDrawShimFactory, DrawShimFactory>(), Times.Once);
    // Verify all shim factories are registered
}
```

### 3. Application Building Tests
**Focus**: Verify the complete application building process

```csharp
[Test]
public void BuildApplication_ReturnsValidMauiApp()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    var mockServices = new Mock<IServiceCollection>();
    var expectedApp = new Mock<MauiApp>();
    
    mockBuilder.Setup(b => b.Services).Returns(mockServices.Object);
    mockBuilder.Setup(b => b.Build()).Returns(expectedApp.Object);
    
    var runner = new MauiAppRunner(mockLoggingService.Object, mockBuilder.Object);
    
    // Act
    var result = runner.BuildApplication();
    
    // Assert
    Assert.That(result, Is.SameAs(expectedApp.Object));
    mockBuilder.Verify(b => b.Build(), Times.Once);
}

[Test]
public void BuildApplication_ConfiguresLogging_WhenLoggingServiceProvided()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    
    // Setup logging service expectations
    mockLoggingService.Setup(l => l.IsEnabled).Returns(true);
    mockLoggingService.Setup(l => l.LogLevel).Returns(LogLevel.Debug);
    
    var runner = new MauiAppRunner(mockLoggingService.Object, mockBuilder.Object);
    
    // Act
    runner.BuildApplication();
    
    // Assert
    // Verify that logging configuration is applied based on the service
    mockBuilder.Verify(b => b.ConfigureLogging(It.IsAny<Action<ILoggingBuilder>>()), Times.Once);
}
```

### 4. Error Handling Tests
**Focus**: Verify proper error handling during application building

```csharp
[Test]
public void BuildApplication_WhenBuilderThrows_ShouldPropagateException()
{
    // Arrange
    var mockLoggingService = new Mock<ILoggingService>();
    var mockBuilder = new Mock<IMauiAppBuilderShim>();
    var mockServices = new Mock<IServiceCollection>();
    
    mockBuilder.Setup(b => b.Services).Returns(mockServices.Object);
    mockBuilder.Setup(b => b.Build()).Throws(new InvalidOperationException("Build failed"));
    
    var runner = new MauiAppRunner(mockLoggingService.Object, mockBuilder.Object);
    
    // Act & Assert
    var ex = Assert.Throws<InvalidOperationException>(() => runner.BuildApplication());
    Assert.That(ex.Message, Is.EqualTo("Build failed"));
}
```

## Simplified MauiProgram Testing

### Command-Line Argument Parsing Tests (Keep These)
The `ParseLoggingConfiguration` method remains in `MauiProgram` and should be tested as originally planned:
**Focus**: Verify ParseLoggingConfiguration handles all argument patterns correctly (remains in MauiProgram)

**Key Test Categories**:

- **Logging Flag Detection**: --logging, --log, -l variations
- **Log Level Parsing**: --logging=Debug, --log=Warning, -l=Error patterns  
- **Case Insensitivity**: Mixed case arguments handled correctly
- **Invalid Arguments**: Graceful handling of malformed inputs
- **Default Behavior**: No arguments returns (false, LogLevel.Info)
- **Error Handling**: Exception scenarios return safe defaults

**Example Tests**:

```csharp
[Test]
[TestCase(new string[] { "--logging" }, true, LogLevel.Info)]
[TestCase(new string[] { "--log" }, true, LogLevel.Info)]
[TestCase(new string[] { "-l" }, true, LogLevel.Info)]
[TestCase(new string[] { "--logging=Debug" }, true, LogLevel.Debug)]
[TestCase(new string[] { "--log=Warning" }, true, LogLevel.Warning)]
[TestCase(new string[] { "-l=Error" }, true, LogLevel.Error)]
[TestCase(new string[] { "--LOGGING=debug" }, true, LogLevel.Debug)]
[TestCase(new string[] { "--logging=InvalidLevel" }, true, LogLevel.Info)]
[TestCase(new string[] { }, false, LogLevel.Info)]
[TestCase(new string[] { "other", "args" }, false, LogLevel.Info)]
public void ParseLoggingConfiguration_ParsesArgumentsCorrectly(string[] args, bool expectedEnabled, LogLevel expectedLevel)
{
    var result = MauiProgram.ParseLoggingConfiguration(args);
    
    Assert.That(result.enabled, Is.EqualTo(expectedEnabled));
    Assert.That(result.level, Is.EqualTo(expectedLevel));
}

[Test]
public void ParseLoggingConfiguration_HandlesNullArguments_ReturnsDefaults()
{
    var result = MauiProgram.ParseLoggingConfiguration(null);
    
    Assert.That(result.enabled, Is.True);
    Assert.That(result.level, Is.EqualTo(LogLevel.Info));
}
```

### MauiProgram Integration Tests (Minimal)

**Focus**: Verify the thin wrapper correctly delegates to MauiAppRunner

```csharp
[Test]
public void CreateMauiApp_WithoutArgs_ShouldReturnValidApplication()
{
    // Act
    var app = MauiProgram.CreateMauiApp();
    
    // Assert
    Assert.That(app, Is.Not.Null);
    Assert.That(app.Services, Is.Not.Null);
}

[Test] 
public void CreateMauiApp_WithLoggingArgs_ShouldConfigureLoggingCorrectly()
{
    // Act
    var app = MauiProgram.CreateMauiApp(new[] { "--logging=Debug" });
    
    // Assert - Verify application was created successfully with debug logging
    Assert.That(app, Is.Not.Null);
    // Additional logging verification if needed
}
```

## Implementation Checklist

### Phase 1 - Refactoring Steps

- [ ] **Step 1**: Create `MauiApp/Infrastructure/MauiAppRunner.cs` class with constructor dependencies
- [ ] **Step 2**: Extract `CreateMauiApp` logic into `MauiAppRunner.BuildApplication()` method  
- [ ] **Step 3**: Identify and inject all dependencies (ILoggingService, MauiAppBuilder) into constructor
- [ ] **Step 4**: Update `MauiProgram.CreateMauiApp()` to become thin wrapper using `MauiAppRunner`
- [ ] **Step 5**: Verify application still runs correctly after refactoring
- [ ] **Step 6**: Create shim abstractions if direct framework dependencies prevent testing
- [ ] **Step 7**: Update `MauiAppRunner` to use shims (if needed) for complete testability

### Phase 2 - Testing Implementation

- [ ] **Step 8**: Create `MauiAppRunnerTests.cs` with comprehensive test suite
- [ ] **Step 9**: Implement constructor dependency tests (null handling, proper injection)  
- [ ] **Step 10**: Implement service registration verification tests using mocked dependencies
- [ ] **Step 11**: Implement application building process tests
- [ ] **Step 12**: Implement error handling and edge case tests
- [ ] **Step 13**: Create minimal `MauiProgramTests.cs` for argument parsing and wrapper functionality
- [ ] **Step 14**: Verify 90%+ coverage on `MauiAppRunner` and argument parsing logic

### Phase 3 - Validation

- [ ] **Step 15**: Run full application to ensure identical behavior after refactoring
- [ ] **Step 16**: Execute complete test suite and verify all tests pass
- [ ] **Step 17**: Verify code coverage meets 90% target for testable components
- [ ] **Step 18**: Validate that service resolution works correctly in both test and runtime environments

## Test Organization

```
MauiApp.Tests/
├── Infrastructure/
│   ├── MauiAppRunnerTests.cs        # Primary test focus - comprehensive coverage
│   ├── MauiAppBuilderShimTests.cs   # If shims are needed
│   └── MauiProgramTests.cs          # Minimal wrapper testing + argument parsing
```

## Coverage Goals

### MauiAppRunner (Primary Focus)
- **Constructor & Dependencies**: 100% - all dependency injection scenarios
- **Service Registration**: 95% - verify all critical services registered correctly  
- **Application Building**: 90% - core building process with error handling
- **Configuration Methods**: 90% - all service configuration logic

### MauiProgram (Minimal Testing)
- **ParseLoggingConfiguration**: 100% - pure function with clear inputs/outputs
- **CreateMauiApp Wrapper**: 80% - integration testing of delegation to MauiAppRunner
- **Helper Methods**: 100% - HasLoggingLevel, TryParseLogLevel are pure functions

## Dependencies for Testing

### Required NuGet Packages
- **NUnit**: Primary testing framework
- **Moq**: Mocking framework for interfaces and dependencies
- **Microsoft.Extensions.DependencyInjection.Abstractions**: For service collection mocking
- **Microsoft.Extensions.Logging.Abstractions**: For logging service abstractions

### Shim Requirements (If Needed)
- **IMauiAppBuilderShim**: Abstraction over MauiAppBuilder for testability
- **IServiceCollectionShim**: Service registration abstraction (if needed)
- **ILoggingBuilderShim**: Logging configuration abstraction (if needed)

## Risk Assessment

### Low Risk (After Refactoring)
- **MauiAppRunner Logic**: Fully testable through dependency injection and mocking
- **Argument Parsing**: Pure functions with deterministic outputs
- **Service Registration**: Mockable dependencies enable complete verification

### Medium Risk
- **Integration**: Ensuring real application and test environment work identically
- **Shim Implementation**: If shims are needed, ensuring they properly abstract framework dependencies

### High Risk (Mitigated)
- **Framework Dependency**: Eliminated through extraction to testable `MauiAppRunner`
- **Service Resolution**: Mitigated through comprehensive registration testing

## Refactoring Commit Strategy

Use Arlo's Commit Notation for systematic refactoring:

1. `^r - create MauiAppRunner class with dependency injection constructor`
2. `^r - extract CreateMauiApp logic into MauiAppRunner.BuildApplication method`  
3. `^r - update MauiProgram.CreateMauiApp to delegate to MauiAppRunner`
4. `^r - add shim abstractions for MauiAppBuilder testability` (if needed)
5. `^r - update MauiAppRunner constructor to use shim dependencies` (if needed)
6. `^f - add comprehensive test suite for MauiAppRunner with 90% coverage`
7. `^f - add minimal test suite for MauiProgram argument parsing and wrapper logic`

## Benefits of This Approach

### Improved Testability
- **Complete Logic Testing**: All service registration and application building logic becomes fully testable
- **Isolated Dependencies**: Framework dependencies are abstracted and mockable
- **Clear Test Scope**: Testing focuses on business logic rather than framework integration

### Better Architecture  
- **Separation of Concerns**: Static entry point separated from configurable application building
- **Dependency Injection**: Proper constructor injection enables better design patterns
- **Maintainability**: Logic is contained in testable, non-static classes

### Preserved Functionality
- **Identical Behavior**: Application behavior remains exactly the same
- **Minimal MauiProgram**: Static entry point becomes simple and focused
- **Framework Compatibility**: All MAUI requirements still properly satisfied