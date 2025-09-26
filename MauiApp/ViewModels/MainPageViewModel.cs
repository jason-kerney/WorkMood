using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Models;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the main page that handles all business logic and user interactions
/// </summary>
public class MainPageViewModel : ViewModelBase, IDisposable
{
    private readonly MoodDataService _moodDataService;
    private readonly MoodDispatcherService _dispatcherService;
    private readonly ScheduleConfigService _scheduleConfigService;
    private readonly IWindowActivationService _windowActivationService;
    private readonly ILoggingService _loggingService;
    private readonly IServiceProvider _serviceProvider;

    private string _currentDate = string.Empty;
    private bool _isInitialized = false;

    public MainPageViewModel(
        MoodDataService moodDataService,
        MoodDispatcherService dispatcherService,
        ScheduleConfigService scheduleConfigService,
        IWindowActivationService windowActivationService,
        ILoggingService loggingService,
        IServiceProvider serviceProvider)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        _scheduleConfigService = scheduleConfigService ?? throw new ArgumentNullException(nameof(scheduleConfigService));
        _windowActivationService = windowActivationService ?? throw new ArgumentNullException(nameof(windowActivationService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        InitializeCommands();
        UpdateCurrentDate();
    }

    #region Properties

    /// <summary>
    /// Current date formatted for display
    /// </summary>
    public string CurrentDate
    {
        get => _currentDate;
        private set => SetProperty(ref _currentDate, value);
    }

    #endregion

    #region Commands

    public ICommand RecordMoodCommand { get; private set; } = null!;
    public ICommand ViewHistoryCommand { get; private set; } = null!;
    public ICommand ViewGraphCommand { get; private set; } = null!;
    public ICommand SettingsCommand { get; private set; } = null!;
    public ICommand AboutCommand { get; private set; } = null!;

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the view model services and subscriptions
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            // Subscribe to dispatcher events
            _dispatcherService.DateChanged += OnDateChanged;
            _dispatcherService.AutoSaveOccurred += OnAutoSaveOccurred;
            _dispatcherService.MorningReminderOccurred += OnMorningReminderOccurred;
            _dispatcherService.EveningReminderOccurred += OnEveningReminderOccurred;

            _loggingService.Log("MainPageViewModel: Initialized with dispatcher service management");

            // Load and verify schedule configuration
            await LoadScheduleConfigurationAsync();

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error during initialization");
        }
    }

    /// <summary>
    /// Updates the current date display (called when page appears)
    /// </summary>
    public void RefreshCurrentDate()
    {
        UpdateCurrentDate();
        _loggingService.Log("MainPageViewModel: Current date display refreshed");
    }

    /// <summary>
    /// Manually trigger a check for date changes
    /// </summary>
    public void CheckForDateChanges()
    {
        try
        {
            _loggingService.Log("MainPageViewModel: Manual date change check requested");
            // The dispatcher service will handle the logic internally
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error during manual date change check");
        }
    }

    #endregion

    #region Events for UI Navigation

    public event EventHandler<NavigateToMoodRecordingEventArgs>? NavigateToMoodRecording;
    public event EventHandler? NavigateToHistory;
    public event EventHandler? NavigateToGraph;
    public event EventHandler? NavigateToSettings;
    public event EventHandler? NavigateToAbout;
    public event EventHandler<DisplayAlertEventArgs>? DisplayAlert;

    #endregion

    #region Private Methods

    private void InitializeCommands()
    {
        RecordMoodCommand = new RelayCommand(() => ExecuteRecordMood());
        ViewHistoryCommand = new RelayCommand(ExecuteViewHistory);
        ViewGraphCommand = new RelayCommand(ExecuteViewGraph);
        SettingsCommand = new RelayCommand(ExecuteSettings);
        AboutCommand = new RelayCommand(ExecuteAbout);
    }

    private void UpdateCurrentDate()
    {
        CurrentDate = DateTime.Today.ToString("dddd, MMMM dd, yyyy");
    }

    private async Task LoadScheduleConfigurationAsync()
    {
        try
        {
            var config = await _scheduleConfigService.LoadScheduleConfigAsync();
            _loggingService.Log($"MainPageViewModel: Schedule configuration loaded - Morning: {config.MorningTime}, Evening: {config.EveningTime}");

            // Trigger a save with the current configuration to clean up old overrides
            _loggingService.Log("MainPageViewModel: Triggering schedule cleanup to remove old overrides");
            await _scheduleConfigService.UpdateScheduleConfigAsync(config.MorningTime, config.EveningTime, newOverride: null);
            _loggingService.Log("MainPageViewModel: Schedule cleanup completed");
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error loading schedule configuration");
        }
    }

    private void ExecuteRecordMood()
    {
        try
        {
            var eventArgs = new NavigateToMoodRecordingEventArgs(_moodDataService, _dispatcherService, _loggingService);
            NavigateToMoodRecording?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error navigating to mood recording");
            DisplayAlert?.Invoke(this, new DisplayAlertEventArgs("Error", $"Failed to open mood recording: {ex.Message}", "OK"));
        }
    }

    private void ExecuteViewHistory()
    {
        try
        {
            NavigateToHistory?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error navigating to history");
            DisplayAlert?.Invoke(this, new DisplayAlertEventArgs("Error", $"Failed to open history: {ex.Message}", "OK"));
        }
    }

    private void ExecuteViewGraph()
    {
        try
        {
            NavigateToGraph?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error navigating to graph");
            DisplayAlert?.Invoke(this, new DisplayAlertEventArgs("Error", $"Failed to open graph: {ex.Message}", "OK"));
        }
    }

    private void ExecuteSettings()
    {
        try
        {
            NavigateToSettings?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error navigating to settings");
            DisplayAlert?.Invoke(this, new DisplayAlertEventArgs("Error", $"Failed to open settings: {ex.Message}", "OK"));
        }
    }

    private void ExecuteAbout()
    {
        try
        {
            NavigateToAbout?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error navigating to about");
            DisplayAlert?.Invoke(this, new DisplayAlertEventArgs("Error", $"Failed to open about page: {ex.Message}", "OK"));
        }
    }

    #endregion

    #region Event Handlers

    private async void OnDateChanged(object? sender, DateChangeEventArgs e)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _loggingService.Log($"MainPageViewModel: Date changed from {e.OldDate} to {e.NewDate}, auto-save decision: {e.AutoSaveDecision}");

                // Update current date display
                UpdateCurrentDate();

                // Show a notification to user if auto-save occurred
                if (e.AutoSaveDecision == AutoSaveDecision.SaveRecord)
                {
                    var alertArgs = new DisplayAlertEventArgs(
                        "Auto-Save", 
                        "Previous day's mood data has been automatically saved.", 
                        "OK"
                    );
                    DisplayAlert?.Invoke(this, alertArgs);
                    _loggingService.Log("MainPageViewModel: Previous day's mood was auto-saved");
                }
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error handling date change");
        }
    }

    private async void OnAutoSaveOccurred(object? sender, AutoSaveEventArgs e)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _loggingService.Log($"MainPageViewModel: Auto-save completed for {e.SavedDate}");
                // Could update UI to show auto-save occurred if needed
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error handling auto-save event");
        }
    }

    private async void OnMorningReminderOccurred(object? sender, MorningReminderEventArgs e)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _loggingService.Log($"MainPageViewModel: Morning reminder triggered - {e.Message}");

                // Activate/bring the window to the foreground
                await _windowActivationService.ActivateCurrentWindowAsync();

                // Show the morning mood reminder alert
                var alertArgs = new DisplayAlertEventArgs("Morning Mood Reminder", e.Message, "OK");
                DisplayAlert?.Invoke(this, alertArgs);
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error handling morning reminder event");
        }
    }

    private async void OnEveningReminderOccurred(object? sender, EveningReminderEventArgs e)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                _loggingService.Log($"MainPageViewModel: Evening reminder triggered - Type: {e.ReminderType}, Message: {e.Message}");

                // Activate/bring the window to the foreground
                await _windowActivationService.ActivateCurrentWindowAsync();

                // Show context-appropriate evening mood reminder alert
                string title = e.ReminderType switch
                {
                    EveningReminderType.EveningOnly => "Evening Mood Reminder",
                    EveningReminderType.OnlyMissedMorning => "Missed Morning Mood",
                    EveningReminderType.EveningAndMissedMorning => "Mood Reminder",
                    _ => "Mood Reminder"
                };

                var alertArgs = new DisplayAlertEventArgs(title, e.Message, "OK");
                DisplayAlert?.Invoke(this, alertArgs);
            });
        }
        catch (Exception ex)
        {
            _loggingService.LogException(ex, "MainPageViewModel: Error handling evening reminder event");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_dispatcherService != null)
        {
            _dispatcherService.DateChanged -= OnDateChanged;
            _dispatcherService.AutoSaveOccurred -= OnAutoSaveOccurred;
            _dispatcherService.MorningReminderOccurred -= OnMorningReminderOccurred;
            _dispatcherService.EveningReminderOccurred -= OnEveningReminderOccurred;
            _dispatcherService.Dispose();
        }
    }

    #endregion
}

#region Event Args Classes

/// <summary>
/// Event arguments for navigation to mood recording page
/// </summary>
public class NavigateToMoodRecordingEventArgs : EventArgs
{
    public MoodDataService MoodDataService { get; }
    public MoodDispatcherService DispatcherService { get; }
    public ILoggingService LoggingService { get; }

    public NavigateToMoodRecordingEventArgs(MoodDataService moodDataService, MoodDispatcherService dispatcherService, ILoggingService loggingService)
    {
        MoodDataService = moodDataService;
        DispatcherService = dispatcherService;
        LoggingService = loggingService;
    }
}

/// <summary>
/// Event arguments for displaying alerts
/// </summary>
public class DisplayAlertEventArgs : EventArgs
{
    public string Title { get; }
    public string Message { get; }
    public string Accept { get; }

    public DisplayAlertEventArgs(string title, string message, string accept)
    {
        Title = title;
        Message = message;
        Accept = accept;
    }
}

#endregion