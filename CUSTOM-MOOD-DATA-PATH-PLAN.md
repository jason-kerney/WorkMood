# WorkMood: Custom Mood Data Path Implementation Plan

**Objective**: Allow users to choose a custom directory for storing `mood_data.json`. When user selects a new path, migrate the data file to that location.

**Scope**: Configuration storage remains unchanged (always `Documents/WorkMood/`). Only mood data file location is customizable.

---

## Mandatory Pre-flight: Read Before Touching

Before modifying any file, read its current contents in full. This plan was authored with knowledge of the codebase at a point in time; the implementation may have drifted. Read each file you are about to modify and reconcile these instructions with the actual code.

**Files to read before Part 1:**
- `MauiApp/Models/ScheduleConfig.cs`

**Files to read before Part 2:**
- `MauiApp/Shims/IFolderShim.cs`
- `MauiApp/Shims/IFileShim.cs`
- `MauiApp/Shims/FolderShim.cs`
- `MauiApp/Shims/FileShim.cs`

**Files to read before Part 3:**
- `MauiApp/Services/IMoodDataService.cs`
- `MauiApp/Services/IScheduleConfigService.cs`

**Files to read before Part 4:**
- `MauiApp/Services/MoodDataService.cs` (full file — constructor signature, `_dataFilePath` field, and `LoadMoodDataAsync` are especially important)
- `WorkMood.MauiApp.Tests/Services/MoodDataServiceShould.cs` (all existing tests — do NOT break them)

**Files to read before Part 5:**
- `MauiApp/MauiProgram.cs` (full file — existing DI registrations for `MoodDataService` and `IScheduleConfigService`)

**Files to read before Part 6:**
- `MauiApp/Infrastructure/ViewModelBase.cs`
- `MauiApp/Infrastructure/RelayCommand.cs`

**Files to read before Part 7:**
- `MauiApp/Pages/Settings.xaml`
- `MauiApp/Pages/Settings.xaml.cs`
- `MauiApp/ViewModels/SettingsPageViewModel.cs`

---

## Anti-Patterns: Never Do These

- **NEVER** use `File`, `Directory`, `Path.Combine`, or `Environment.SpecialFolder` directly in `MoodDataService` or `StorageSettingsViewModel`. Use the shim interfaces (`IFolderShim`, `IFileShim`, `IPathValidationShim`, `IFolderPickerShim`).
- **NEVER** use `async void` for methods that caller code can await. Only use `async void` for `RelayCommand` handlers (event-style, fire-and-forget).
- **NEVER** use `IAsyncRelayCommand` — it does not exist in this codebase. Use `RelayCommand` wrapping an async lambda instead.
- **NEVER** call `FilePicker.PickFolder()` — it does not exist in MAUI. The folder picker API is `FolderPicker.Default.PickAsync()`.
- **NEVER** move the file (cut + paste). Always copy then conditionally delete (see Part 4, migration algorithm).
- **NEVER** modify the existing 6-parameter `MoodDataService` constructor signature or its behavior — existing tests depend on it.
- **NEVER** add `IScheduleConfigService` to the existing 6-parameter constructor.
- **NEVER** batch multiple phases into a single change. Complete each phase, verify build, then proceed.

---

## Part 1: Model Change — Add CustomMoodDataPath

**File to modify**: `MauiApp/Models/ScheduleConfig.cs`

**READ THE FILE FIRST.**

**Change**: Add the following property to the `ScheduleConfig` class body (before the constructors):

```csharp
[JsonPropertyName("customMoodDataPath")]
public string? CustomMoodDataPath { get; set; }
```

**Constraints**:
- Property type MUST be `string?` (nullable). Default is `null`.
- `null` means "use the default Documents folder". Empty string also means "use the default".
- `[JsonPropertyName]` attribute is required to match the casing convention of existing properties in this class.
- Do NOT add this property to any constructor parameter list.

**Verification**:
- [ ] Build succeeds: `dotnet build WorkMood.sln`
- [ ] All existing tests pass: `dotnet test`

---

## Part 2: New Shims

### 2.1 IPathValidationShim

**File to create**: `MauiApp/Shims/IPathValidationShim.cs`

