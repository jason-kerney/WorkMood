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
    private string _developerInfo = "Built with ❤️ using .NET MAUI";
    private string _iconUrl = "https://www.flaticon.com/free-icon/smiles_10949258?term=smiles&page=1&position=41&origin=style&related_id=10949258";

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
        OpenIconLinkCommand = new RelayCommand(ExecuteOpenIconLink, CanExecuteOpenIconLink);
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
    /// Gets the developer information
    /// </summary>
    public string DeveloperInfo
    {
        get => _developerInfo;
        set => SetProperty(ref _developerInfo, value);
    }

    /// <summary>
    /// Gets the icon attribution URL
    /// </summary>
    public string IconUrl
    {
        get => _iconUrl;
        set => SetProperty(ref _iconUrl, value);
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

    #region Commands

    /// <summary>
    /// Command to open the icon attribution link
    /// </summary>
    public ICommand OpenIconLinkCommand { get; }

    #endregion

    #region Command Implementations

    /// <summary>
    /// Executes the open icon link command
    /// </summary>
    private async void ExecuteOpenIconLink()
    {
        try
        {
            await _browserService.OpenAsync(IconUrl);
        }
        catch (Exception ex)
        {
            // Handle the case where the browser couldn't be opened
            await _navigationService.ShowAlertAsync("Error", $"Unable to open link: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Determines if the open icon link command can be executed
    /// </summary>
    /// <returns>True if the command can be executed</returns>
    private bool CanExecuteOpenIconLink()
    {
        return !string.IsNullOrEmpty(IconUrl);
    }

    #endregion
}