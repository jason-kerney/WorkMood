# AI Codex: Build & Testing Processes

## Overview
This codex provides comprehensive guidance for building, testing, and quality assurance within the WorkMood MAUI application, focusing on framework targeting, cross-platform considerations, and testing strategies.

**When to Reference**: Setting up build processes; configuring testing environments; troubleshooting compilation issues; implementing cross-platform builds; establishing quality gates; organizing test projects.

## Framework Targeting Requirements

### Critical Framework Specification
MAUI applications **MUST** specify target frameworks explicitly due to multi-platform support. Never use generic `dotnet build` without framework specification for MAUI projects.

#### Windows Development (Primary Platform)
```bash
# Standard build
dotnet build --framework net9.0-windows10.0.19041.0

# Specific project build
dotnet build MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# Run application
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# Build with detailed output
dotnet build --framework net9.0-windows10.0.19041.0 --verbosity detailed

# Clean build
dotnet clean && dotnet build --framework net9.0-windows10.0.19041.0
```

**Windows Requirements**:
- **Minimum**: Windows 10 version 1809 (Build 17763)
- **Target**: Windows 10 version 19041 (May 2020 Update)
- **Architecture**: x64, x86, ARM64 support
- **Dependencies**: Windows App SDK, WinUI 3

#### macOS Development (Cross-Platform)
```bash
# Standard build
dotnet build --framework net9.0-maccatalyst

# Publishing for distribution
dotnet publish --framework net9.0-maccatalyst --configuration Release

# Creating app bundle
dotnet publish --framework net9.0-maccatalyst -p:CreatePackage=true

# Build with code signing (when configured)
dotnet publish --framework net9.0-maccatalyst -p:CodesignKey="Developer ID Application: Your Name"
```

**macOS Requirements**:
- **Minimum**: macOS 15.0 Monterey
- **Architecture**: x64 (Intel), ARM64 (Apple Silicon)
- **Dependencies**: Xcode Command Line Tools
- **Code Signing**: Required for distribution

#### Solution-Wide Operations
```bash
# Build all projects in solution
dotnet build WorkMood.sln

# Build with specific configuration
dotnet build WorkMood.sln --configuration Release

# Clean entire solution
dotnet clean WorkMood.sln

# Restore packages for all projects
dotnet restore WorkMood.sln

# Watch mode for development
dotnet watch run --project WorkMood.sln
```

### Platform-Specific Build Configurations

#### Debug Configuration
```bash
# Windows Debug
dotnet build --framework net9.0-windows10.0.19041.0 --configuration Debug

# macOS Debug  
dotnet build --framework net9.0-maccatalyst --configuration Debug

# Enable detailed debugging
dotnet build --framework net9.0-windows10.0.19041.0 --configuration Debug -p:DebugSymbols=true -p:DebugType=portable
```

#### Release Configuration
```bash
# Windows Release
dotnet build --framework net9.0-windows10.0.19041.0 --configuration Release

# macOS Release
dotnet build --framework net9.0-maccatalyst --configuration Release

# Optimized release build
dotnet build --framework net9.0-windows10.0.19041.0 --configuration Release -p:PublishTrimmed=true -p:TrimMode=link
```

### Build Troubleshooting

#### Common Build Issues
1. **Framework Not Found**:
   ```bash
   # Install required workloads
   dotnet workload install maui
   dotnet workload install maui-windows
   dotnet workload install maui-maccatalyst
   ```

2. **Package Restore Issues**:
   ```bash
   # Clear package cache
   dotnet nuget locals all --clear
   dotnet restore --force
   ```

3. **MSBuild Cache Issues**:
   ```bash
   # Clean MSBuild cache
   dotnet clean
   rd /s /q bin obj  # Windows
   rm -rf bin obj    # macOS/Linux
   ```

#### Build Performance Optimization
```bash
# Parallel builds
dotnet build --framework net9.0-windows10.0.19041.0 -m

# Skip unnecessary steps during development
dotnet build --framework net9.0-windows10.0.19041.0 --no-restore

# Incremental builds
dotnet build --framework net9.0-windows10.0.19041.0 --no-dependencies
```

## Testing Strategy & Organization

### Test Project Structure
Mirror the main application structure in test projects to maintain organization and discoverability:

```
WorkMood.MauiApp.Tests/
├── ViewModels/              # ViewModel unit tests
│   ├── MainPageViewModelTests.cs
│   ├── MoodEntryViewModelTests.cs
│   └── SettingsViewModelTests.cs
├── Services/                # Service integration tests
│   ├── MoodDataServiceTests.cs
│   ├── LineGraphServiceTests.cs
│   └── NavigationServiceTests.cs
├── Models/                  # Model validation tests
│   ├── MoodEntryTests.cs
│   ├── AppSettingsTests.cs
│   └── ChartDataPointTests.cs
├── Shims/                   # Shim implementation tests
│   ├── FileShimTests.cs
│   ├── DrawingShimsTests.cs
│   └── DateShimTests.cs
├── TestHelpers/             # Shared test utilities
│   ├── MockServices/        # Service mocks
│   ├── TestData/           # Test data builders
│   └── Extensions/         # Test extension methods
├── Integration/             # Full integration scenarios
│   ├── DataFlowTests.cs
│   ├── NavigationFlowTests.cs
│   └── EndToEndScenarios.cs
└── Processors/             # Business logic tests
    ├── MoodDataProcessorTests.cs
    └── TrendAnalysisProcessorTests.cs
```

### Test Execution Commands

#### Basic Test Execution
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests in parallel
dotnet test --parallel

