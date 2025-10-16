using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Services;
using WhatsYourVersion;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the About page following MVVM pattern and SOLID principles
/// Responsible for handling About page business logic and UI interactions
/// </summary>
public class AboutViewModel : ViewModelBase
{
    private readonly IBrowserService _browserService;
    private readonly IVersionRetriever _versionRetriever;
    private readonly IMoodDataService _moodDataService;
    private readonly ILoggingService _loggingService;

    private string _appTitle = "WorkMood - Daily Mood Tracker";
    private string _appVersion = string.Empty;
    private string _appDescription = "WorkMood helps you track your daily mood and build healthy emotional awareness habits. Record your feelings throughout the day and view your mood patterns over time.";



    /// <summary>
    /// Initializes a new instance of the AboutViewModel
    /// </summary>
    /// <param name="navigationService">Service for navigation operations</param>
    /// <param name="browserService">Service for opening external URLs</param>
    /// <param name="versionRetriever">Service for retrieving version information</param>
    /// <param name="moodDataService">Service for mood data operations</param>
    /// <param name="loggingService">Service for logging operations</param>
    public AboutViewModel(IBrowserService browserService, IVersionRetriever versionRetriever, IMoodDataService moodDataService, ILoggingService loggingService)
    {
        _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));
        _versionRetriever = versionRetriever ?? throw new ArgumentNullException(nameof(versionRetriever));
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

        // Initialize version information
        InitializeVersionInfo();

        // Initialize commands
    }

    #region Properties

    /// <summary>
    /// Gets the application title
    /// </summary>
    public string AppTitle
    {
        get => _appTitle;
        set => SetProperty(ref _appTitle, value);
    }

    /// <summary>
    /// Gets the application version
    /// </summary>
    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    /// <summary>
    /// Gets the application description
    /// </summary>
    public string AppDescription
    {
        get => _appDescription;
        set => SetProperty(ref _appDescription, value);
    }

    /// <summary>
    /// Gets or sets whether logging is enabled
    /// </summary>
    public bool IsLoggingEnabled
    {
        get => _loggingService.IsEnabled;
        set
        {
            if (_loggingService.IsEnabled != value)
            {
                _loggingService.IsEnabled = value;
                OnPropertyChanged();
                
                // Log the change (if logging is enabled)
                if (value)
                {
                    _loggingService.Log("Logging has been enabled via About page");
                }
                // Note: We can't log when disabling because logging would be disabled
            }
        }
    }

    /// <summary>
    /// Gets the available log levels for the picker (ordered by hierarchy)
    /// </summary>
    public List<string> LogLevels { get; } = new List<string>
    {
        "Info",
        "Debug", 
        "Warning",
        "Error"
    };

    /// <summary>
    /// Gets or sets the selected minimum log level
    /// </summary>
    public string SelectedLogLevel
    {
        get => _loggingService.MinimumLogLevel.ToString();
        set
        {
            if (Enum.TryParse<LogLevel>(value, out var logLevel) && _loggingService.MinimumLogLevel != logLevel)
            {
                _loggingService.MinimumLogLevel = logLevel;
                OnPropertyChanged();
                
                // Log the change if logging is enabled
                if (_loggingService.IsEnabled)
                {
                    _loggingService.Log($"Minimum log level changed to {logLevel} via About page");
                }
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes the version information from the version retriever
    /// </summary>
    private void InitializeVersionInfo()
    {
        try
        {
            var versionInfo = _versionRetriever.GetVersion();
            AppVersion = $"Version {versionInfo.Version}";
        }
        catch (Exception)
        {
            // Fallback to default version if retrieval fails
            AppVersion = "Version 0.1.0";
        }
    }

    #endregion




}