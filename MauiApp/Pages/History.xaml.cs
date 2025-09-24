using System.Collections.ObjectModel;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using WorkMood.MauiApp.Pages;

namespace WorkMood.MauiApp.Pages;

/// <summary>
/// History page following MVVM pattern and SOLID principles
/// This page is only responsible for view-specific logic and delegates business logic to the ViewModel
/// </summary>
public partial class History : ContentPage
{
    private readonly HistoryViewModel _viewModel;
    private readonly IMoodEntryViewFactory _viewFactory;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Initializes a new instance of the History
    /// </summary>
    /// <param name="viewModel">The view model for this page</param>
    /// <param name="viewFactory">Factory for creating mood entry views</param>
    public History(HistoryViewModel viewModel, IMoodEntryViewFactory viewFactory)
    {
        InitializeComponent();
        
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _viewFactory = viewFactory ?? throw new ArgumentNullException(nameof(viewFactory));
        _navigationService = new NavigationService(this);
        
        BindingContext = _viewModel;
        
        // Subscribe to ViewModel property changes for UI updates that require programmatic handling
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        // Set up navigation handler for backwards compatibility
        _viewModel.SetVisualizationNavigationHandler(HandleVisualizationNavigationAsync);
    }

    /// <summary>
    /// Constructor for backwards compatibility and dependency injection support
    /// </summary>
    /// <param name="moodDataService">The mood data service (for backwards compatibility)</param>
    public History(MoodDataService? moodDataService = null)
    {
        InitializeComponent();
        
        // Create dependencies - in a proper DI container, these would be injected
        var dataService = moodDataService ?? new MoodDataService();
        var navigationService = new NavigationService(this);
        _viewFactory = new MoodEntryViewFactory();
        
        _viewModel = new HistoryViewModel(dataService, navigationService);
        BindingContext = _viewModel;
        _navigationService = navigationService;
        
        // Subscribe to ViewModel property changes for UI updates that require programmatic handling
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        // Set up navigation handler for backwards compatibility
        _viewModel.SetVisualizationNavigationHandler(HandleVisualizationNavigationAsync);
        
        // Load data on the main thread to ensure proper event handling
        Task.Run(async () =>
        {
            await _viewModel.InitializeAsync();
        });
    }

    /// <summary>
    /// Handles view model property changes that require programmatic UI updates
    /// </summary>
    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnViewModelPropertyChanged: {e.PropertyName}");
        
        if (e.PropertyName == nameof(HistoryViewModel.RecentEntries))
        {
            System.Diagnostics.Debug.WriteLine("Updating UI for RecentEntries change");
            // Update the UI when the recent entries collection changes
            await MainThread.InvokeOnMainThreadAsync(UpdateRecentEntriesUI);
        }
    }

    /// <summary>
    /// Handles the visualization command by delegating to the original navigation logic
    /// This maintains backwards compatibility while following MVVM pattern
    /// </summary>
    public async Task HandleVisualizationNavigationAsync()
    {
        // For backwards compatibility, create the visualization page with the service
        var moodDataService = new MoodDataService(); // In DI, this would be injected
        await _navigationService.NavigateAsync(() => new Visualization(moodDataService));
    }

    /// <summary>
    /// Updates the recent entries UI programmatically
    /// This is needed because we're using a factory pattern for creating complex UI elements
    /// </summary>
    private void UpdateRecentEntriesUI()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"UpdateRecentEntriesUI: Starting update with {_viewModel.RecentEntries.Count} entries");
            
            // Clear existing entries
            HistoryEntriesStack.Children.Clear();

            // Add new entries using the factory
            foreach (var entry in _viewModel.RecentEntries)
            {
                var entryView = _viewFactory.CreateEntryView(entry);
                HistoryEntriesStack.Children.Add(entryView);
                System.Diagnostics.Debug.WriteLine($"Added UI for entry: {entry.Date}");
            }
            
            System.Diagnostics.Debug.WriteLine($"UpdateRecentEntriesUI: Completed. UI now has {HistoryEntriesStack.Children.Count} children");
        }
        catch (Exception ex)
        {
            // Log error but don't break the UI
            System.Diagnostics.Debug.WriteLine($"Error updating recent entries UI: {ex.Message}");
        }
    }

    /// <summary>
    /// Called when the page appears - triggers data refresh
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        System.Diagnostics.Debug.WriteLine("History.OnAppearing called");
        
        try
        {
            await _viewModel.InitializeAsync();
            
            // Force UI update after initialization to ensure recent entries are displayed
            System.Diagnostics.Debug.WriteLine("Forcing UI update after ViewModel initialization");
            await MainThread.InvokeOnMainThreadAsync(UpdateRecentEntriesUI);
        }
        catch (Exception ex)
        {
            // Let the ViewModel handle error display
            System.Diagnostics.Debug.WriteLine($"Error loading data on page appearing: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up event subscriptions when the page is disposed
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
}