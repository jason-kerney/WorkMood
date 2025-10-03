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
    private readonly INavigationService _navigationService;
    private readonly IBrowserService _browserService;
    private readonly IVersionRetriever _versionRetriever;
    private readonly IMoodDataService _moodDataService;

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
    public AboutViewModel(INavigationService navigationService, IBrowserService browserService, IVersionRetriever versionRetriever, IMoodDataService moodDataService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));
        _versionRetriever = versionRetriever ?? throw new ArgumentNullException(nameof(versionRetriever));
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));

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