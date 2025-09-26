using System.Windows.Input;
using System.ComponentModel;
using System.Globalization;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the MoodRecording page following MVVM pattern and SOLID principles
/// Handles mood selection, validation, saving, and UI state management
/// </summary>
public class MoodRecordingViewModel : ViewModelBase
{
    #region Private Fields
    
    private readonly MoodDataService _moodDataService;
    private readonly MoodDispatcherService _dispatcherService;
    private readonly ILoggingService _loggingService;
    
    private MoodEntryOld _currentMoodEntry = new();
    private int? _selectedMorningMood;
    private int? _selectedEveningMood;
    private bool _morningMoodSaved;
    private bool _eveningMoodSaved;
    private bool _isEditingMorning;
    private bool _isEditingEvening;
    private bool _isLoading;
    private string _morningMoodLabel = "No mood selected";
    private string _eveningMoodLabel = "Save morning mood first";
    private bool _isMorningInfoVisible;
    private bool _isEveningInfoVisible;
    private bool _isEditMorningVisible;
    private bool _isEditEveningVisible;
    private string _editMorningText = "Edit";
    private string _editEveningText = "Edit";
    
    #endregion

    #region Constructor
    
    public MoodRecordingViewModel(
        MoodDataService moodDataService, 
        MoodDispatcherService dispatcherService,
        ILoggingService loggingService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        
        InitializeCommands();
        InitializeViewModel();
    }
    
    #endregion

    #region Public Properties

    public string CurrentDate => DateTime.Today.ToString("dddd, MMMM dd, yyyy");

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int? SelectedMorningMood
    {
        get => _selectedMorningMood;
        set
        {
            if (SetProperty(ref _selectedMorningMood, value))
            {
                UpdateMorningMoodLabel();
                UpdateUIState();
                OnPropertyChanged(nameof(IsSaveMorningEnabled));
                OnPropertyChanged(nameof(MorningMoodBorderColor));
                for (int i = 1; i <= 10; i++)
                {
                    OnPropertyChanged($"IsMorningMood{i}Selected");
                }
            }
        }
    }

    public int? SelectedEveningMood
    {
        get => _selectedEveningMood;
        set
        {
            if (SetProperty(ref _selectedEveningMood, value))
            {
                UpdateEveningMoodLabel();
                UpdateUIState();
                OnPropertyChanged(nameof(IsSaveEveningEnabled));
                OnPropertyChanged(nameof(EveningMoodBorderColor));
                for (int i = 1; i <= 10; i++)
                {
                    OnPropertyChanged($"IsEveningMood{i}Selected");
                }
            }
        }
    }

    public bool MorningMoodSaved
    {
        get => _morningMoodSaved;
        set
        {
            if (SetProperty(ref _morningMoodSaved, value))
            {
                UpdateUIState();
                OnPropertyChanged(nameof(IsSaveMorningEnabled));
                OnPropertyChanged(nameof(IsSaveEveningEnabled));
                OnPropertyChanged(nameof(MorningMoodBorderColor));
                OnPropertyChanged(nameof(EveningMoodBorderColor));
                OnPropertyChanged(nameof(AreMorningMoodButtonsVisible));
                OnPropertyChanged(nameof(AreEveningMoodButtonsEnabled));
                for (int i = 1; i <= 10; i++)
                {
                    OnPropertyChanged($"IsEveningMood{i}Enabled");
                }
            }
        }
    }

    public bool EveningMoodSaved
    {
        get => _eveningMoodSaved;
        set
        {
            if (SetProperty(ref _eveningMoodSaved, value))
            {
                UpdateUIState();
                OnPropertyChanged(nameof(IsSaveEveningEnabled));
                OnPropertyChanged(nameof(EveningMoodBorderColor));
                OnPropertyChanged(nameof(AreEveningMoodButtonsVisible));
            }
        }
    }

    public bool IsEditingMorning
    {
        get => _isEditingMorning;
        set
        {
            if (SetProperty(ref _isEditingMorning, value))
            {
                UpdateUIState();
                OnPropertyChanged(nameof(AreMorningMoodButtonsVisible));
                OnPropertyChanged(nameof(MorningMoodBorderColor));
                OnPropertyChanged(nameof(IsSaveMorningEnabled));
            }
        }
    }

