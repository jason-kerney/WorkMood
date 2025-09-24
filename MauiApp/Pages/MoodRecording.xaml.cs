using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class MoodRecording : ContentPage
{
    private readonly MoodRecordingViewModel _viewModel;
    private readonly INavigationService _navigationService;

    public MoodRecording(MoodDataService moodDataService, MoodDispatcherService dispatcherService, ILoggingService loggingService)
    {
        InitializeComponent();
        
        _viewModel = new MoodRecordingViewModel(moodDataService, dispatcherService, loggingService);
        BindingContext = _viewModel;
        _navigationService = new NavigationService(this);
        
        // Subscribe to ViewModel events
        _viewModel.ErrorOccurred += OnErrorOccurred;
        _viewModel.NavigateBackRequested += OnNavigateBackRequested;
    }

    private async void OnErrorOccurred(object? sender, string errorMessage)
    {
        await _navigationService.ShowErrorAsync(errorMessage);
    }

    private async void OnNavigateBackRequested(object? sender, EventArgs e)
    {
        await _navigationService.GoBackAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh data when the page appears
        await _viewModel.RefreshMoodDataAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from events to prevent memory leaks
        _viewModel.ErrorOccurred -= OnErrorOccurred;
        _viewModel.NavigateBackRequested -= OnNavigateBackRequested;
    }
}