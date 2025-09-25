using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Services;
using WhatsYourVersion;

namespace WorkMood.MauiApp.Pages;

/// <summary>
/// About page following MVVM pattern and SOLID principles
/// This page is only responsible for view-specific logic and delegates business logic to the ViewModel
/// </summary>
public partial class About : ContentPage
{
    private readonly AboutViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the About page with dependency injection
    /// </summary>
    /// <param name="viewModel">The view model for this page</param>
    public About(AboutViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    /// <summary>
    /// Constructor for backwards compatibility and dependency injection support
    /// </summary>
    /// <param name="navigationService">The navigation service (optional for DI scenarios)</param>
    /// <param name="browserService">The browser service (optional for DI scenarios)</param>
    /// <param name="versionRetriever">The version retriever service (optional for DI scenarios)</param>
    /// <param name="moodDataService">The mood data service (optional for DI scenarios)</param>
    public About(INavigationService? navigationService = null, IBrowserService? browserService = null, IVersionRetriever? versionRetriever = null, IMoodDataService? moodDataService = null)
    {
        InitializeComponent();
        
        // Create dependencies following Dependency Inversion Principle
        var navService = navigationService ?? new NavigationService(this);
        var browService = browserService ?? new BrowserService();
        var verService = versionRetriever ?? new VersionRetriever(AssemblyWrapper.From<App>());
        var moodService = moodDataService ?? new MoodDataService(new DataArchiveService());
        
        // Create and set the ViewModel
        _viewModel = new AboutViewModel(navService, browService, verService, moodService);
        BindingContext = _viewModel;
    }
}