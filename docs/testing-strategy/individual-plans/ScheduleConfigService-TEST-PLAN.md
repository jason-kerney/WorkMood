# ScheduleConfigService Test Plan

## Overview

### Object Under Test

**Target**: `ScheduleConfigService`
**File**: `MauiApp/Services/ScheduleConfigService.cs` (189 lines)
**Type**: Data persistence service implementing IScheduleConfigService interface  
**Current Coverage**: 0% (Source: CoverageReport/Summary.txt)
**Target Coverage**: 90%+

### Current Implementation Analysis

`ScheduleConfigService` is a critical data persistence service that manages work schedule configurations through JSON file storage. It handles schedule persistence, caching, automatic cleanup of old overrides, and provides a clean abstraction over file I/O operations for schedule management.

**Key Characteristics**:
- **File-Based Persistence**: JSON serialization to local application data directory
- **Caching Layer**: In-memory caching with cache invalidation support
- **Configuration Management**: Default values, overrides, and automatic cleanup
- **Error Resilience**: Comprehensive exception handling with fallback to defaults
- **Schedule Logic**: Integration with ScheduleConfig and ScheduleOverride models

## Section 1: Class Structure Analysis

### Interface Definition
```csharp
public interface IScheduleConfigService
{
    Task<ScheduleConfig> LoadScheduleConfigAsync();
    Task SaveScheduleConfigAsync(ScheduleConfig config);
    Task<ScheduleConfig> UpdateScheduleConfigAsync(TimeSpan morningTime, TimeSpan eveningTime, ScheduleOverride? newOverride = null);
}
```

### Constructor Dependencies
```csharp
public ScheduleConfigService(ILoggingService? loggingService = null)
{
    _loggingService = loggingService ?? new LoggingService();
    // File path setup and JSON options configuration
}
```

**Dependencies**:
- `ILoggingService` - Optional logging service (creates default if null)

### Public Interface Implementation
```csharp
// Core Operations
public async Task<ScheduleConfig> LoadScheduleConfigAsync()
public async Task SaveScheduleConfigAsync(ScheduleConfig config)
public async Task<ScheduleConfig> UpdateScheduleConfigAsync(TimeSpan morningTime, TimeSpan eveningTime, ScheduleOverride? newOverride = null)

// Cache Management (Not in interface - internal methods)
public ScheduleConfig? GetCachedConfig()
public void ClearCache()
```

### Private Implementation
```csharp
// Logging
private void Log(string message)

// Internal Fields
private readonly string _configFilePath;
private readonly JsonSerializerOptions _jsonOptions;  
private readonly ILoggingService _loggingService;
private ScheduleConfig? _cachedConfig;
```

### Dependencies Analysis
- **File System Operations**: `Environment.GetFolderPath()`, `Directory.CreateDirectory()`, `File.Exists()`, `File.ReadAllTextAsync()`, `File.WriteAllTextAsync()`
- **JSON Serialization**: `JsonSerializer.Serialize()`, `JsonSerializer.Deserialize()`
- **Static DateTime**: `DateTime.Today` for override cleanup
- **ILoggingService**: Abstracted logging interface (good for testing)

## Section 2: Testability Assessment

### Testability Score: 4/10 ⚠️ **REQUIRES SIGNIFICANT REFACTORING**

**Major Testability Issues**:
- ❌ **Hard File System Dependencies**: Direct File.* and Directory.* calls (not abstracted)
- ❌ **Static Environment Calls**: `Environment.GetFolderPath()` not injectable  
- ❌ **Static DateTime Dependencies**: `DateTime.Today` calls in override cleanup
- ❌ **Fixed File Path**: Constructor creates fixed file path in LocalApplicationData
- ❌ **JSON Serialization**: Direct JsonSerializer calls (not abstracted)

**Moderate Issues**:
- ⚠️ **Error Handling**: Exception swallowing makes some failure paths untestable
- ⚠️ **Caching Logic**: Private cache state difficult to verify in tests
- ⚠️ **Path Construction**: Complex path building logic not easily testable

