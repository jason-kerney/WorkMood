# AI Codex: Code Creation & Architecture

## Overview
This codex provides comprehensive guidance for creating code within the WorkMood MAUI application, focusing on MVVM Clean Architecture principles, project organization, and service design patterns.

**When to Reference**: Creating new classes, services, or ViewModels; organizing project structure; implementing dependency injection; designing service interfaces; setting up MVVM patterns.

## MAUI MVVM Clean Architecture Requirements

### Core Architectural Principles
All code in WorkMood must strictly follow these patterns:

#### MVVM Pattern Implementation
- **ViewModels**: 
  - No direct UI manipulation or dependencies
  - Implement `INotifyPropertyChanged` for data binding
  - Use command patterns for user interactions
  - Handle business logic orchestration only
  
- **Views (XAML + Code-Behind)**:
  - XAML defines UI structure and bindings
  - Code-behind handles view-specific logic only
  - No business logic in code-behind
  - Compiled bindings enabled for performance
  
- **Models**: 
  - Pure data containers and DTOs
  - No business logic or UI concerns
  - Immutable where possible
  - Support serialization requirements

#### Dependency Injection Requirements
- **Constructor Injection**: All services injected via constructors
- **Interface-First Design**: Never depend on concrete implementations
- **Service Lifetime Management**: Appropriate scoping (Singleton, Transient, Scoped)
- **Registration Pattern**: All services registered in `MauiProgram.cs`

#### Interface Segregation Principles  
- **Naming Convention**: All service interfaces prefixed with `I[ServiceName]`
- **Single Responsibility**: Interfaces should have one clear purpose
- **Client-Specific**: Design interfaces around client needs, not implementation
- **Composition Over Inheritance**: Favor composition of smaller interfaces

#### Nullable Reference Types
- **Project-Wide Enabled**: All projects have nullable reference types enabled
- **Explicit Nullability**: Always declare nullable intentions (`string?` vs `string`)
- **Null-Safe Operations**: Use null-conditional operators and null-coalescing
- **Validation Patterns**: Implement proper null checks and validations

## Detailed Folder Structure

### Primary Application Structure (MauiApp/)

#### ViewModels/
**Purpose**: MVVM ViewModels implementing business logic orchestration
- **Base Classes**: Inherit from `BaseViewModel` for common functionality
- **Command Implementation**: Use `RelayCommand` or `AsyncRelayCommand`
- **Data Binding**: Implement `INotifyPropertyChanged` properly
- **Navigation**: Inject `INavigationService` for page transitions
- **Example**: `MainPageViewModel`, `MoodEntryViewModel`, `SettingsViewModel`

#### Services/
**Purpose**: Business logic interfaces and their implementations
- **Interface Definition**: Define contract in `I[ServiceName].cs`
- **Implementation**: Concrete implementation in `[ServiceName].cs`
- **Registration**: Register both interface and implementation in DI container
- **Testing**: Mock interfaces for unit testing
- **Example**: `MoodDataService` implementing `IMoodDataService`

#### Models/
**Purpose**: Data models, DTOs, and value objects
- **Entities**: Core domain objects (`MoodEntry`, `User`, `Settings`)
- **DTOs**: Data transfer objects for service boundaries
- **Value Objects**: Immutable objects representing values (`MoodLevel`, `DateRange`)
- **Configuration**: Application configuration models
- **Example**: `MoodEntry.cs`, `AppSettings.cs`, `ChartDataPoint.cs`

#### Pages/
**Purpose**: XAML views and their code-behind files
- **XAML Files**: UI structure and data binding definitions
- **Code-Behind**: View-specific logic only (no business logic)
- **Navigation**: Page lifecycle management
- **Platform-Specific**: Platform-specific view implementations when needed
- **Example**: `MainPage.xaml`, `MoodEntryPage.xaml`, `SettingsPage.xaml`

