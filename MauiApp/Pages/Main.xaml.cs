using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Pages;

namespace WorkMood.MauiApp.Pages;

public partial class Main : ContentPage
{
    private readonly MainPageViewModel _viewModel;
    private readonly INavigationService _navigationService;

    public Main(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
        _navigationService = new NavigationService(this);
        
        SubscribeToViewModelEvents();
    }
    private void SubscribeToViewModelEvents()
    {
        _viewModel.NavigateToMoodRecording += OnNavigateToMoodRecording;
        _viewModel.NavigateToHistory += OnNavigateToHistory;
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
        await _navigationService.NavigateAsync(() => new History());
    }

    private async void OnNavigateToSettings(object? sender, EventArgs e)
    {
        // Get the schedule config service from DI if available
        var scheduleConfigService = Handler?.MauiContext?.Services.GetService<ScheduleConfigService>();
        if (scheduleConfigService != null)
        {
            await _navigationService.NavigateAsync(() => new Settings(scheduleConfigService));
        }
        else
        {
            await _navigationService.ShowErrorAsync("Failed to get schedule configuration service");
        }
    }

    private async void OnNavigateToAbout(object? sender, EventArgs e)
    {
        await _navigationService.NavigateAsync(() => new About());
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