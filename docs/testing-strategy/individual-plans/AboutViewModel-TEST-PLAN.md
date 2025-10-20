# AboutViewModel Test Plan

## Testing Framework Requirements
**Testing Framework**: xUnit with Assert.* methods (NOT NUnit)  
**Required Imports**: `using Xunit;`  
**Assertion Style**: `Assert.NotNull()`, `Assert.Equal()`, `Assert.True()` etc. (xUnit syntax)  
**Test Method Attributes**: `[Fact]` for single tests, `[Theory]` for parameterized tests  
**NOT SUPPORTED**: NUnit syntax (`[Test]`, `Assert.That()`, `Is.EqualTo()`, etc.)

## 1. Object Analysis

### Purpose & Responsibilities
The `AboutViewModel` serves as the business logic controller for the About page in WorkMood. It provides application metadata, version information, and logging configuration controls to users. This ViewModel follows MVVM pattern principles by completely separating UI concerns from business logic.

**Core Responsibilities**:
- Display application information (title, version, description)
- Provide logging configuration interface (enable/disable, log level selection)
- Handle version information retrieval and display
- Manage logging service state changes with appropriate feedback

### Architecture Role
**Pattern**: MVVM ViewModel
**Layer**: Presentation Layer (MVVM Clean Architecture)
**Base Class**: `ViewModelBase` (provides `INotifyPropertyChanged` implementation)
**DI Registration**: Registered as `Transient` in `MauiProgram.cs`

### Dependencies Analysis

#### Constructor Dependencies (Interfaces/Services)
- `IBrowserService _browserService` - For opening external URLs (currently unused but injected)
- `IVersionRetriever _versionRetriever` - Retrieves application version information
- `IMoodDataService _moodDataService` - Mood data operations (currently unused but injected)
- `ILoggingService _loggingService` - Controls logging state and configuration

#### Static Dependencies
- **None identified** - All dependencies properly injected

#### Platform Dependencies
- **None identified** - No direct platform calls

### Public Interface Documentation

#### Public Properties (with INotifyPropertyChanged)
- `string AppTitle { get; set; }` - Application title display text
- `string AppVersion { get; set; }` - Formatted version string (e.g., "Version 0.3.0")
- `string AppDescription { get; set; }` - Application description for users
- `bool IsLoggingEnabled { get; set; }` - Controls logging service enabled state
- `List<string> LogLevels { get; }` - Available log level options (read-only)
- `string SelectedLogLevel { get; set; }` - Currently selected minimum log level

#### Public Methods
- **Constructor**: `AboutViewModel(IBrowserService, IVersionRetriever, IMoodDataService, ILoggingService)`

#### Commands
- **None currently implemented**

#### Events
- **PropertyChanged** (inherited from ViewModelBase) - Raised for all property changes

## 2. Testability Assessment

**📚 For comprehensive refactoring guidance**: See `.github/ai-codex-refactoring.md` for detailed shim factory methodology, existing abstractions, refactoring priorities, and anti-patterns to avoid.

### Current Testability Score: 9/10

**Justification**: AboutViewModel demonstrates excellent testability with proper dependency injection, clear separation of concerns, and no hard dependencies. The only minor issue is unused injected dependencies that create unnecessary coupling.

### Hard Dependencies Identified
- **None** - All dependencies properly abstracted through interfaces

### Required Refactoring
**Minor Cleanup Recommended**:
- Remove unused `IBrowserService` and `IMoodDataService` dependencies if not planned for future features
- Consider making `LogLevels` property injectable for better testability of log level options

**No Blocking Issues**: Object is fully testable as-is.

## 3. Test Implementation Strategy

**📚 For comprehensive build & testing guidance**: See `.github/ai-codex-build-testing.md` for detailed framework targeting, cross-platform builds, testing strategies, quality gates, and CI/CD configuration.

### Test Class Structure
```csharp
[TestFixture]
public class AboutViewModelTests
{
    private Mock<IBrowserService> _mockBrowserService;
    private Mock<IVersionRetriever> _mockVersionRetriever;
    private Mock<IMoodDataService> _mockMoodDataService;
    private Mock<ILoggingService> _mockLoggingService;
    private AboutViewModel _sut; // System Under Test
    
    [SetUp]
    public void SetUp()
    {
        // Mock initialization
        // System under test creation
    }
    
    [TearDown]
    public void TearDown()
    {
        // Cleanup
    }
}
```