```csharp
namespace WorkMood.MauiApp.Shims;

public interface IPathValidationShim
{
    /// <summary>
    /// Returns true if path is absolute (rooted) and contains no invalid characters.
    /// Returns false if path is null, empty, relative, or contains invalid path characters.
    /// Does NOT check whether the path exists or is writable.
    /// </summary>
    bool IsAbsolutePath(string? path);

    /// <summary>
    /// Attempts to write and delete a temporary file inside path.
    /// Returns true if write succeeded and cleanup succeeded.
    /// Returns false if the write attempt throws any exception.
    /// Does NOT throw on permission failure — returns false instead.
    /// </summary>
    bool HasWritePermission(string path);
}
```

**File to create**: `MauiApp/Shims/PathValidationShim.cs`

```csharp
namespace WorkMood.MauiApp.Shims;

public class PathValidationShim : IPathValidationShim
{
    public bool IsAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        try { return Path.IsPathRooted(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0; }
        catch { return false; }
    }

    public bool HasWritePermission(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $".workmood_write_test_{Guid.NewGuid()}");
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

**Constraint**: `PathValidationShim` is the ONLY shim implementation that may call `File` or `Path` directly. All other classes use shim interfaces.

### 2.2 IFolderPickerShim

**File to create**: `MauiApp/Shims/IFolderPickerShim.cs`

```csharp
namespace WorkMood.MauiApp.Shims;

public interface IFolderPickerShim
{
    /// <summary>
    /// Opens a folder picker dialog. Returns the selected absolute folder path,
    /// or null if the user cancelled the dialog.
    /// </summary>
    Task<string?> PickFolderAsync();
}
```

**File to create**: `MauiApp/Shims/FolderPickerShim.cs`

```csharp
namespace WorkMood.MauiApp.Shims;

public class FolderPickerShim : IFolderPickerShim
{
    public async Task<string?> PickFolderAsync()
    {
        var result = await FolderPicker.Default.PickAsync(CancellationToken.None);
        if (result.IsSuccessful)
            return result.Folder.Path;
        return null;
    }
}
```

**Constraint**: `FolderPicker.Default.PickAsync()` is a MAUI API. Do NOT call it from a non-UI thread. It will be called from the ViewModel via a `RelayCommand` handler, which runs on the UI thread — this is correct.

**Verification**:
- [ ] Build succeeds: `dotnet build WorkMood.sln`
- [ ] All existing tests pass: `dotnet test`

---

## Part 3: Update IMoodDataService Interface

**File to modify**: `MauiApp/Services/IMoodDataService.cs`

**READ THE FILE FIRST.**

Add the following two method signatures to the interface body. Add them after the last existing method:

```csharp
/// <summary>
/// Returns the absolute directory path where mood_data.json is currently stored.
/// </summary>
string GetMoodDataDirectory();

/// <summary>
/// Copies mood_data.json to newPath/mood_data.json, saves newPath to schedule config,
/// then (on success) deletes the original file. On any failure after the copy, deletes
/// the copy and leaves the original untouched.
/// Throws InvalidOperationException if newPath is not an absolute path.
/// Throws InvalidOperationException if newPath is not writable.
/// </summary>
Task MigrateMoodDataAsync(string newPath);
```

**Constraints**:
- Do NOT change the signatures of any existing interface methods.
- `GetMoodDataDirectory()` returns a `string`, not `Task<string>`. It is synchronous — it reads an in-memory field, not disk.
- `MigrateMoodDataAsync` takes the destination **directory**, not a file path. The filename `mood_data.json` is appended internally.

**Verification**:
- [ ] Build fails (expected) because `MoodDataService` does not implement the new methods yet.
- [ ] Resolve only by proceeding to Part 4.

---

## Part 4: Update MoodDataService Implementation

**Files to modify**: `MauiApp/Services/MoodDataService.cs`

**READ THE ENTIRE FILE FIRST. Pay special attention to:**
1. The field `private readonly string _dataFilePath;` — you will make it non-readonly and mutable.
2. The existing 6-parameter constructor — you MUST NOT change it.
3. The `_cachedCollection` field — migration must clear it after path change.

### 4.1 Field Change

Change the `_dataFilePath` field declaration from:

```csharp
private readonly string _dataFilePath;
```

to:

```csharp
private string _dataFilePath;
```

Do NOT change any other field declarations.

### 4.2 New Private Field for IScheduleConfigService

Add alongside the other private fields at the top of the class:

```csharp
private readonly IScheduleConfigService? _scheduleConfigService;
```

Note: `IScheduleConfigService?` is nullable because the existing 6-parameter constructor does not provide it. `MigrateMoodDataAsync` will throw if called on an instance created without the overload constructor.

### 4.3 New Constructor Overload

Add a new constructor AFTER the existing 6-parameter constructor. Do NOT modify the existing constructor.

**New constructor signature**:

```csharp
public MoodDataService(
    string initialMoodDataFilePath,
    IDataArchiveService archiveService,
    IFolderShim folderShim,
    IDateShim dateShim,
    IFileShim fileShim,
    IJsonSerializerShim jsonSerializerShim,
    ILoggingService loggingService,
    IScheduleConfigService scheduleConfigService)