**Good Architecture Elements**:
- ✅ **Interface-Based Design**: IScheduleConfigService enables clean mocking
- ✅ **ILoggingService Abstraction**: Proper dependency injection for logging
- ✅ **Optional Constructor Parameters**: Default logging service creation
- ✅ **Separation of Concerns**: Clear separation between file operations and business logic
- ✅ **Cache Management**: Explicit cache control methods

## Section 3: Required Refactoring Analysis

### Refactoring Requirements: SIGNIFICANT - Major Architectural Changes Needed ⚠️

**Critical Refactoring Tasks**:

#### 1. Abstract File System Dependencies
```csharp
// BEFORE (Hard to test)
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
Directory.CreateDirectory(appFolder);
if (!File.Exists(_configFilePath))
await File.WriteAllTextAsync(_configFilePath, json);

// AFTER (Testable via shims)
public interface IFileSystemShim
{
    string GetLocalApplicationDataPath();
    void CreateDirectory(string path);
    bool FileExists(string path);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
}

private readonly IFileSystemShim _fileSystemShim;
```

#### 2. Abstract JSON Serialization
```csharp
// BEFORE (Hard to test)
var config = JsonSerializer.Deserialize<ScheduleConfig>(json, _jsonOptions);
var json = JsonSerializer.Serialize(config, _jsonOptions);

// AFTER (Testable via shim)
public interface IJsonSerializerShim
{
    T? Deserialize<T>(string json, JsonSerializerOptions? options = null);
    string Serialize<T>(T value, JsonSerializerOptions? options = null);
}

private readonly IJsonSerializerShim _jsonSerializerShim;
```

#### 3. Abstract DateTime Dependencies
```csharp
// BEFORE (Hard to test)
var today = DateOnly.FromDateTime(DateTime.Today);

// AFTER (Testable via shim)
private readonly IDateShim _dateShim;
var today = _dateShim.GetTodayDate();
```

#### 4. Update Constructor for Dependency Injection
```csharp
// BEFORE (Hard to test)
public ScheduleConfigService(ILoggingService? loggingService = null)

// AFTER (Testable)
public ScheduleConfigService(
    IFileSystemShim fileSystemShim,
    IJsonSerializerShim jsonSerializerShim,
    IDateShim dateShim,
    ILoggingService? loggingService = null)
{
    _fileSystemShim = fileSystemShim ?? throw new ArgumentNullException(nameof(fileSystemShim));
    _jsonSerializerShim = jsonSerializerShim ?? throw new ArgumentNullException(nameof(jsonSerializerShim));
    _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
    _loggingService = loggingService ?? new LoggingService();
}

// Convenience constructor for existing usage
public ScheduleConfigService(ILoggingService? loggingService = null)
    : this(new FileSystemShim(), new JsonSerializerShim(), new DateShim(), loggingService)
{
}
```

### Required Interface Extractions

#### IFileSystemShim Interface
```csharp
public interface IFileSystemShim
{
    string GetLocalApplicationDataPath();
    void CreateDirectory(string path);
    bool FileExists(string path);
    Task<string> ReadAllTextAsync(string path);
    Task WriteAllTextAsync(string path, string content);
    string CombinePath(params string[] paths);
}
```

#### IJsonSerializerShim Interface
```csharp
public interface IJsonSerializerShim
{
    T? Deserialize<T>(string json, JsonSerializerOptions? options = null);
    string Serialize<T>(T value, JsonSerializerOptions? options = null);
}
```

### Refactoring Priority: **CRITICAL**
This service cannot be effectively tested without abstracting file system and serialization dependencies.

## Section 4: Test Strategy (Post-Refactoring)

### Testing Approach

After refactoring to inject dependencies, focus on:

1. **Constructor Testing**: Dependency validation and file path setup
2. **Load Operations**: File existence, deserialization, caching, error handling
3. **Save Operations**: Serialization, file writing, cache updates, error propagation  
4. **Update Operations**: Complex business logic with override management and cleanup
5. **Cache Management**: Cache behavior, invalidation, and state verification
6. **Error Scenarios**: File I/O failures, JSON parsing errors, permission issues

### Test Categories

#### 4.1 Constructor and Initialization Tests
- **Valid Dependencies**: All required shims provided
- **Null Dependency Validation**: ArgumentNullException for null shims
- **Optional Logging Service**: Default service creation when null
- **File Path Setup**: Verify correct application data path construction
- **Directory Creation**: Ensure app folder creation