#### Shims/
**Purpose**: Dependency abstractions using Shim Factory Pattern
- **Interface Definition**: Abstract external dependencies (`IFileShim`, `IPaintShim`)
- **Implementation**: Concrete wrappers around framework APIs
- **Factory Pattern**: Create objects through factory interfaces
- **Testing**: Enable complete isolation and mocking
- **Example**: `FileShim.cs`, `DrawingShims.cs`, `DateShim.cs`

#### Infrastructure/
**Purpose**: Base classes, utilities, and cross-cutting concerns
- **Base Classes**: `BaseViewModel`, `BaseCommand`, `BaseService`
- **Commands**: Reusable command implementations
- **Extensions**: Extension methods for common operations
- **Helpers**: Utility classes and helper methods
- **Example**: `BaseViewModel.cs`, `RelayCommand.cs`, `StringExtensions.cs`

#### Converters/
**Purpose**: XAML value converters for data transformation in bindings
- **Boolean Converters**: `BoolToVisibilityConverter`, `InvertBoolConverter`
- **Value Converters**: `EnumToStringConverter`, `DateToStringConverter`
- **Complex Converters**: Multi-value converters for complex scenarios
- **Platform-Specific**: Converters for platform-specific formatting
- **Example**: `MoodLevelToColorConverter.cs`, `DateTimeToRelativeStringConverter.cs`

#### Strategies/
**Purpose**: Strategy pattern implementations for configurable behavior
- **Algorithm Selection**: Different algorithms for same operation
- **Configuration-Driven**: Runtime strategy selection based on settings
- **Testability**: Easy to test different behavioral scenarios
- **Extensibility**: Add new strategies without modifying existing code
- **Example**: `MoodAnalysisStrategy`, `NotificationStrategy`, `ExportStrategy`

#### Processors/
**Purpose**: Data processing and transformation logic
- **Input Processing**: Transform user input into domain objects
- **Data Analysis**: Analyze mood data for insights and trends
- **Export Processing**: Prepare data for various export formats
- **Validation**: Complex validation logic for business rules
- **Example**: `MoodDataProcessor`, `TrendAnalysisProcessor`, `ExportDataProcessor`

#### Graphics/
**Purpose**: Custom drawing using SkiaSharp for charts and visualizations
- **Chart Rendering**: Line graphs, bar charts, mood visualizations
- **Custom Controls**: SkiaSharp-based custom UI elements
- **Drawing Abstractions**: Abstracted through shim pattern
- **Performance**: Optimized drawing routines for smooth animations
- **Example**: `LineGraphRenderer`, `MoodChartDrawer`, `CustomCanvasView`

#### Adapters/
**Purpose**: Data transformation between different representations
- **API Adapters**: Transform between internal models and external APIs
- **Format Adapters**: Convert between different data formats
- **Legacy Adapters**: Handle legacy data format compatibility
- **Platform Adapters**: Platform-specific data adaptations
- **Example**: `MoodEntryAdapter`, `JsonDataAdapter`, `LegacyFormatAdapter`

#### Factories/
**Purpose**: Object creation patterns and factory implementations
- **Service Factories**: Create services with complex initialization
- **Model Factories**: Create domain objects with validation
- **Shim Factories**: Create shim objects for dependency abstraction
- **Configuration Factories**: Create configured objects based on settings
- **Example**: `MoodEntryFactory`, `ChartConfigurationFactory`, `DrawingShimFactory`

## Core Service Interfaces

### Essential Business Services

#### IMoodDataService
**Purpose**: Mood data CRUD operations and persistence
```csharp
public interface IMoodDataService
{
    Task<IEnumerable<MoodEntry>> GetMoodEntriesAsync(DateRange range);
    Task<MoodEntry> CreateMoodEntryAsync(MoodEntry entry);
    Task<MoodEntry> UpdateMoodEntryAsync(MoodEntry entry);
    Task<bool> DeleteMoodEntryAsync(int entryId);
    Task<MoodStatistics> GetStatisticsAsync(DateRange range);
}
```