    public bool IsEditingEvening
    {
        get => _isEditingEvening;
        set
        {
            if (SetProperty(ref _isEditingEvening, value))
            {
                UpdateUIState();
                OnPropertyChanged(nameof(AreEveningMoodButtonsVisible));
                OnPropertyChanged(nameof(AreEveningMoodButtonsEnabled));
                OnPropertyChanged(nameof(EveningMoodBorderColor));
                OnPropertyChanged(nameof(IsSaveEveningEnabled));
                for (int i = 1; i <= 10; i++)
                {
                    OnPropertyChanged($"IsEveningMood{i}Enabled");
                }
            }
        }
    }

    public string MorningMoodLabel
    {
        get => _morningMoodLabel;
        set => SetProperty(ref _morningMoodLabel, value);
    }

    public string EveningMoodLabel
    {
        get => _eveningMoodLabel;
        set => SetProperty(ref _eveningMoodLabel, value);
    }

    public bool IsMorningInfoVisible
    {
        get => _isMorningInfoVisible;
        set => SetProperty(ref _isMorningInfoVisible, value);
    }

    public bool IsEveningInfoVisible
    {
        get => _isEveningInfoVisible;
        set => SetProperty(ref _isEveningInfoVisible, value);
    }

    public bool IsEditMorningVisible
    {
        get => _isEditMorningVisible;
        set => SetProperty(ref _isEditMorningVisible, value);
    }

    public bool IsEditEveningVisible
    {
        get => _isEditEveningVisible;
        set => SetProperty(ref _isEditEveningVisible, value);
    }

    public string EditMorningText
    {
        get => _editMorningText;
        set => SetProperty(ref _editMorningText, value);
    }

    public string EditEveningText
    {
        get => _editEveningText;
        set => SetProperty(ref _editEveningText, value);
    }

    #endregion

    #region UI State Properties

    public Color MorningMoodBorderColor =>
        (MorningMoodSaved && !IsEditingMorning) ? Color.FromArgb("#6E6E6E") : Color.FromArgb("#512BD4");

    public Color EveningMoodBorderColor
    {
        get
        {
            if (EveningMoodSaved && !IsEditingEvening)
                return Color.FromArgb("#6E6E6E");
            if (MorningMoodSaved || IsEditingEvening)
                return Color.FromArgb("#512BD4");
            return Color.FromArgb("#6E6E6E");
        }
    }

    public bool AreMorningMoodButtonsVisible => !(MorningMoodSaved && !IsEditingMorning);

    public bool AreEveningMoodButtonsVisible => !(EveningMoodSaved && !IsEditingEvening);

    public bool AreEveningMoodButtonsEnabled => MorningMoodSaved || IsEditingEvening;

    public bool IsSaveMorningEnabled => 
        (SelectedMorningMood.HasValue && !MorningMoodSaved) || 
        (IsEditingMorning && SelectedMorningMood.HasValue);

    public bool IsSaveEveningEnabled => 
        MorningMoodSaved && SelectedEveningMood.HasValue && 
        (!EveningMoodSaved || IsEditingEvening);

    public Color SaveMorningButtonColor => 
        IsSaveMorningEnabled ? Color.FromArgb("#512BD4") : Color.FromArgb("#6E6E6E");

    public Color SaveEveningButtonColor => 
        IsSaveEveningEnabled ? Color.FromArgb("#512BD4") : Color.FromArgb("#6E6E6E");

    #endregion

    #region Mood Button Selection Properties

    // Morning mood selection properties
    public bool IsMorningMood1Selected => SelectedMorningMood == 1;
    public bool IsMorningMood2Selected => SelectedMorningMood == 2;
    public bool IsMorningMood3Selected => SelectedMorningMood == 3;
    public bool IsMorningMood4Selected => SelectedMorningMood == 4;
    public bool IsMorningMood5Selected => SelectedMorningMood == 5;
    public bool IsMorningMood6Selected => SelectedMorningMood == 6;
    public bool IsMorningMood7Selected => SelectedMorningMood == 7;
    public bool IsMorningMood8Selected => SelectedMorningMood == 8;
    public bool IsMorningMood9Selected => SelectedMorningMood == 9;
    public bool IsMorningMood10Selected => SelectedMorningMood == 10;