#### 4.2 Load Configuration Tests
- **First Load - File Exists**: Successful deserialization and caching
- **First Load - File Missing**: Default configuration creation
- **First Load - Empty File**: Default configuration for empty JSON
- **Cached Load**: Return cached configuration without file access
- **Malformed JSON**: Error handling and default fallback
- **File I/O Exceptions**: Error handling with default configuration
- **Cache Population**: Verify cache state after successful load

#### 4.3 Save Configuration Tests  
- **Successful Save**: JSON serialization and file writing
- **Cache Update**: Verify cache updated with saved configuration
- **File I/O Exception**: Exception propagation (does not swallow)
- **Serialization Exception**: Exception propagation 
- **Directory Permissions**: Handling write permission failures

#### 4.4 Update Configuration Tests
- **Basic Update**: Morning/evening time updates without overrides
- **Update with New Override**: Adding new override and verifying persistence
- **Override Cleanup**: Automatic removal of past overrides
- **Multiple Overrides**: Handling multiple active overrides
- **Complex Scenarios**: Combining updates with cleanup operations

#### 4.5 Cache Management Tests
- **GetCachedConfig**: Return current cached configuration or null
- **ClearCache**: Null cache and force reload on next access
- **Cache State Verification**: Verify cache consistency across operations

#### 4.6 Error Handling and Edge Case Tests
- **JSON Parsing Failures**: Malformed JSON handling
- **File Access Denied**: Permission error scenarios
- **Disk Full**: File write failure handling
- **Null Configuration**: Passing null config to save operation
- **DateTime Edge Cases**: Override cleanup with edge dates

## Section 5: Test Implementation Strategy (Post-Refactoring)

### Test File Structure
```
WorkMood.MauiApp.Tests/
└── Services/
    └── ScheduleConfigServiceTests.cs
```

### Test Class Organization
```csharp
[TestClass]
public class ScheduleConfigServiceTests
{
    private Mock<IFileSystemShim> _mockFileSystemShim = null!;
    private Mock<IJsonSerializerShim> _mockJsonSerializerShim = null!;
    private Mock<IDateShim> _mockDateShim = null!;
    private Mock<ILoggingService> _mockLoggingService = null!;
    private ScheduleConfigService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockFileSystemShim = new Mock<IFileSystemShim>();
        _mockJsonSerializerShim = new Mock<IJsonSerializerShim>();
        _mockDateShim = new Mock<IDateShim>();
        _mockLoggingService = new Mock<ILoggingService>();
        
        // Setup default behaviors
        _mockFileSystemShim.Setup(x => x.GetLocalApplicationDataPath()).Returns("/test/appdata");
        _mockFileSystemShim.Setup(x => x.CombinePath(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join("/", paths));
        _mockDateShim.Setup(x => x.GetTodayDate()).Returns(new DateOnly(2024, 6, 15));
        
        _service = CreateService();
    }

    private ScheduleConfigService CreateService()
    {
        return new ScheduleConfigService(
            _mockFileSystemShim.Object,
            _mockJsonSerializerShim.Object,
            _mockDateShim.Object,
            _mockLoggingService.Object
        );
    }
}
```

### Mock Strategy
- **IFileSystemShim**: Mock all file operations for controlled testing
- **IJsonSerializerShim**: Mock JSON operations for serialization testing
- **IDateShim**: Mock date operations for predictable time-based tests
- **ILoggingService**: Mock logging calls for verification
- **ScheduleConfig/ScheduleOverride**: Use real model objects in tests

## Section 6: Detailed Test Specifications (Post-Refactoring)

### 6.1 Constructor Tests

#### Test: Valid Dependencies
```csharp
[TestMethod]
public void Constructor_WithValidDependencies_ShouldInitializeCorrectly()
{
    // Arrange & Act
    var service = CreateService();
    
    // Assert - Service should initialize without throwing
    Assert.IsNotNull(service);
    
    // Verify directory creation
    _mockFileSystemShim.Verify(x => x.CreateDirectory("/test/appdata/WorkMood"), Times.Once);
}
```

