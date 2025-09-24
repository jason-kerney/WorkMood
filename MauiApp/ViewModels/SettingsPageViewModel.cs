using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the Settings page following MVVM pattern and SOLID principles
/// </summary>
public class SettingsPageViewModel : ViewModelBase
{
    private readonly IScheduleConfigService _scheduleConfigService;
    private readonly INavigationService _navigationService;
    private ScheduleConfig _currentConfig;

    // Private backing fields
    private TimeSpan _morningTime;
    private TimeSpan _eveningTime;
    private bool _isNewOverrideEnabled;
    private DateTime _newOverrideDate;
    private bool _hasNewMorningOverride;
    private bool _hasNewEveningOverride;
    private TimeSpan _newOverrideMorningTime;
    private TimeSpan _newOverrideEveningTime;
    private ObservableCollection<ScheduleOverride> _existingOverrides;

    // Public properties for data binding
    public TimeSpan MorningTime 
    { 
        get => _morningTime;
        set 
        { 
            if (SetProperty(ref _morningTime, value))
            {
                OnPropertyChanged(nameof(MorningPreviewText));
            }
        }
    }

    public TimeSpan EveningTime 
    { 
        get => _eveningTime;
        set 
        { 
            if (SetProperty(ref _eveningTime, value))
            {
                OnPropertyChanged(nameof(EveningPreviewText));
            }
        }
    }

    public bool IsNewOverrideEnabled 
    { 
        get => _isNewOverrideEnabled;
        set => SetProperty(ref _isNewOverrideEnabled, value);
    }

    public DateTime NewOverrideDate 
    { 
        get => _newOverrideDate;
        set => SetProperty(ref _newOverrideDate, value);
    }

    public bool HasNewMorningOverride 
    { 
        get => _hasNewMorningOverride;
        set => SetProperty(ref _hasNewMorningOverride, value);
    }

    public bool HasNewEveningOverride 
    { 
        get => _hasNewEveningOverride;
        set => SetProperty(ref _hasNewEveningOverride, value);
    }

    public TimeSpan NewOverrideMorningTime 
    { 
        get => _newOverrideMorningTime;
        set => SetProperty(ref _newOverrideMorningTime, value);
    }

    public TimeSpan NewOverrideEveningTime 
    { 
        get => _newOverrideEveningTime;
        set => SetProperty(ref _newOverrideEveningTime, value);
    }

    public ObservableCollection<ScheduleOverride> ExistingOverrides 
    {
        get => _existingOverrides;
        private set => SetProperty(ref _existingOverrides, value);
    }

    // Computed properties
    public string MorningPreviewText => $"Morning reminder will be shown at {MorningTime:hh\\:mm}";
    public string EveningPreviewText => $"Evening reminder will be shown at {EveningTime:hh\\:mm}";

    // Commands
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand EditOverrideCommand { get; }
    public ICommand DeleteOverrideCommand { get; }

    public SettingsPageViewModel(IScheduleConfigService scheduleConfigService, INavigationService navigationService)
    {
        _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        _currentConfig = new ScheduleConfig();
        _existingOverrides = new ObservableCollection<ScheduleOverride>();

        // Initialize commands
        SaveCommand = new RelayCommand(async () => await SaveSettingsAsync());
        CancelCommand = new RelayCommand(async () => await CancelChangesAsync());
        EditOverrideCommand = new RelayCommand<ScheduleOverride>(async (overrideItem) => await EditOverrideAsync(overrideItem));
        DeleteOverrideCommand = new RelayCommand<ScheduleOverride>(async (overrideItem) => await DeleteOverrideAsync(overrideItem));

        // Initialize form with default values
        InitializeFormDefaults();
    }

    /// <summary>
    /// Loads the current configuration from the service
    /// </summary>
    public async Task LoadConfigurationAsync()
    {
        try
        {
            _currentConfig = await _scheduleConfigService.LoadScheduleConfigAsync();
            
            // Update properties
            MorningTime = _currentConfig.MorningTime;
            EveningTime = _currentConfig.EveningTime;
            
            // Initialize new override form
            InitializeFormDefaults();
            
            // Update existing overrides collection
            RefreshExistingOverrides();
        }
        catch (Exception ex)
        {
            await _navigationService.ShowAlertAsync("Error", $"Failed to load current configuration: {ex.Message}", "OK");
            
            // Use default values
            InitializeFormDefaults();
        }
    }

    /// <summary>
    /// Initializes the form with default values
    /// </summary>
    private void InitializeFormDefaults()
    {
        IsNewOverrideEnabled = false;
        NewOverrideDate = DateTime.Today.AddDays(1);
        HasNewMorningOverride = false;
        HasNewEveningOverride = false;
        NewOverrideMorningTime = _currentConfig?.MorningTime ?? new TimeSpan(8, 20, 0);
        NewOverrideEveningTime = _currentConfig?.EveningTime ?? new TimeSpan(17, 20, 0);
    }