#### IMoodVisualizationService  
**Purpose**: Graph generation and data visualization coordination
```csharp
public interface IMoodVisualizationService
{
    Task<ChartConfiguration> GenerateLineChartConfigAsync(IEnumerable<MoodEntry> data);
    Task<ChartConfiguration> GenerateTrendChartConfigAsync(DateRange range);
    Task<byte[]> ExportChartAsPngAsync(ChartConfiguration config);
    Task<string> GenerateChartSummaryAsync(ChartConfiguration config);
}
```

#### ILineGraphService
**Purpose**: SkiaSharp-based line graph rendering engine
```csharp
public interface ILineGraphService
{
    Task<SKBitmap> RenderLineGraphAsync(LineGraphConfiguration config);
    void ConfigureGraphStyle(GraphStyleSettings settings);
    Task<SKRect> CalculateOptimalBoundsAsync(IEnumerable<DataPoint> data);
    void RegisterCustomRenderer(string renderType, ICustomRenderer renderer);
}
```

#### IScheduleConfigService
**Purpose**: Reminder scheduling and notification configuration
```csharp
public interface IScheduleConfigService
{
    Task<ScheduleConfiguration> GetCurrentScheduleAsync();
    Task<bool> UpdateScheduleAsync(ScheduleConfiguration schedule);
    Task<IEnumerable<NotificationTime>> GetUpcomingNotificationsAsync();
    Task<bool> EnableScheduleAsync();
    Task<bool> DisableScheduleAsync();
}
```

#### INavigationService
**Purpose**: Page routing and navigation management
```csharp
public interface INavigationService
{
    Task NavigateToAsync(string pageName, Dictionary<string, object>? parameters = null);
    Task NavigateBackAsync();
    Task NavigateToRootAsync();
    Task<bool> CanNavigateBackAsync();
    void RegisterPage<TPage>(string pageName) where TPage : Page;
}
```

#### IDataArchiveService
**Purpose**: Backup, export, and data portability operations
```csharp
public interface IDataArchiveService
{
    Task<ArchiveResult> CreateBackupAsync(BackupConfiguration config);
    Task<ImportResult> ImportDataAsync(Stream dataStream, ImportFormat format);
    Task<ExportResult> ExportDataAsync(ExportConfiguration config);
    Task<bool> ValidateArchiveIntegrityAsync(Stream archiveStream);
}
```

#### ILoggingService
**Purpose**: Application diagnostics, telemetry, and error logging
```csharp
public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, Exception? exception = null);
    void LogError(string message, Exception exception);
    void LogDebug(string message, params object[] args);
    Task FlushLogsAsync();
}
```

### Shim Abstractions (Dependency Injection Ready)

#### File System Abstractions
**Purpose**: Abstract file system operations for testability

**IFileShim** - Individual file operations
- File reading/writing (text and binary)
- File existence checks and metadata
- Async operations for large files

**IFolderShim** - Directory operations  
- Directory creation and deletion
- File enumeration and searching
- Path manipulation and validation

**IFileShimFactory** - Factory for file operations
- Create file and folder shims
- Platform-specific path handling
- Permission and access validation

#### Drawing Abstractions (SkiaSharp)
**Purpose**: Abstract SkiaSharp drawing operations (see `DrawingShims.cs`)

**IPaintShim** - Paint object abstraction
- Color, style, and effect configuration
- Anti-aliasing and quality settings
- Resource management and disposal

**IColorShim** - Color system abstraction  
- Color creation and manipulation
- Platform-specific color representations
- Accessibility and contrast handling

**IPathEffectShim** - Path effects abstraction
- Line styles and dash patterns  
- Shadow and blur effects
- Custom path transformations

#### Utility Abstractions
**Purpose**: Abstract common framework dependencies

**IDateShim** - Date/time operations
- Current date/time access
- Time zone handling
- Date formatting and parsing