#### Test: Null Dependency Validation
```csharp
[TestMethod]
[DataRow("fileSystemShim")]
[DataRow("jsonSerializerShim")]
[DataRow("dateShim")]
public void Constructor_WithNullDependency_ShouldThrowArgumentNullException(string nullDependency)
{
    // Arrange
    var fileSystemShim = nullDependency == "fileSystemShim" ? null : _mockFileSystemShim.Object;
    var jsonSerializerShim = nullDependency == "jsonSerializerShim" ? null : _mockJsonSerializerShim.Object;
    var dateShim = nullDependency == "dateShim" ? null : _mockDateShim.Object;
    
    // Act & Assert
    Assert.ThrowsException<ArgumentNullException>(() =>
        new ScheduleConfigService(fileSystemShim!, jsonSerializerShim!, dateShim!, _mockLoggingService.Object));
}
```

### 6.2 Load Configuration Tests

#### Test: First Load with Existing File
```csharp
[TestMethod]
public async Task LoadScheduleConfigAsync_WithExistingFile_ShouldDeserializeAndCache()
{
    // Arrange
    var expectedConfig = new ScheduleConfig(new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0));
    var json = "{\"morningTime\":\"09:00:00\",\"eveningTime\":\"18:00:00\",\"overrides\":[]}";
    
    _mockFileSystemShim.Setup(x => x.FileExists("/test/appdata/WorkMood/schedule_config.json")).Returns(true);
    _mockFileSystemShim.Setup(x => x.ReadAllTextAsync("/test/appdata/WorkMood/schedule_config.json")).ReturnsAsync(json);
    _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(json, It.IsAny<JsonSerializerOptions>()))
        .Returns(expectedConfig);
    
    // Act
    var result = await _service.LoadScheduleConfigAsync();
    
    // Assert
    Assert.AreEqual(expectedConfig.MorningTime, result.MorningTime);
    Assert.AreEqual(expectedConfig.EveningTime, result.EveningTime);
    
    // Verify caching
    var cachedResult = await _service.LoadScheduleConfigAsync();
    Assert.AreSame(result, cachedResult);
    
    // Verify file was only read once
    _mockFileSystemShim.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Once);
}
```

#### Test: First Load with Missing File
```csharp
[TestMethod]
public async Task LoadScheduleConfigAsync_WithMissingFile_ShouldReturnDefaultConfig()
{
    // Arrange
    _mockFileSystemShim.Setup(x => x.FileExists("/test/appdata/WorkMood/schedule_config.json")).Returns(false);
    
    // Act
    var result = await _service.LoadScheduleConfigAsync();
    
    // Assert
    Assert.AreEqual(new TimeSpan(8, 20, 0), result.MorningTime);
    Assert.AreEqual(new TimeSpan(17, 20, 0), result.EveningTime);
    Assert.AreEqual(0, result.Overrides.Count);
    
    // Verify no file read attempted
    _mockFileSystemShim.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Never);
}
```

#### Test: Load with JSON Exception
```csharp
[TestMethod]
public async Task LoadScheduleConfigAsync_WithJsonException_ShouldReturnDefaultConfig()
{
    // Arrange
    _mockFileSystemShim.Setup(x => x.FileExists("/test/appdata/WorkMood/schedule_config.json")).Returns(true);
    _mockFileSystemShim.Setup(x => x.ReadAllTextAsync("/test/appdata/WorkMood/schedule_config.json")).ReturnsAsync("invalid json");
    _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
        .Throws(new JsonException("Invalid JSON"));
    
    // Act
    var result = await _service.LoadScheduleConfigAsync();
    
    // Assert
    Assert.AreEqual(new TimeSpan(8, 20, 0), result.MorningTime);
    Assert.AreEqual(new TimeSpan(17, 20, 0), result.EveningTime);
    
    // Verify error was logged
    _mockLoggingService.Verify(x => x.LogDebug(It.Is<string>(s => s.Contains("Error loading schedule config"))), Times.Once);
}
```

### 6.3 Save Configuration Tests