    // Evening mood selection properties
    public bool IsEveningMood1Selected => SelectedEveningMood == 1;
    public bool IsEveningMood2Selected => SelectedEveningMood == 2;
    public bool IsEveningMood3Selected => SelectedEveningMood == 3;
    public bool IsEveningMood4Selected => SelectedEveningMood == 4;
    public bool IsEveningMood5Selected => SelectedEveningMood == 5;
    public bool IsEveningMood6Selected => SelectedEveningMood == 6;
    public bool IsEveningMood7Selected => SelectedEveningMood == 7;
    public bool IsEveningMood8Selected => SelectedEveningMood == 8;
    public bool IsEveningMood9Selected => SelectedEveningMood == 9;
    public bool IsEveningMood10Selected => SelectedEveningMood == 10;

    // Evening mood enabled properties
    public bool IsEveningMood1Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood2Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood3Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood4Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood5Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood6Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood7Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood8Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood9Enabled => AreEveningMoodButtonsEnabled;
    public bool IsEveningMood10Enabled => AreEveningMoodButtonsEnabled;

    #endregion

    #region Commands

    public ICommand MorningMoodSelectedCommand { get; private set; } = null!;
    public ICommand EveningMoodSelectedCommand { get; private set; } = null!;
    public ICommand SaveMorningCommand { get; private set; } = null!;
    public ICommand SaveEveningCommand { get; private set; } = null!;
    public ICommand EditMorningCommand { get; private set; } = null!;
    public ICommand EditEveningCommand { get; private set; } = null!;
    public ICommand BackToMainCommand { get; private set; } = null!;
    public ICommand LoadDataCommand { get; private set; } = null!;

    #endregion