**IJsonSerializerShim** - JSON serialization
- Object to JSON conversion
- Custom serialization settings
- Error handling and validation

**IBrowserShim** - Web browser operations
- Launch URLs in system browser
- Handle web-based authentication
- Platform-specific browser selection

## Architecture Patterns & Best Practices

### Dependency Injection Pattern
```csharp
// Service Registration (MauiProgram.cs)
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Register services
        builder.Services.AddSingleton<IMoodDataService, MoodDataService>();
        builder.Services.AddTransient<IMoodEntryViewModel, MoodEntryViewModel>();
        
        // Register factories
        builder.Services.AddSingleton<IFileShimFactory, FileShimFactory>();
        
        return builder.Build();
    }
}

// Service Consumption (ViewModel)
public class MoodEntryViewModel : BaseViewModel
{
    private readonly IMoodDataService _dataService;
    private readonly INavigationService _navigationService;
    
    public MoodEntryViewModel(
        IMoodDataService dataService, 
        INavigationService navigationService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }
}
```

### MVVM Data Binding Pattern
```csharp
// ViewModel Implementation
public class MainPageViewModel : BaseViewModel
{
    private string _currentMood = string.Empty;
    
    public string CurrentMood
    {
        get => _currentMood;
        set => SetProperty(ref _currentMood, value);
    }
    
    public ICommand SaveMoodCommand { get; }
    
    public MainPageViewModel(IMoodDataService dataService)
    {
        SaveMoodCommand = new AsyncRelayCommand(SaveMoodAsync);
    }
    
    private async Task SaveMoodAsync()
    {
        // Business logic implementation
    }
}

// XAML Binding
<Entry Text="{Binding CurrentMood}" />
<Button Text="Save" Command="{Binding SaveMoodCommand}" />
```

### Factory Pattern Implementation
```csharp
// Factory Interface
public interface IDrawingShimFactory
{
    IPaintShim CreatePaint(PaintShimArgs args);
    IColorShim CreateColor(byte r, byte g, byte b, byte a = 255);
    ICanvasShim CreateCanvas(SKCanvas canvas);
}

// Factory Usage in Service
public class LineGraphService : ILineGraphService
{
    private readonly IDrawingShimFactory _drawingFactory;
    
    public LineGraphService(IDrawingShimFactory drawingFactory)
    {
        _drawingFactory = drawingFactory;
    }
    
    public async Task<SKBitmap> RenderLineGraphAsync(LineGraphConfiguration config)
    {
        using var paint = _drawingFactory.CreatePaint(new PaintShimArgs 
        { 
            Color = _drawingFactory.CreateColor(255, 255, 255),
            Style = SKPaintStyle.Fill 
        });
        
        // Rendering implementation using abstracted dependencies
    }
}
```

## Testing Considerations

### Test Project Structure
Mirror the main application structure in test projects:
```
WorkMood.MauiApp.Tests/
├── ViewModels/          # ViewModel unit tests
├── Services/            # Service integration tests  
├── Models/              # Model validation tests
├── Shims/               # Shim implementation tests
├── TestHelpers/         # Shared test utilities and mocks
└── Integration/         # Full integration test scenarios
```

### Mocking Strategy
- **Mock Interfaces**: Use mocking frameworks (Moq, NSubstitute) for service interfaces
- **Shim Testing**: Shims enable complete isolation of external dependencies
- **Test Factories**: Create test-specific factories for predictable object creation
- **Data Builders**: Use builder pattern for test data creation

### Architecture Validation Tests
- **Dependency Direction**: Ensure dependencies point toward abstractions
- **Circular Dependencies**: Validate no circular references exist
- **Interface Segregation**: Verify interfaces follow single responsibility
- **MVVM Compliance**: Ensure ViewModels don't reference UI directly

---

*This codex serves as the definitive guide for architectural decisions and code organization within the WorkMood application. All new code should align with these patterns and principles.*