using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.ViewModels;

public class StorageSettingsViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly INavigationService _navigationService;
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
        INavigationService navigationService,
        IFolderPickerShim folderPickerShim,
        IPathValidationShim pathValidationShim)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _folderPickerShim = folderPickerShim ?? throw new ArgumentNullException(nameof(folderPickerShim));
        _pathValidationShim = pathValidationShim ?? throw new ArgumentNullException(nameof(pathValidationShim));

        BrowseCommand = new RelayCommand(async () => await ExecuteBrowseAsync());
        MigrateCommand = new RelayCommand(
            async () => await ExecuteMigrateAsync(),
            () => IsSelectedPathValid && !IsMigrating);

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
            SetValidationState(string.Empty, false);
            return;
        }

        if (string.Equals(SelectedPath, CurrentPath, StringComparison.OrdinalIgnoreCase))
        {
            SetValidationState("Selected path is already the current location.", false);
            return;
        }

        if (!_pathValidationShim.IsAbsolutePath(SelectedPath))
        {
            SetValidationState("Path must be an absolute (non-relative) path.", false);
            return;
        }

        // Ensure directory exists before checking write permission
        try { Directory.CreateDirectory(SelectedPath); } catch { /* fall through to permission check */ }

        if (!_pathValidationShim.HasWritePermission(SelectedPath))
        {
            SetValidationState("Cannot write to this location. Check permissions.", false);
            return;
        }

        SetValidationState("Path is valid and writable.", true);
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
            await _navigationService.GoToRootAsync();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Migration failed: {ex.Message}";
        }
        finally
        {
            IsMigrating = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void SetValidationState(string message, bool isValid)
    {
        ValidationMessage = message;
        IsSelectedPathValid = isValid;
        CommandManager.InvalidateRequerySuggested();
    }
}