    #region Events

    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler? NavigateBackRequested;

    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        MorningMoodSelectedCommand = new RelayCommand<int>(ExecuteMorningMoodSelected);
        EveningMoodSelectedCommand = new RelayCommand<int>(ExecuteEveningMoodSelected);
        SaveMorningCommand = new RelayCommand(ExecuteSaveMorning, () => IsSaveMorningEnabled);
        SaveEveningCommand = new RelayCommand(ExecuteSaveEvening, () => IsSaveEveningEnabled);
        EditMorningCommand = new RelayCommand(ExecuteEditMorning);
        EditEveningCommand = new RelayCommand(ExecuteEditEvening);
        BackToMainCommand = new RelayCommand(ExecuteBackToMain);
        LoadDataCommand = new RelayCommand(async () => await LoadTodaysMoodAsync());
    }

    private async void InitializeViewModel()
    {
        await LoadTodaysMoodAsync();
        UpdateUIState();
    }

    private async Task LoadTodaysMoodAsync()
    {
        try
        {
            IsLoading = true;
            await LoadTodaysMood();
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to load today's mood: {ex.Message}");
            ErrorOccurred?.Invoke(this, $"Failed to load today's mood: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTodaysMood()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var existingEntry = await _moodDataService.GetMoodEntryAsync(today);
        
        if (existingEntry != null)
        {
            _currentMoodEntry = existingEntry;
            
            if (_currentMoodEntry.MorningMood.HasValue)
            {
                SelectedMorningMood = _currentMoodEntry.MorningMood.Value;
                MorningMoodSaved = true;
            }
            
            if (_currentMoodEntry.EveningMood.HasValue)
            {
                SelectedEveningMood = _currentMoodEntry.EveningMood.Value;
                EveningMoodSaved = true;
            }
            
            UpdateUIState();
        }
        else
        {
            // Create new entry for today
            _currentMoodEntry = new MoodEntryOld { Date = today };
        }
    }

    private void ExecuteMorningMoodSelected(int mood)
    {
        SelectedMorningMood = mood;
        _currentMoodEntry.MorningMood = mood;
        _loggingService.LogInfo($"Morning mood selected: {mood}");
    }

    private void ExecuteEveningMoodSelected(int mood)
    {
        _loggingService.LogInfo($"Evening mood selected - Morning saved: {MorningMoodSaved}, Evening saved: {EveningMoodSaved}, Editing evening: {IsEditingEvening}");
        
        // Apply business rules for evening mood selection
        if (!MorningMoodSaved && !IsEditingEvening)
        {
            _loggingService.LogInfo("Blocking evening selection - morning not saved and not editing");
            ErrorOccurred?.Invoke(this, "Please save your morning mood before selecting evening mood.");
            return;
        }

        if (EveningMoodSaved && !IsEditingEvening)
        {
            _loggingService.LogInfo("Blocking evening selection - evening saved but not editing");
            return; // Silently ignore clicks when saved but not editing
        }

        SelectedEveningMood = mood;
        _currentMoodEntry.EveningMood = mood;
        _loggingService.LogInfo($"Setting evening mood to: {mood}");
    }

    private async void ExecuteSaveMorning()
    {
        try
        {
            if (!SelectedMorningMood.HasValue)
            {
                ErrorOccurred?.Invoke(this, "Please select a morning mood first.");
                return;
            }

            _currentMoodEntry.MorningMood = SelectedMorningMood.Value;
            await _moodDataService.SaveMoodEntryAsync(_currentMoodEntry);
            
            MorningMoodSaved = true;
            IsEditingMorning = false;
            
            UpdateUIState();
            NavigateBackRequested?.Invoke(this, EventArgs.Empty);
            
            _loggingService.LogInfo($"Morning mood saved: {SelectedMorningMood.Value}");
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to save morning mood: {ex.Message}");
            ErrorOccurred?.Invoke(this, $"Failed to save morning mood: {ex.Message}");
        }
    }

    private async void ExecuteSaveEvening()
    {
        try
        {
            if (!MorningMoodSaved)
            {
                ErrorOccurred?.Invoke(this, "Please save your morning mood first.");
                return;
            }

            if (!SelectedEveningMood.HasValue)
            {
                ErrorOccurred?.Invoke(this, "Please select an evening mood first.");
                return;
            }

            _currentMoodEntry.EveningMood = SelectedEveningMood.Value;
            await _moodDataService.SaveMoodEntryAsync(_currentMoodEntry);
            
            EveningMoodSaved = true;
            IsEditingEvening = false;
            
            UpdateUIState();
            NavigateBackRequested?.Invoke(this, EventArgs.Empty);
            
            _loggingService.LogInfo($"Evening mood saved: {SelectedEveningMood.Value}");
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Failed to save evening mood: {ex.Message}");
            ErrorOccurred?.Invoke(this, $"Failed to save evening mood: {ex.Message}");
        }
    }

    private void ExecuteEditMorning()
    {
        if (IsEditingMorning)
        {
            // Cancel edit - restore saved state
            IsEditingMorning = false;
            SelectedMorningMood = _currentMoodEntry.MorningMood;
            _loggingService.LogInfo("Cancelled morning editing");
        }
        else
        {
            // Start editing
            IsEditingMorning = true;
            _loggingService.LogInfo("Started morning editing");
        }
        UpdateUIState();
    }

    private void ExecuteEditEvening()
    {
        _loggingService.LogInfo($"Edit evening clicked - Currently editing: {IsEditingEvening}");
        
        if (IsEditingEvening)
        {
            // Cancel edit - restore saved state
            IsEditingEvening = false;
            SelectedEveningMood = _currentMoodEntry.EveningMood;
            _loggingService.LogInfo("Cancelled evening editing");
        }
        else
        {
            // Start editing
            IsEditingEvening = true;
            _loggingService.LogInfo("Started evening editing");
        }
        UpdateUIState();
    }

    private void ExecuteBackToMain()
    {
        NavigateBackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMorningMoodLabel()
    {
        if (MorningMoodSaved && !IsEditingMorning)
        {
            MorningMoodLabel = "Mood saved (hidden to prevent bias)";
        }
        else if (IsEditingMorning && SelectedMorningMood.HasValue)
        {
            MorningMoodLabel = $"Editing: {SelectedMorningMood.Value}";
        }
        else if (SelectedMorningMood.HasValue)
        {
            MorningMoodLabel = $"Selected: {SelectedMorningMood.Value}";
        }
        else
        {
            MorningMoodLabel = "No mood selected";
        }
    }

    private void UpdateEveningMoodLabel()
    {
        if (EveningMoodSaved && !IsEditingEvening)
        {
            EveningMoodLabel = "Mood saved (hidden to prevent bias)";
        }
        else if (IsEditingEvening && SelectedEveningMood.HasValue)
        {
            EveningMoodLabel = $"Editing: {SelectedEveningMood.Value}";
        }
        else if (SelectedEveningMood.HasValue)
        {
            EveningMoodLabel = $"Selected: {SelectedEveningMood.Value}";
        }
        else if (!MorningMoodSaved)
        {
            EveningMoodLabel = "Save morning mood first";
        }
        else
        {
            EveningMoodLabel = "No mood selected";
        }
    }

    private void UpdateUIState()
    {
        UpdateMorningMoodLabel();
        UpdateEveningMoodLabel();
        UpdateEditButtonVisibility();
        UpdateInfoLabelVisibility();
        
        // Ensure computed color properties are refreshed
        OnPropertyChanged(nameof(SaveMorningButtonColor));
        OnPropertyChanged(nameof(SaveEveningButtonColor));
        
        // Notify command state changes
        (SaveMorningCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SaveEveningCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void UpdateEditButtonVisibility()
    {
        IsEditMorningVisible = MorningMoodSaved;
        IsEditEveningVisible = EveningMoodSaved;
        
        EditMorningText = IsEditingMorning ? "Cancel Edit" : "Edit";
        EditEveningText = IsEditingEvening ? "Cancel Edit" : "Edit";
    }

    private void UpdateInfoLabelVisibility()
    {
        IsMorningInfoVisible = MorningMoodSaved && !IsEditingMorning;
        IsEveningInfoVisible = EveningMoodSaved && !IsEditingEvening;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the mood data (called when returning from navigation)
    /// </summary>
    public async Task RefreshMoodDataAsync()
    {
        try
        {
            await CreateNewRecordForToday();
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error refreshing mood data: {ex.Message}");
        }
    }

    private async Task CreateNewRecordForToday()
    {
        // Reset all state for new day
        _currentMoodEntry = new MoodEntryOld { Date = DateOnly.FromDateTime(DateTime.Today) };
        SelectedMorningMood = null;
        SelectedEveningMood = null;
        MorningMoodSaved = false;
        EveningMoodSaved = false;
        IsEditingMorning = false;
        IsEditingEvening = false;
        
        // Load any existing data for today
        await LoadTodaysMood();
        
        _loggingService.LogInfo($"Created new record for {_currentMoodEntry.Date}");
    }

    #endregion
}

/// <summary>
/// Generic RelayCommand that accepts a parameter of type T
/// </summary>
/// <typeparam name="T">The type of parameter the command accepts</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        if (TryConvertParameter(parameter, out var typed))
            return _canExecute?.Invoke(typed) ?? true;
        return false;
    }

    public void Execute(object? parameter)
    {
        if (TryConvertParameter(parameter, out var typed))
            _execute(typed);
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    private static bool TryConvertParameter(object? parameter, out T result)
    {
        // Direct match
        if (parameter is T direct)
        {
            result = direct;
            return true;
        }

        // Handle nulls for reference/nullable types
        if (parameter is null)
        {
            result = default!;
            // Allow null when T is a reference type or nullable
            return default(T) == null;
        }

        try
        {
            var targetType = typeof(T);

            // Unwrap Nullable<T>
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Enum conversion
            if (underlying.IsEnum)
            {
                if (parameter is string es)
                {
                    result = (T)Enum.Parse(underlying, es, ignoreCase: true);
                    return true;
                }
                if (parameter is IConvertible)
                {
                    var numeric = Convert.ChangeType(parameter, Enum.GetUnderlyingType(underlying), CultureInfo.InvariantCulture);
                    result = (T)Enum.ToObject(underlying, numeric!);
                    return true;
                }
            }

            // Use type converter if available
            var converter = TypeDescriptor.GetConverter(underlying);
            if (converter != null)
            {
                if (converter.CanConvertFrom(parameter.GetType()))
                {
                    var converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, parameter);
                    result = (T)(converted is null ? default! : (Nullable.GetUnderlyingType(targetType) != null ? converted : Convert.ChangeType(converted, underlying, CultureInfo.InvariantCulture))!);
                    return true;
                }
                if (parameter is string s && converter.CanConvertFrom(typeof(string)))
                {
                    var converted = converter.ConvertFromString(null, CultureInfo.InvariantCulture, s);
                    result = (T)(converted is null ? default! : (Nullable.GetUnderlyingType(targetType) != null ? converted : Convert.ChangeType(converted, underlying, CultureInfo.InvariantCulture))!);
                    return true;
                }
            }

            // Fallback to Convert.ChangeType (covers primitives like int)
            var value = Convert.ChangeType(parameter, underlying, CultureInfo.InvariantCulture);
            result = (T)(Nullable.GetUnderlyingType(targetType) != null ? value : Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture))!;
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
    }
}