#### Test: Successful Save
```csharp
[TestMethod]
public async Task SaveScheduleConfigAsync_WithValidConfig_ShouldSerializeAndSave()
{
    // Arrange
    var config = new ScheduleConfig(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0));
    var expectedJson = "{\"morningTime\":\"10:00:00\",\"eveningTime\":\"19:00:00\",\"overrides\":[]}";
    
    _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>())).Returns(expectedJson);
    
    // Act
    await _service.SaveScheduleConfigAsync(config);
    
    // Assert
    _mockJsonSerializerShim.Verify(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>()), Times.Once);
    _mockFileSystemShim.Verify(x => x.WriteAllTextAsync("/test/appdata/WorkMood/schedule_config.json", expectedJson), Times.Once);
    
    // Verify cache updated
    var cachedConfig = _service.GetCachedConfig();
    Assert.AreSame(config, cachedConfig);
}
```

#### Test: Save with File Exception
```csharp
[TestMethod]
public async Task SaveScheduleConfigAsync_WithFileException_ShouldPropagateException()
{
    // Arrange
    var config = new ScheduleConfig(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0));
    var expectedException = new UnauthorizedAccessException("Access denied");
    
    _mockJsonSerializerShim.Setup(x => x.Serialize(config, It.IsAny<JsonSerializerOptions>())).Returns("{}");
    _mockFileSystemShim.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(expectedException);
    
    // Act & Assert
    var actualException = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(() => _service.SaveScheduleConfigAsync(config));
    Assert.AreSame(expectedException, actualException);
    
    // Verify error was logged
    _mockLoggingService.Verify(x => x.LogDebug(It.Is<string>(s => s.Contains("Error saving schedule config"))), Times.Once);
}
```

### 6.4 Update Configuration Tests

#### Test: Update with Override and Cleanup
```csharp
[TestMethod]
public async Task UpdateScheduleConfigAsync_WithOverrideAndCleanup_ShouldUpdateAndCleanup()
{
    // Arrange
    var currentDate = new DateOnly(2024, 6, 15);
    var pastDate = new DateOnly(2024, 6, 10);
    var futureDate = new DateOnly(2024, 6, 20);
    
    _mockDateShim.Setup(x => x.GetTodayDate()).Returns(currentDate);
    
    // Setup existing config with past override
    var existingConfig = new ScheduleConfig(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
    existingConfig.SetOverride(pastDate, new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0));
    
    _mockFileSystemShim.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
    _mockFileSystemShim.Setup(x => x.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync("{}");
    _mockJsonSerializerShim.Setup(x => x.Deserialize<ScheduleConfig>(It.IsAny<string>(), It.IsAny<JsonSerializerOptions>()))
        .Returns(existingConfig);
    
    var newOverride = new ScheduleOverride(futureDate, new TimeSpan(10, 0, 0), new TimeSpan(20, 0, 0));
    
    // Act
    var result = await _service.UpdateScheduleConfigAsync(new TimeSpan(9, 30, 0), new TimeSpan(18, 30, 0), newOverride);
    
    // Assert
    Assert.AreEqual(new TimeSpan(9, 30, 0), result.MorningTime);
    Assert.AreEqual(new TimeSpan(18, 30, 0), result.EveningTime);
    
    // Verify past override was cleaned up and new override was added
    Assert.AreEqual(1, result.Overrides.Count);
    Assert.AreEqual(futureDate, result.Overrides[0].Date);
    
    // Verify save was called
    _mockFileSystemShim.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
}
```

### 6.5 Cache Management Tests

#### Test: Cache Operations
```csharp
[TestMethod]
public void GetCachedConfig_WhenNoCache_ShouldReturnNull()
{
    // Act
    var result = _service.GetCachedConfig();
    
    // Assert
    Assert.IsNull(result);
}

[TestMethod]
public async Task ClearCache_ShouldInvalidateCacheAndForceReload()
{
    // Arrange
    _mockFileSystemShim.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
    
    // Load config to populate cache
    await _service.LoadScheduleConfigAsync();
    Assert.IsNotNull(_service.GetCachedConfig());
    
    // Act
    _service.ClearCache();
    
    // Assert
    Assert.IsNull(_service.GetCachedConfig());
    
    // Verify next load reads from file again
    await _service.LoadScheduleConfigAsync();
    // (File operations would be called again)
}
```

## Section 7: Implementation Checklist