```

**New constructor body** — initialize all fields the same as the existing constructor, except use `initialMoodDataFilePath` for the path instead of computing it from `_folderShim.GetDocumentsFolder()`:

```csharp
{
    _archiveService = archiveService ?? throw new ArgumentNullException(nameof(archiveService));
    _folderShim = folderShim ?? throw new ArgumentNullException(nameof(folderShim));
    _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
    _fileShim = fileShim ?? throw new ArgumentNullException(nameof(fileShim));
    _jsonSerializerShim = jsonSerializerShim ?? throw new ArgumentNullException(nameof(jsonSerializerShim));
    _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));

    Log("MoodDataService (overload): Constructor starting");

    var dir = Path.GetDirectoryName(initialMoodDataFilePath)
              ?? throw new ArgumentException("initialMoodDataFilePath must be an absolute path", nameof(initialMoodDataFilePath));

    _folderShim.CreateDirectory(dir);
    _dataFilePath = initialMoodDataFilePath;

    Log($"MoodDataService (overload): Data file path: {_dataFilePath}");

    _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new DateOnlyJsonConverter() }
    };

    Log("MoodDataService (overload): Constructor completed");
}
```

**Constraint**: The `_jsonOptions` initialization above must match EXACTLY the `_jsonOptions` initialization in the existing constructor. Read the existing constructor to confirm the converters list.

### 4.4 Implement GetMoodDataDirectory()

Add the following method to `MoodDataService`:

```csharp
public string GetMoodDataDirectory()
{
    return Path.GetDirectoryName(_dataFilePath)
           ?? _folderShim.GetDocumentsFolder();
}
```

### 4.5 Implement MigrateMoodDataAsync()

Add the following method to `MoodDataService`. The algorithm: **copy first, then conditionally delete**:

```csharp
public async Task MigrateMoodDataAsync(string newPath)
{
    if (_scheduleConfigService == null)
        throw new InvalidOperationException(
            "MigrateMoodDataAsync requires IScheduleConfigService. " +
            "Use the 8-parameter constructor overload to enable migration.");

    if (string.IsNullOrWhiteSpace(newPath) || !Path.IsPathRooted(newPath))
        throw new InvalidOperationException(
            $"newPath must be a non-empty absolute path. Received: '{newPath}'");

    var newFilePath = _folderShim.CombinePaths(newPath, "mood_data.json");

    if (string.Equals(_dataFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
    {
        Log("MigrateMoodDataAsync: newPath is the same as current path. No action taken.");
        return;
    }

    // Step 1: Ensure destination directory exists
    _folderShim.CreateDirectory(newPath);

    // Step 2: Copy file content to new location
    // IFileShim does not have a Copy method; use ReadAllTextAsync + WriteAllTextAsync
    var originalContent = _fileShim.Exists(_dataFilePath)
        ? await _fileShim.ReadAllTextAsync(_dataFilePath)
        : "[]";
    await _fileShim.WriteAllTextAsync(newFilePath, originalContent);

    // Step 3: Attempt config save. If it fails, rollback by deleting the copy.
    try
    {
        var config = await _scheduleConfigService.LoadScheduleConfigAsync();
        config.CustomMoodDataPath = newPath;
        await _scheduleConfigService.SaveScheduleConfigAsync(config);
    }
    catch (Exception ex)
    {
        Log($"MigrateMoodDataAsync: Config save failed ({ex.Message}). Rolling back copy.");
        try { if (_fileShim.Exists(newFilePath)) File.Delete(newFilePath); }
        catch (Exception rollbackEx) { Log($"MigrateMoodDataAsync: Rollback delete failed: {rollbackEx.Message}"); }
        throw new InvalidOperationException(
            $"Migration failed: could not save configuration. Original data is unchanged. Details: {ex.Message}", ex);
    }

    // Step 4: Config saved — commit migration: update path, clear cache, delete original
    var originalFilePath = _dataFilePath;
    _dataFilePath = newFilePath;
    _cachedCollection = null;

    try
    {
        if (_fileShim.Exists(originalFilePath))
            File.Delete(originalFilePath);
    }
    catch (Exception ex)
    {
        // Original delete failed. Both copies exist, but config points to new location. Non-fatal.
        Log($"MigrateMoodDataAsync: Could not delete original at '{originalFilePath}': {ex.Message}. Non-fatal.");
    }

    Log($"MigrateMoodDataAsync: Migration complete. Active path is now '{_dataFilePath}'.");
}
```

**Constraints**:
- Step 2 uses shim methods (`_fileShim.ReadAllTextAsync`, `_fileShim.WriteAllTextAsync`). Do NOT use `File.Copy` here.
- Steps 3 and 4 are strictly ordered: **config save must succeed before the original is deleted**.
- `File.Delete` in steps 3 (rollback) and 4 (commit) is acceptable because `IFileShim` does not expose a delete method. If `IFileShim` gains a `DeleteAsync` method, update these calls.

**Verification**:
- [ ] Build succeeds: `dotnet build WorkMood.sln`
- [ ] All existing tests pass: `dotnet test` (existing tests use the 6-parameter constructor and must still pass unchanged)

---

## Part 5: Register New Dependencies in DI

**File to modify**: `MauiApp/MauiProgram.cs`

**READ THE ENTIRE FILE FIRST. Locate the existing `MoodDataService` DI registration block.**

### 5.1 Register IPathValidationShim

Add the following in the "Register core services" block, alongside the other shim registrations:

```csharp
builder.Services.AddSingleton<IPathValidationShim, PathValidationShim>();
```

### 5.2 Register IFolderPickerShim

Add:

```csharp
builder.Services.AddSingleton<IFolderPickerShim, FolderPickerShim>();
```

### 5.3 Replace MoodDataService DI Registration

Find the existing `builder.Services.AddSingleton<MoodDataService>(...)` lambda. Replace its body with the version below that uses the new 8-parameter overload constructor. Do NOT remove the `builder.Services.AddSingleton<IMoodDataService>(...)` line that follows it.

**New registration body**:

```csharp
builder.Services.AddSingleton<MoodDataService>(serviceProvider =>
{
    var archiveService = serviceProvider.GetRequiredService<IDataArchiveService>();
    var folderShim = serviceProvider.GetRequiredService<IFolderShim>();
    var dateShim = serviceProvider.GetRequiredService<IDateShim>();
    var fileShim = serviceProvider.GetRequiredService<IFileShim>();
    var jsonSerializerShim = serviceProvider.GetRequiredService<IJsonSerializerShim>();
    var loggingService = serviceProvider.GetRequiredService<ILoggingService>();
    var scheduleConfigService = serviceProvider.GetRequiredService<IScheduleConfigService>();

    // Blocking at app startup: IScheduleConfigService only exposes async methods.
    // This is the only location in the codebase where blocking await is acceptable.
    var config = scheduleConfigService.LoadScheduleConfigAsync().GetAwaiter().GetResult();
    var initialDir = string.IsNullOrWhiteSpace(config.CustomMoodDataPath)
        ? folderShim.GetDocumentsFolder()
        : config.CustomMoodDataPath;
    var initialFilePath = folderShim.CombinePaths(initialDir, "mood_data.json");

    return new MoodDataService(
        initialFilePath,
        archiveService,
        folderShim,
        dateShim,
        fileShim,
        jsonSerializerShim,
        loggingService,
        scheduleConfigService);
});
```

### 5.4 Register StorageSettingsPage and ViewModel

Add the following registrations in the Pages/ViewModels section (after existing page registrations):

```csharp
builder.Services.AddTransient<StorageSettingsViewModel>();
builder.Services.AddTransient<StorageSettingsPage>();
```

**Verification**:
- [ ] Build succeeds: `dotnet build --framework net9.0-windows10.0.19041.0 MauiApp/WorkMood.MauiApp.csproj`
- [ ] All existing tests pass: `dotnet test`

---

## Part 6: StorageSettingsViewModel

**File to create**: `MauiApp/ViewModels/StorageSettingsViewModel.cs`

**READ `MauiApp/Infrastructure/ViewModelBase.cs` and `MauiApp/Infrastructure/RelayCommand.cs` FIRST.**

```csharp
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.ViewModels;

public class StorageSettingsViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly IFolderPickerShim _folderPickerShim;
    private readonly IPathValidationShim _pathValidationShim;

    private string _currentPath = string.Empty;
    private string _selectedPath = string.Empty;
    private string _validationMessage = string.Empty;
    private bool _isSelectedPathValid;
    private bool _isMigrating;

    public string CurrentPath
    {
        get => _currentPath;
        private set => SetProperty(ref _currentPath, value);
    }

    public string SelectedPath
    {
        get => _selectedPath;
        private set
        {
            if (SetProperty(ref _selectedPath, value))
                ValidateSelectedPath();
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public bool IsSelectedPathValid
    {
        get => _isSelectedPathValid;
        private set => SetProperty(ref _isSelectedPathValid, value);
    }

    public bool IsMigrating
    {
        get => _isMigrating;
        private set => SetProperty(ref _isMigrating, value);
    }

    public ICommand BrowseCommand { get; }
    public ICommand MigrateCommand { get; }

    public StorageSettingsViewModel(
        IMoodDataService moodDataService,
        IFolderPickerShim folderPickerShim,
        IPathValidationShim pathValidationShim)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _folderPickerShim = folderPickerShim ?? throw new ArgumentNullException(nameof(folderPickerShim));
        _pathValidationShim = pathValidationShim ?? throw new ArgumentNullException(nameof(pathValidationShim));

        BrowseCommand = new RelayCommand(async () => await ExecuteBrowseAsync());
        MigrateCommand = new RelayCommand(
            async () => await ExecuteMigrateAsync(),
            _ => IsSelectedPathValid && !IsMigrating);

        CurrentPath = _moodDataService.GetMoodDataDirectory();
    }

    private async Task ExecuteBrowseAsync()
    {
        var picked = await _folderPickerShim.PickFolderAsync();
        if (picked == null) return;
        SelectedPath = picked;
    }

    private void ValidateSelectedPath()
    {
        if (string.IsNullOrWhiteSpace(SelectedPath))
        {
            ValidationMessage = string.Empty;
            IsSelectedPathValid = false;
            return;
        }

        if (!_pathValidationShim.IsAbsolutePath(SelectedPath))
        {
            ValidationMessage = "Path must be an absolute (non-relative) path.";
            IsSelectedPathValid = false;
            return;
        }

        // Ensure directory exists before checking write permission
        try { Directory.CreateDirectory(SelectedPath); } catch { /* fall through to permission check */ }

        if (!_pathValidationShim.HasWritePermission(SelectedPath))
        {
            ValidationMessage = "Cannot write to this location. Check permissions.";
            IsSelectedPathValid = false;
            return;
        }

        ValidationMessage = "Path is valid and writable.";
        IsSelectedPathValid = true;
        RelayCommand.InvalidateRequerySuggested();
    }

    private async Task ExecuteMigrateAsync()
    {
        if (!IsSelectedPathValid) return;

        IsMigrating = true;
        ValidationMessage = "Migrating data...";

        try
        {
            await _moodDataService.MigrateMoodDataAsync(SelectedPath);
            CurrentPath = _moodDataService.GetMoodDataDirectory();
            SelectedPath = string.Empty;
            ValidationMessage = "Migration complete.";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Migration failed: {ex.Message}";
        }
        finally
        {
            IsMigrating = false;
            RelayCommand.InvalidateRequerySuggested();
        }
    }
}
```

**Constraints**:
- Do NOT inject `IScheduleConfigService` — config persistence is handled inside `MoodDataService.MigrateMoodDataAsync`.
- Do NOT call `Directory.CreateDirectory` anywhere except `ValidateSelectedPath` (one location, for the pre-permission-check directory creation). All other directory creation must use `IFolderShim`.
- `BrowseCommand` and `MigrateCommand` use `RelayCommand` only. `IAsyncRelayCommand` does not exist.

**Verification**:
- [ ] Build succeeds.

---

## Part 7: StorageSettingsPage View

**File to create**: `MauiApp/Pages/StorageSettingsPage.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="WorkMood.MauiApp.Pages.StorageSettingsPage"
             Title="Storage Settings">
    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="16">

            <Label Text="Mood Data Storage Location"
                   FontSize="18"
                   FontAttributes="Bold" />

            <Label Text="Current location:" />
            <Label Text="{Binding CurrentPath}"
                   FontFamily="Monospace"
                   TextColor="Gray"
                   LineBreakMode="CharacterWrap" />

            <BoxView HeightRequest="1" Color="LightGray" />

            <Label Text="Select a new location:" />

            <HorizontalStackLayout Spacing="8">
                <Label Text="{Binding SelectedPath}"
                       VerticalOptions="Center"
                       LineBreakMode="CharacterWrap"
                       HorizontalOptions="FillAndExpand" />
                <Button Text="Browse..."
                        Command="{Binding BrowseCommand}"
                        IsEnabled="{Binding IsMigrating, Converter={StaticResource InvertedBoolConverter}}" />
            </HorizontalStackLayout>

            <Label Text="{Binding ValidationMessage}">
                <Label.Triggers>
                    <DataTrigger TargetType="Label" Binding="{Binding IsSelectedPathValid}" Value="True">
                        <Setter Property="TextColor" Value="Green" />
                    </DataTrigger>
                    <DataTrigger TargetType="Label" Binding="{Binding IsSelectedPathValid}" Value="False">
                        <Setter Property="TextColor" Value="Red" />
                    </DataTrigger>
                </Label.Triggers>
            </Label>

            <Button Text="Migrate Data to New Location"
                    Command="{Binding MigrateCommand}"
                    IsEnabled="{Binding IsSelectedPathValid}" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

**Constraint — InvertedBoolConverter**: Verify that `InvertedBoolConverter` is accessible in this XAML (check `App.xaml` or `GlobalXmlns.cs` for the resource key). If the key differs from `InvertedBoolConverter`, use the correct key.

**File to create**: `MauiApp/Pages/StorageSettingsPage.xaml.cs`

```csharp
using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class StorageSettingsPage : ContentPage
{
    public StorageSettingsPage(StorageSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

**Constraint**: The code-behind has NO logic. All logic lives in `StorageSettingsViewModel`.

**Verification**:
- [ ] Build succeeds: `dotnet build --framework net9.0-windows10.0.19041.0 MauiApp/WorkMood.MauiApp.csproj`

---

## Part 8: Navigation — Add to Settings Page

**Files to modify**: `MauiApp/Pages/Settings.xaml` and `MauiApp/Pages/Settings.xaml.cs`

**READ BOTH FILES FIRST.**

### 8.1 Settings.xaml — Add Button

Add a button at the bottom of the existing content root layout (before its closing tag). Read the file to find the exact insertion point.

```xml
<Button Text="Configure Storage Location..."
        Clicked="OnStorageSettingsClicked"
        Margin="0,16,0,0" />
```

### 8.2 Settings.xaml.cs — Add Navigation Handler

Add the following event handler to the `Settings` class:

```csharp
private async void OnStorageSettingsClicked(object sender, EventArgs e)
{
    var page = Handler?.MauiContext?.Services?.GetService<StorageSettingsPage>();
    if (page != null)
        await Navigation.PushAsync(page);
}
```

**Constraints**:
- Do NOT add `StorageSettingsPage` as a constructor parameter to `Settings`. Resolve it from `MauiContext.Services` at click time.
- Do NOT add a new route to `AppShell.xaml`. Navigation is push-based from this page.

**Verification**:
- [ ] Build succeeds: `dotnet build --framework net9.0-windows10.0.19041.0 MauiApp/WorkMood.MauiApp.csproj`
- [ ] All existing tests pass: `dotnet test`

---

## Part 9: Tests — MoodDataService Migration

**File to modify**: `WorkMood.MauiApp.Tests/Services/MoodDataServiceShould.cs`

**READ THE ENTIRE FILE FIRST** to understand existing test structure, mock setup, and the `CreateMoodDataService()` helper. Do NOT modify that helper.

### 9.1 New Mock Field

Add to the class-level mock fields:
```csharp
private readonly Mock<IScheduleConfigService> _mockScheduleConfigService;
```

Initialize in the existing constructor alongside the other mocks:
```csharp
_mockScheduleConfigService = new Mock<IScheduleConfigService>();
```

### 9.2 New Helper: CreateMigratableMoodDataService

Add this factory helper to the test class. Do NOT replace or modify `CreateMoodDataService()`:

```csharp
private MoodDataService CreateMigratableMoodDataService(string initialFilePath = @"C:\TestDocuments\WorkMood\mood_data.json")
{
    return new MoodDataService(
        initialFilePath,
        _mockArchiveService.Object,
        _mockFolderShim.Object,
        _mockDateShim.Object,
        _mockFileShim.Object,
        _mockJsonSerializerShim.Object,
        new Mock<ILoggingService>().Object,
        _mockScheduleConfigService.Object);
}
```

### 9.3 Test: GetMoodDataDirectory returns active directory

```csharp
[Fact]
public void GetMoodDataDirectory_ReturnsDirectoryOfActiveFilePath()
{
    // Arrange
    var sut = CreateMigratableMoodDataService(@"C:\TestDocuments\WorkMood\mood_data.json");

    // Act
    var result = sut.GetMoodDataDirectory();

    // Assert
    result.Should().Be(@"C:\TestDocuments\WorkMood");
}
```

### 9.4 Test: Successful migration

```csharp
[Fact]
public async Task MigrateMoodDataAsync_SuccessfulMigration_UpdatesActiveDirectory()
{
    // Arrange
    const string originalFilePath = @"C:\TestDocuments\WorkMood\mood_data.json";
    const string newDir = @"C:\CustomStorage\WorkMood";
    const string newFilePath = @"C:\CustomStorage\WorkMood\mood_data.json";

    _mockFolderShim.Setup(x => x.CombinePaths(newDir, "mood_data.json")).Returns(newFilePath);
    _mockFolderShim.Setup(x => x.CreateDirectory(newDir));
    _mockFileShim.Setup(x => x.Exists(originalFilePath)).Returns(true);
    _mockFileShim.Setup(x => x.ReadAllTextAsync(originalFilePath)).ReturnsAsync("[{\"date\":\"2024-01-01\"}]");
    _mockFileShim.Setup(x => x.WriteAllTextAsync(newFilePath, It.IsAny<string>())).Returns(Task.CompletedTask);
    _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync()).ReturnsAsync(new ScheduleConfig());
    _mockScheduleConfigService.Setup(x => x.SaveScheduleConfigAsync(It.IsAny<ScheduleConfig>())).Returns(Task.CompletedTask);

    var sut = CreateMigratableMoodDataService(originalFilePath);

    // Act
    await sut.MigrateMoodDataAsync(newDir);

    // Assert
    sut.GetMoodDataDirectory().Should().Be(newDir);
    _mockFileShim.Verify(x => x.WriteAllTextAsync(newFilePath, It.IsAny<string>()), Times.Once);
    _mockScheduleConfigService.Verify(
        x => x.SaveScheduleConfigAsync(It.Is<ScheduleConfig>(c => c.CustomMoodDataPath == newDir)),
        Times.Once);
}
```

### 9.5 Test: Config save fails — rollback, path unchanged

```csharp
[Fact]
public async Task MigrateMoodDataAsync_WhenConfigSaveFails_ThrowsAndDoesNotChangeActiveDirectory()
{
    // Arrange
    const string originalFilePath = @"C:\TestDocuments\WorkMood\mood_data.json";
    const string newDir = @"C:\CustomStorage\WorkMood";
    const string newFilePath = @"C:\CustomStorage\WorkMood\mood_data.json";

    _mockFolderShim.Setup(x => x.CombinePaths(newDir, "mood_data.json")).Returns(newFilePath);
    _mockFolderShim.Setup(x => x.CreateDirectory(newDir));
    _mockFileShim.Setup(x => x.Exists(originalFilePath)).Returns(true);
    _mockFileShim.Setup(x => x.ReadAllTextAsync(originalFilePath)).ReturnsAsync("[]");
    _mockFileShim.Setup(x => x.WriteAllTextAsync(newFilePath, It.IsAny<string>())).Returns(Task.CompletedTask);
    _mockFileShim.Setup(x => x.Exists(newFilePath)).Returns(true);
    _mockScheduleConfigService.Setup(x => x.LoadScheduleConfigAsync()).ReturnsAsync(new ScheduleConfig());
    _mockScheduleConfigService.Setup(x => x.SaveScheduleConfigAsync(It.IsAny<ScheduleConfig>()))
        .ThrowsAsync(new IOException("Disk full"));

    var sut = CreateMigratableMoodDataService(originalFilePath);

    // Act
    var act = async () => await sut.MigrateMoodDataAsync(newDir);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Migration failed*");
    sut.GetMoodDataDirectory().Should().Be(@"C:\TestDocuments\WorkMood"); // unchanged
}
```

### 9.6 Test: Relative path throws immediately

```csharp
[Fact]
public async Task MigrateMoodDataAsync_RelativePath_ThrowsInvalidOperationException()
{
    // Arrange
    var sut = CreateMigratableMoodDataService();

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => sut.MigrateMoodDataAsync("relative/path"));
}
```

### 9.7 Test: Same path is idempotent

```csharp
[Fact]
public async Task MigrateMoodDataAsync_SamePath_DoesNotCopyOrSaveConfig()
{
    // Arrange
    const string samePath = @"C:\TestDocuments\WorkMood";
    _mockFolderShim.Setup(x => x.CombinePaths(samePath, "mood_data.json"))
        .Returns(@"C:\TestDocuments\WorkMood\mood_data.json");

    var sut = CreateMigratableMoodDataService(@"C:\TestDocuments\WorkMood\mood_data.json");

    // Act
    await sut.MigrateMoodDataAsync(samePath);

    // Assert — no file I/O or config save
    _mockFileShim.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    _mockScheduleConfigService.Verify(x => x.SaveScheduleConfigAsync(It.IsAny<ScheduleConfig>()), Times.Never);
}
```

**Verification**:
- [ ] Build succeeds: `dotnet build WorkMood.sln`
- [ ] All tests pass including new ones: `dotnet test`

---

## Implementation Order

Execute in this order. Verify build and tests after each part. Do NOT skip ahead.

1. **Part 1** — `ScheduleConfig.CustomMoodDataPath`
2. **Part 2** — Four new shim files
3. **Part 3** — `IMoodDataService` additions (build will fail until Part 4)
4. **Part 4** — `MoodDataService` field, overload constructor, two new methods
5. **Part 5** — `MauiProgram.cs` DI updates
6. **Part 9** — Tests (add before writing UI to verify service layer)
7. **Part 6** — `StorageSettingsViewModel`
8. **Part 7** — `StorageSettingsPage.xaml` and `.xaml.cs`
9. **Part 8** — Update `Settings.xaml` and `Settings.xaml.cs`

---

## Files to Create

| File | Part |
|------|------|
| `MauiApp/Shims/IPathValidationShim.cs` | 2.1 |
| `MauiApp/Shims/PathValidationShim.cs` | 2.1 |
| `MauiApp/Shims/IFolderPickerShim.cs` | 2.2 |
| `MauiApp/Shims/FolderPickerShim.cs` | 2.2 |
| `MauiApp/ViewModels/StorageSettingsViewModel.cs` | 6 |
| `MauiApp/Pages/StorageSettingsPage.xaml` | 7 |
| `MauiApp/Pages/StorageSettingsPage.xaml.cs` | 7 |

## Files to Modify

| File | Part | What Changes |
|------|------|--------------|
| `MauiApp/Models/ScheduleConfig.cs` | 1 | Add `CustomMoodDataPath` property |
| `MauiApp/Services/IMoodDataService.cs` | 3 | Add `GetMoodDataDirectory()` and `MigrateMoodDataAsync()` |
| `MauiApp/Services/MoodDataService.cs` | 4 | Remove `readonly` from `_dataFilePath`; add `_scheduleConfigService` field; add 8-param constructor overload; add two new methods |
| `MauiApp/MauiProgram.cs` | 5 | Register 4 new types; replace `MoodDataService` DI lambda |
| `MauiApp/Pages/Settings.xaml` | 8 | Add "Configure Storage Location..." button |
| `MauiApp/Pages/Settings.xaml.cs` | 8 | Add `OnStorageSettingsClicked` handler |
| `WorkMood.MauiApp.Tests/Services/MoodDataServiceShould.cs` | 9 | Add mock field, factory helper, and 5 new tests |

---

## Overall Acceptance Criteria

All of the following must be true before the feature is done:

- [ ] `dotnet build WorkMood.sln` — zero errors
- [ ] `dotnet test` — zero failures (all pre-existing tests continue to pass)
- [ ] Settings page has a "Configure Storage Location..." button
- [ ] Button opens a StorageSettings page (push navigation, not modal)
- [ ] "Browse..." opens a native OS folder picker
- [ ] Selecting a valid writable folder: shows green "Path is valid and writable." message; enables "Migrate Data" button
- [ ] Selecting a relative path: shows red validation message; "Migrate Data" button disabled
- [ ] Selecting an unwritable path: shows red validation message; "Migrate Data" button disabled
- [ ] "Migrate Data" on a valid path: copies `mood_data.json` to the new directory; updates "Current location" label; clears selected path field
- [ ] After migration, mood data is loadable from the new location in the same session
- [ ] After app restart following migration, mood data loads from the new path (config was persisted)
- [ ] Migrating again to a third path works without crashing
- [ ] Migration failure (e.g., disk full on config save) shows an error message; original data is untouched