### Mock Strategy
- **IBrowserService**: Mock using Moq (unused but required for constructor)
- **IVersionRetriever**: Mock version retrieval scenarios (success, failure)
- **IMoodDataService**: Mock using Moq (unused but required for constructor)
- **ILoggingService**: Mock logging state and configuration changes
- **Test Data**: Use `VersionInfo` objects for version testing scenarios

### Test Categories
- **Constructor Tests**: Dependency injection validation and initialization
- **Property Tests**: Property change notifications and value updates
- **Version Initialization**: Version retrieval and fallback scenarios
- **Logging Configuration**: Logging state and level management
- **Error Handling**: Exception scenarios and graceful degradation

## 4. Detailed Test Cases

### Constructor: AboutViewModel(...)
**Purpose**: Validates dependency injection and proper initialization

#### Test Cases:
- **Happy Path**: Valid dependencies create functional ViewModel
- **Edge Cases**: 
  - Null `browserService` throws `ArgumentNullException`
  - Null `versionRetriever` throws `ArgumentNullException`
  - Null `moodDataService` throws `ArgumentNullException`
  - Null `loggingService` throws `ArgumentNullException`
- **Error Conditions**: Version retrieval failure falls back gracefully
- **Async Scenarios**: N/A (constructor is synchronous)

**Test Implementation Examples**:
```csharp
[Test]
public void Constructor_WhenValidDependencies_ShouldInitializeSuccessfully()
{
    // Arrange
    var mockBrowser = new Mock<IBrowserService>();
    var mockVersion = new Mock<IVersionRetriever>();
    var mockMoodData = new Mock<IMoodDataService>();
    var mockLogging = new Mock<ILoggingService>();
    
    mockVersion.Setup(v => v.GetVersion()).Returns(new VersionInfo { Version = "0.3.0" });
    
    // Act
    var sut = new AboutViewModel(mockBrowser.Object, mockVersion.Object, mockMoodData.Object, mockLogging.Object);
    
    // Assert
    Assert.That(sut, Is.Not.Null);
    Assert.That(sut.AppTitle, Is.EqualTo("WorkMood - Daily Mood Tracker"));
    Assert.That(sut.AppVersion, Is.EqualTo("Version 0.3.0"));
    mockVersion.Verify(v => v.GetVersion(), Times.Once);
}

[Test]
public void Constructor_WhenVersionRetrievalFails_ShouldUseFallbackVersion()
{
    // Arrange
    var mockBrowser = new Mock<IBrowserService>();
    var mockVersion = new Mock<IVersionRetriever>();
    var mockMoodData = new Mock<IMoodDataService>();
    var mockLogging = new Mock<ILoggingService>();
    
    mockVersion.Setup(v => v.GetVersion()).Throws(new InvalidOperationException("Version not found"));
    
    // Act
    var sut = new AboutViewModel(mockBrowser.Object, mockVersion.Object, mockMoodData.Object, mockLogging.Object);
    
    // Assert
    Assert.That(sut.AppVersion, Is.EqualTo("Version 0.1.0"));
}

[Test]
public void Constructor_WhenBrowserServiceIsNull_ShouldThrowArgumentNullException()
{
    // Arrange
    var mockVersion = new Mock<IVersionRetriever>();
    var mockMoodData = new Mock<IMoodDataService>();
    var mockLogging = new Mock<ILoggingService>();
    
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new AboutViewModel(null, mockVersion.Object, mockMoodData.Object, mockLogging.Object));
}
```

### Property: AppTitle
**Purpose**: Manages application title display

#### Test Cases:
- **Happy Path**: Setting valid title raises PropertyChanged event
- **Edge Cases**: Setting same value doesn't raise event, empty/null values handled
- **Error Conditions**: N/A (simple string property)

### Property: AppVersion
**Purpose**: Displays formatted version information

#### Test Cases:
- **Happy Path**: Setting version raises PropertyChanged event
- **Edge Cases**: Setting same value behavior, format validation
- **Error Conditions**: N/A (simple string property)

