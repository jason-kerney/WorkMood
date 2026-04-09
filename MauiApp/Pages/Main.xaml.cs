using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Pages;
using WorkMood.MauiApp.Shims;
using Microsoft.Extensions.DependencyInjection;

namespace WorkMood.MauiApp.Pages;

public partial class Main : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public Main(MainPageViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        BindingContext = _viewModel;
        _navigationService = new NavigationService(this);
        
        SubscribeToViewModelEvents();
    }
    private void SubscribeToViewModelEvents()
    {
        _viewModel.NavigateToMoodRecording += OnNavigateToMoodRecording;
        _viewModel.NavigateToHistory += OnNavigateToHistory;
        _viewModel.NavigateToGraph += OnNavigateToGraph;
        _viewModel.NavigateToSettings += OnNavigateToSettings;
        _viewModel.NavigateToAbout += OnNavigateToAbout;
        _viewModel.DisplayAlert += OnDisplayAlert;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        _viewModel.RefreshCurrentDate();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    #region Navigation Event Handlers

    private async void OnNavigateToMoodRecording(object? sender, NavigateToMoodRecordingEventArgs e)
    {
        await _navigationService.NavigateAsync(() => new MoodRecording(e.MoodDataService, e.DispatcherService, e.LoggingService));
    }

    private async void OnNavigateToHistory(object? sender, EventArgs e)
    {
        // Get singleton services from DI to ensure consistent data path and unified logging
        var moodDataService = _serviceProvider.GetRequiredService<IMoodDataService>();
        var loggingService = _serviceProvider.GetRequiredService<ILoggingService>();
        await _navigationService.NavigateAsync(() => new History(moodDataService, loggingService));
    }

    private async void OnNavigateToGraph(object? sender, EventArgs e)
    {
        var graphViewModel = Handler?.MauiContext?.Services.GetService<GraphViewModel>();
        if (graphViewModel != null)
        {
            await _navigationService.NavigateAsync(() => new Graph(graphViewModel));
        }
        else
        {
            await _navigationService.ShowErrorAsync("Failed to get graph view model service");
        }
    }

    private async void OnNavigateToSettings(object? sender, EventArgs e)
    {
        // Get the schedule config service from DI if available
        var scheduleConfigService = Handler?.MauiContext?.Services.GetService<ScheduleConfigService>();
        var folderPickerShim = Handler?.MauiContext?.Services.GetService<IFolderPickerShim>();
        var pathValidationShim = Handler?.MauiContext?.Services.GetService<IPathValidationShim>();

        if (scheduleConfigService != null && folderPickerShim != null && pathValidationShim != null)
        {
            await _navigationService.NavigateAsync(() => new Settings(scheduleConfigService, folderPickerShim, pathValidationShim));
        }
        else
        {
            await _navigationService.ShowErrorAsync("Failed to get required settings services");
        }
    }

    private async void OnNavigateToAbout(object? sender, EventArgs e)
    {
        await _navigationService.NavigateAsync(() => _serviceProvider.GetRequiredService<About>());
    }

    private async void OnDisplayAlert(object? sender, DisplayAlertEventArgs e)
    {
        await _navigationService.ShowAlertAsync(e.Title, e.Message, e.Accept);
    }

    #endregion

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
    }
}