### Pre-Implementation Tasks (CRITICAL REFACTORING REQUIRED)
- [ ] **Create IFileSystemShim Interface**: Abstract file system operations
- [ ] **Create IJsonSerializerShim Interface**: Abstract JSON serialization
- [ ] **Implement FileSystemShim**: Concrete implementation wrapping System.IO operations
- [ ] **Implement JsonSerializerShim**: Concrete implementation wrapping System.Text.Json
- [ ] **Update ScheduleConfigService Constructor**: Add shim-based dependency injection
- [ ] **Update All Service Methods**: Use shims instead of direct static calls
- [ ] **Update MauiProgram.cs**: Register new service interfaces in DI container

### Implementation Tasks (Post-Refactoring)
- [ ] **Create Test Class**: ScheduleConfigServiceTests with comprehensive mock setup
- [ ] **Constructor Tests**: Dependency validation, null checks, initialization verification
- [ ] **Load Tests**: File operations, caching behavior, error handling scenarios
- [ ] **Save Tests**: Serialization, file writing, cache updates, exception propagation
- [ ] **Update Tests**: Complex business logic with override management and cleanup
- [ ] **Cache Tests**: Cache state verification, invalidation, consistency
- [ ] **Error Handling Tests**: JSON failures, file I/O errors, permission issues

### Validation Tasks
- [ ] **Build Verification**: All refactoring compiles successfully
- [ ] **Integration Testing**: File operations work with real file system shim
- [ ] **Mock Verification**: All dependency interactions properly mocked
- [ ] **Coverage Verification**: Achieve 90%+ coverage after refactoring
- [ ] **Performance Validation**: File I/O performance not degraded by abstractions

### Documentation Tasks
- [ ] **Refactoring Documentation**: Document shim interfaces and rationale
- [ ] **Testing Patterns**: Document file I/O testing patterns for future use
- [ ] **Architecture Notes**: Document data persistence abstraction layer

## Test Implementation Estimate

**Complexity**: High (Data persistence service with significant refactoring required)
**Refactoring Time**: 6-10 hours (shim interface creation, file system abstraction)
**Testing Time**: 5-7 hours (comprehensive persistence testing with mocks)
**Total Estimate**: 11-17 hours
**Estimated Test Count**: 20-30 tests
**Expected Coverage**: 90%+ (after refactoring enables proper testing)

**Implementation Priority**: High (Critical data persistence service)
**Risk Level**: High (Major refactoring required, core data operations)

**Key Success Factors**:
- Successful file system abstraction without breaking existing functionality
- Comprehensive error handling testing for all file I/O scenarios
- Proper async testing patterns for file operations
- Clean abstraction layer that maintains existing usage patterns
- Thorough cache behavior verification

---

## Commit Strategy (Arlo's Commit Notation)

### Phase 1: Refactoring for Testability
```
^r - extract IFileSystemShim interface for file system operation abstraction
^r - extract IJsonSerializerShim interface for JSON serialization abstraction
^r - implement FileSystemShim wrapper for System.IO operations
^r - implement JsonSerializerShim wrapper for System.Text.Json operations
^r - update ScheduleConfigService constructor with shim-based dependency injection
^r - update ScheduleConfigService methods to use shim interfaces instead of static calls
^r - register new shim interfaces in MauiProgram dependency injection container
```

### Phase 2: Test Implementation
```
^f - add comprehensive ScheduleConfigService tests with 90% coverage

- Constructor tests: dependency validation, null checks, file path setup, directory creation
- Load tests: file existence scenarios, JSON deserialization, caching behavior, error handling
- Save tests: JSON serialization, file writing, cache updates, exception propagation
- Update tests: complex business logic with override management, cleanup operations, persistence
- Cache tests: GetCachedConfig, ClearCache, cache state verification and consistency
- Error handling tests: JSON parsing failures, file I/O exceptions, permission errors
- Mock verification: all shim interactions properly tested via interface abstractions
- Data persistence service managing schedule configuration through JSON file storage
```

**Risk Assessment**: `^` (Validated) - Data persistence service requiring significant refactoring for testability, but comprehensive test coverage planned with proper file system and serialization abstractions and async testing patterns.

**Testing Confidence**: Medium-High - After refactoring, the service becomes highly testable through shim interfaces. All file operations and business logic will be thoroughly verified through controlled mocks. 🤖