    /// <summary>
    /// Refreshes the existing overrides collection
    /// </summary>
    private void RefreshExistingOverrides()
    {
        ExistingOverrides.Clear();
        
        if (_currentConfig?.Overrides != null)
        {
            var sortedOverrides = _currentConfig.Overrides.OrderBy(o => o.Date);
            foreach (var overrideItem in sortedOverrides)
            {
                ExistingOverrides.Add(overrideItem);
            }
        }
    }

    /// <summary>
    /// Saves the current settings
    /// </summary>
    private async Task SaveSettingsAsync()
    {
        try
        {
            // Create new override if enabled
            ScheduleOverride? newOverride = null;
            if (IsNewOverrideEnabled && (HasNewMorningOverride || HasNewEveningOverride))
            {
                var overrideDate = DateOnly.FromDateTime(NewOverrideDate);
                TimeSpan? overrideMorningTime = HasNewMorningOverride ? NewOverrideMorningTime : null;
                TimeSpan? overrideEveningTime = HasNewEveningOverride ? NewOverrideEveningTime : null;
                
                newOverride = new ScheduleOverride(overrideDate, overrideMorningTime, overrideEveningTime);
            }
            
            // Use the centralized update method with automatic cleanup
            var updatedConfig = await _scheduleConfigService.UpdateScheduleConfigAsync(MorningTime, EveningTime, newOverride);
            
            // Update our local reference
            _currentConfig = updatedConfig;
            
            // Navigate back to the previous page
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _navigationService.ShowAlertAsync("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Cancels changes and navigates back
    /// </summary>
    private async Task CancelChangesAsync()
    {
        // Check for changes in basic schedule
        bool hasBasicChanges = MorningTime != _currentConfig.MorningTime || 
                              EveningTime != _currentConfig.EveningTime;
        
        // Check for changes in new override settings
        bool hasOverrideChanges = false;
        
        if (IsNewOverrideEnabled && (HasNewMorningOverride || HasNewEveningOverride))
        {
            // User has enabled a new override - this is a change
            hasOverrideChanges = true;
        }
        
        bool hasChanges = hasBasicChanges || hasOverrideChanges;
        
        if (hasChanges)
        {
            bool discardChanges = await _navigationService.ShowConfirmationAsync("Discard Changes?", 
                "You have unsaved changes. Are you sure you want to go back?", 
                "Discard", "Stay");
            
            if (!discardChanges)
                return;
        }
        
        // Navigate back to the previous page
        await _navigationService.GoBackAsync();
    }

    /// <summary>
    /// Edits an existing override by populating the new override form
    /// </summary>
    private async Task EditOverrideAsync(ScheduleOverride? overrideToEdit)
    {
        if (overrideToEdit == null) return;

        try
        {
            // First, remove the existing override
            _currentConfig.RemoveOverride(overrideToEdit.Date);
            await _scheduleConfigService.SaveScheduleConfigAsync(_currentConfig);

            // Populate the new override form with the existing override's values
            IsNewOverrideEnabled = true;
            NewOverrideDate = overrideToEdit.Date.ToDateTime(TimeOnly.MinValue);
            
            if (overrideToEdit.MorningTime.HasValue)
            {
                HasNewMorningOverride = true;
                NewOverrideMorningTime = overrideToEdit.MorningTime.Value;
            }
            else
            {
                HasNewMorningOverride = false;
                NewOverrideMorningTime = _currentConfig.MorningTime;
            }

            if (overrideToEdit.EveningTime.HasValue)
            {
                HasNewEveningOverride = true;
                NewOverrideEveningTime = overrideToEdit.EveningTime.Value;
            }
            else
            {
                HasNewEveningOverride = false;
                NewOverrideEveningTime = _currentConfig.EveningTime;
            }

            // Refresh the existing overrides list
            RefreshExistingOverrides();
        }
        catch (Exception ex)
        {
            await _navigationService.ShowAlertAsync("Error", $"Failed to edit override: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Deletes an existing override
    /// </summary>
    private async Task DeleteOverrideAsync(ScheduleOverride? overrideToDelete)
    {
        if (overrideToDelete == null) return;

        bool confirm = await _navigationService.ShowConfirmationAsync("Delete Override?", 
            $"Are you sure you want to delete the override for {overrideToDelete.Date:yyyy-MM-dd}?", 
            "Delete", "Cancel");
        
        if (confirm)
        {
            // Remove the override from the configuration
            _currentConfig.RemoveOverride(overrideToDelete.Date);
            
            // Save the updated configuration
            await _scheduleConfigService.SaveScheduleConfigAsync(_currentConfig);
            
            // Refresh the UI
            RefreshExistingOverrides();
            
            await _navigationService.ShowAlertAsync("Success", "Override deleted successfully.", "OK");
        }
    }
}