### Property: AppDescription  
**Purpose**: Provides application description text

#### Test Cases:
- **Happy Path**: Setting description raises PropertyChanged event
- **Edge Cases**: Setting same value behavior
- **Error Conditions**: N/A (simple string property)

### Property: IsLoggingEnabled
**Purpose**: Controls logging service enabled state with side effects

#### Test Cases:
- **Happy Path**: Enabling logging calls service and logs change
- **Edge Cases**: 
  - Setting same value doesn't trigger service calls
  - Disabling logging doesn't attempt to log (since logging disabled)
- **Error Conditions**: Logging service exceptions handled gracefully

**Test Implementation Examples**:
```csharp
[Test]
public void IsLoggingEnabled_WhenEnabledFromDisabled_ShouldEnableServiceAndLog()
{
    // Arrange
    _mockLoggingService.Setup(l => l.IsEnabled).Returns(false);
    _mockLoggingService.SetupProperty(l => l.IsEnabled);
    
    // Act
    _sut.IsLoggingEnabled = true;
    
    // Assert
    _mockLoggingService.VerifySet(l => l.IsEnabled = true, Times.Once);
    _mockLoggingService.Verify(l => l.Log("Logging has been enabled via About page"), Times.Once);
}

[Test]
public void IsLoggingEnabled_WhenDisabledFromEnabled_ShouldDisableServiceWithoutLogging()
{
    // Arrange
    _mockLoggingService.Setup(l => l.IsEnabled).Returns(true);
    _mockLoggingService.SetupProperty(l => l.IsEnabled);
    
    // Act
    _sut.IsLoggingEnabled = false;
    
    // Assert
    _mockLoggingService.VerifySet(l => l.IsEnabled = false, Times.Once);
    _mockLoggingService.Verify(l => l.Log(It.IsAny<string>()), Times.Never);
}
```

### Property: LogLevels
**Purpose**: Provides available log level options

#### Test Cases:
- **Happy Path**: Returns expected log levels in correct order
- **Edge Cases**: List is read-only, contains expected values
- **Error Conditions**: N/A (static list)

### Property: SelectedLogLevel
**Purpose**: Manages current minimum log level selection

#### Test Cases:
- **Happy Path**: Valid log level updates service and logs change
- **Edge Cases**: 
  - Invalid log level string ignored
  - Setting same value doesn't trigger service calls
- **Error Conditions**: Parse failures handled gracefully

**Test Implementation Examples**:
```csharp
[Test]
public void SelectedLogLevel_WhenValidLevel_ShouldUpdateServiceAndLog()
{
    // Arrange
    _mockLoggingService.Setup(l => l.MinimumLogLevel).Returns(LogLevel.Info);
    _mockLoggingService.SetupProperty(l => l.MinimumLogLevel);
    _mockLoggingService.Setup(l => l.IsEnabled).Returns(true);
    
    // Act
    _sut.SelectedLogLevel = "Debug";
    
    // Assert
    _mockLoggingService.VerifySet(l => l.MinimumLogLevel = LogLevel.Debug, Times.Once);
    _mockLoggingService.Verify(l => l.Log("Minimum log level changed to Debug via About page"), Times.Once);
}

[Test]
public void SelectedLogLevel_WhenInvalidLevel_ShouldNotUpdateService()
{
    // Arrange
    _mockLoggingService.Setup(l => l.MinimumLogLevel).Returns(LogLevel.Info);
    
    // Act
    _sut.SelectedLogLevel = "InvalidLevel";
    
    // Assert
    _mockLoggingService.VerifySet(l => l.MinimumLogLevel = It.IsAny<LogLevel>(), Times.Never);
}
```

### Method: InitializeVersionInfo (Private)
**Purpose**: Retrieves and sets version information during construction

#### Test Cases:
- **Happy Path**: Version retrieved successfully and formatted
- **Edge Cases**: Version with different formats
- **Error Conditions**: Version retrieval exception uses fallback

## 5. MVVM-Specific Testing

### Property Change Notification Tests
```csharp
[Test]
public void AppTitle_WhenChanged_ShouldRaisePropertyChangedEvent()
{
    // Arrange
    var eventRaised = false;
    _sut.PropertyChanged += (s, e) => {
        if (e.PropertyName == nameof(_sut.AppTitle))
            eventRaised = true;
    };
    
    // Act
    _sut.AppTitle = "New Title";
    
    // Assert
    Assert.That(eventRaised, Is.True);
}
```