# Run specific test project
dotnet test WorkMood.MauiApp.Tests/WorkMood.MauiApp.Tests.csproj
dotnet test whats-your-version-tests/whats-your-version-tests.csproj
```

#### Test Filtering and Selection
```bash
# Run tests by category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration

# Run tests by namespace
dotnet test --filter FullyQualifiedName~WorkMood.MauiApp.Tests.ViewModels

# Run specific test method
dotnet test --filter Name=ShouldCalculateMoodTrendCorrectly

# Run tests matching pattern
dotnet test --filter DisplayName~"Mood*"
```

#### Code Coverage Analysis
```bash
# Collect coverage data
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report (requires reportgenerator tool)
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html

# Coverage with threshold enforcement
dotnet test --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Threshold=80
```

### Testing Principles & Best Practices

#### Unit Testing Focus Areas
1. **ViewModels**: 
   - Property change notifications
   - Command execution logic
   - Data binding scenarios
   - Validation rules

2. **Services**:
   - Business logic correctness
   - Error handling
   - Dependency interactions
   - Async operation handling

3. **Models**:
   - Data validation
   - Serialization/deserialization
   - Immutability contracts
   - Value object behavior

#### Integration Testing Scope
1. **Service Integration**:
   - Multi-service workflows
   - Data persistence scenarios
   - External dependency interaction
   - Cross-cutting concerns

2. **Navigation Flows**:
   - Page transition logic
   - Parameter passing
   - Back navigation handling
   - Deep linking scenarios

#### Shim-Based Testing Strategy
**Isolation Through Abstractions**:
```csharp
// Example: Testing file operations without file system
[Test]
public async Task ShouldSaveMoodDataCorrectly()
{
    // Arrange
    var mockFileShim = new Mock<IFileShim>();
    var mockFileFactory = new Mock<IFileShimFactory>();
    mockFileFactory.Setup(f => f.CreateFile(It.IsAny<string>()))
               .Returns(mockFileShim.Object);
    
    var dataService = new MoodDataService(mockFileFactory.Object);
    
    // Act
    await dataService.SaveMoodEntryAsync(testMoodEntry);
    
    // Assert
    mockFileShim.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
}
```

#### Test Data Management
**Builder Pattern for Test Data**:
```csharp
public class MoodEntryBuilder
{
    private MoodEntry _moodEntry = new MoodEntry();
    
    public MoodEntryBuilder WithMoodLevel(int level)
    {
        _moodEntry.MoodLevel = level;
        return this;
    }
    
    public MoodEntryBuilder WithDate(DateTime date)
    {
        _moodEntry.Date = date;
        return this;
    }
    
    public MoodEntry Build() => _moodEntry;
}

// Usage in tests
var testEntry = new MoodEntryBuilder()
    .WithMoodLevel(7)
    .WithDate(DateTime.Today)
    .Build();
```

### Quality Gates & Continuous Integration

#### Pre-Commit Quality Checks
```bash
# Full quality gate sequence
dotnet clean
dotnet restore
dotnet build --framework net9.0-windows10.0.19041.0 --no-restore
dotnet test --no-build
dotnet publish --framework net9.0-windows10.0.19041.0 --configuration Release --no-build
```

#### Build Validation Pipeline
1. **Clean Build**: Ensure no cached artifacts affect build
2. **Package Restore**: Verify all dependencies are available
3. **Compilation**: Confirm code compiles without errors
4. **Unit Tests**: Execute all automated tests
5. **Integration Tests**: Run integration test suite
6. **Coverage Analysis**: Ensure adequate test coverage
7. **Static Analysis**: Run code quality checks
8. **Package Creation**: Validate deployment artifacts

#### Performance Testing Considerations
```bash
# Memory usage profiling
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0 --configuration Release

# Startup time measurement
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0 -- --measure-startup

# Load testing for data operations
dotnet test --filter Category=Performance --configuration Release
```

### Platform-Specific Testing Considerations

#### Windows-Specific Testing
- **UI Automation**: WinAppDriver for UI testing
- **File System**: Windows path handling and permissions
- **Registry**: Windows-specific configuration storage
- **Notifications**: Windows notification system integration

#### macOS-Specific Testing
- **Sandboxing**: App Store sandbox restrictions
- **File System**: macOS permission model
- **Notifications**: macOS notification center integration
- **Accessibility**: VoiceOver compatibility

### Test Environment Configuration

#### Development Environment Setup
```bash
# Install test tools
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool install --global dotnet-coverage

# Configure test settings
dotnet new editorconfig
# Add test-specific settings to .editorconfig

# Setup test data
mkdir TestData
# Populate with sample data files
```

#### CI/CD Pipeline Configuration
```yaml
# Example GitHub Actions workflow
name: Build and Test
on: [push, pull_request]
jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --framework net9.0-windows10.0.19041.0 --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

### Debugging and Diagnostics

#### Debug Build Configuration
```bash
# Debug with specific framework
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0 --configuration Debug

# Enable additional diagnostics
dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0 --configuration Debug --verbosity diagnostic
```

#### Test Debugging
```bash
# Debug specific test
dotnet test --filter Name=SpecificTestMethod --logger "console;verbosity=detailed"

# Attach debugger to test process
dotnet test --debug --filter Name=SpecificTestMethod
```

#### Performance Profiling
```bash
# Memory profiling
dotnet-trace collect --providers Microsoft-DotNETCore-SampleProfiler -- dotnet run --project MauiApp/WorkMood.MauiApp.csproj --framework net9.0-windows10.0.19041.0

# CPU profiling
dotnet-counters monitor --process-id [PID] Microsoft.AspNetCore.Hosting System.Runtime
```

---

*This codex serves as the definitive guide for all build and testing operations within the WorkMood application. All build scripts, CI/CD pipelines, and testing strategies should align with these established patterns and practices.*