### Command Testing
**N/A** - AboutViewModel currently has no commands

### Data Binding Scenarios
- Verify all properties implement INotifyPropertyChanged correctly
- Test two-way binding scenarios for IsLoggingEnabled and SelectedLogLevel
- Validate property getter/setter behavior

### Navigation Testing  
**N/A** - AboutViewModel doesn't handle navigation directly

## 6. Coverage Goals

### Target Coverage
- **Line Coverage**: 95% minimum (simple ViewModel with clear logic paths)
- **Branch Coverage**: 90% minimum
- **Method Coverage**: 100% (all public members tested)

### Priority Areas
- Constructor dependency validation (critical for DI)
- Property change notifications (critical for MVVM binding)
- Logging configuration logic (business critical functionality)
- Version initialization and fallback (error handling)

### Acceptable Exclusions
- Exception handling catch blocks that only provide fallback values
- Generated property setter boilerplate (covered by base class tests)

### Measurement Strategy
- Run coverage after each test implementation phase
- Use `dotnet test --collect:"XPlat Code Coverage"` 
- Focus on uncovered branches in logging and version scenarios

## 7. Implementation Checklist

### Phase 1 - Testability ✅
- [x] **Object Analysis Complete**: AboutViewModel already properly designed for testing
- [x] **Dependencies Identified**: All dependencies injectable and mockable
- [x] **No Refactoring Required**: Object is fully testable as-is

### Phase 2 - Test Setup
- [ ] Create `AboutViewModelTests.cs` in `WorkMood.MauiApp.Tests/ViewModels/`
- [ ] Setup NUnit test framework with Moq for mocking
- [ ] Create test class structure with SetUp/TearDown methods
- [ ] Initialize all required mocks (IBrowserService, IVersionRetriever, etc.)

### Phase 3 - Core Tests  
- [ ] **Constructor Tests**: All dependency injection scenarios
- [ ] **Property Tests**: AppTitle, AppVersion, AppDescription basic functionality
- [ ] **Version Tests**: InitializeVersionInfo success and failure paths
- [ ] **Logging Tests**: IsLoggingEnabled and SelectedLogLevel core scenarios

### Phase 4 - Edge Cases
- [ ] **Null Handling**: All constructor null argument scenarios
- [ ] **Property Edge Cases**: Same value setting, empty strings
- [ ] **Logging Edge Cases**: Invalid log levels, service exceptions
- [ ] **PropertyChanged Events**: Verify all property change notifications

### Phase 5 - Coverage Verification
- [ ] Run coverage analysis: `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Generate coverage report: `reportgenerator` with AboutViewModel filter
- [ ] Achieve 95%+ line coverage and 90%+ branch coverage
- [ ] Document any acceptable exclusions

### Phase 6 - Code Review
- [ ] Verify test naming follows `MethodName_Scenario_ExpectedResult` pattern
- [ ] Ensure proper mock setup and verification
- [ ] Validate test independence (no shared state)
- [ ] Confirm tests execute quickly (<50ms each)

## 8. Arlo's Commit Strategy

### Planned Commits (Arlo's Commit Notation)
```
^f - add AboutViewModel test infrastructure with mocks and test class setup
^f - add AboutViewModel constructor tests for dependency injection validation  
^f - add AboutViewModel property change notification tests for MVVM compliance
^f - add AboutViewModel logging configuration tests for business logic coverage
^f - add AboutViewModel version initialization tests with error handling scenarios
^f - add AboutViewModel edge case tests to achieve 95% coverage target
```

### Commit Granularity
- **One test category per commit** (constructor, properties, logging, etc.)
- **Manual verification** after each commit to ensure tests pass
- **Coverage check** after core test implementation
- **Final verification** before marking object complete

---

**Success Criteria Met**: This test plan provides complete guidance for implementing comprehensive tests for AboutViewModel, achieving 95%+ coverage while following WorkMood's MVVM and testing patterns.

**Next Steps**: Proceed with manual verification gate, then implement the test plan following the detailed checklist above.