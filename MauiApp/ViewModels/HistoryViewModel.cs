using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the History Page following MVVM pattern and SOLID principles
/// </summary>
public class HistoryViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly INavigationService _navigationService;
    private Func<Task>? _visualizationNavigationHandler;

    // Backing fields for properties
    private string _totalEntries = "0";
    private string _overallAverage = "N/A";
    private string _last7Days = "N/A";
    private string _last30Days = "N/A";
    private string _trend = "N/A";
    private Color _trendColor = Colors.Gray;
    private bool _isLoading = false;
    private bool _hasNoData = false;
    private string _errorMessage = string.Empty;
    private bool _hasError = false;

    public HistoryViewModel(IMoodDataService moodDataService, INavigationService navigationService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        RecentEntries = new ObservableCollection<MoodEntry>();
        
        // Initialize commands
        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        OpenVisualizationCommand = new RelayCommand(async () => await OpenVisualizationAsync());
    }

    /// <summary>
    /// Sets the visualization navigation handler (for backwards compatibility)
    /// </summary>
    public void SetVisualizationNavigationHandler(Func<Task> handler)
    {
        _visualizationNavigationHandler = handler;
    }

    #region Properties

    /// <summary>
    /// Collection of recent mood entries
    /// </summary>
    public ObservableCollection<MoodEntry> RecentEntries { get; }

    /// <summary>
    /// Total number of entries text
    /// </summary>
    public string TotalEntries
    {
        get => _totalEntries;
        set => SetProperty(ref _totalEntries, value);
    }

    /// <summary>
    /// Overall average mood text
    /// </summary>
    public string OverallAverage
    {
        get => _overallAverage;
        set => SetProperty(ref _overallAverage, value);
    }

    /// <summary>
    /// Last 7 days average text
    /// </summary>
    public string Last7Days
    {
        get => _last7Days;
        set => SetProperty(ref _last7Days, value);
    }

    /// <summary>
    /// Last 30 days average text
    /// </summary>
    public string Last30Days
    {
        get => _last30Days;
        set => SetProperty(ref _last30Days, value);
    }

    /// <summary>
    /// Trend text
    /// </summary>
    public string Trend
    {
        get => _trend;
        set => SetProperty(ref _trend, value);
    }

    /// <summary>
    /// Color for the trend text
    /// </summary>
    public Color TrendColor
    {
        get => _trendColor;
        set => SetProperty(ref _trendColor, value);
    }

    /// <summary>
    /// Indicates if data is currently loading
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Indicates if there's no data to display
    /// </summary>
    public bool HasNoData
    {
        get => _hasNoData;
        set => SetProperty(ref _hasNoData, value);
    }

    /// <summary>
    /// Error message to display
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Indicates if there's an error to display
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to load data
    /// </summary>
    public ICommand LoadDataCommand { get; }

    /// <summary>
    /// Command to open visualization page
    /// </summary>
    public ICommand OpenVisualizationCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the view model data
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads mood data and statistics
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Load statistics
            await LoadStatisticsAsync();

            // Load recent entries
            await LoadRecentEntriesAsync();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load mood history: {ex.Message}";
            await _navigationService.ShowAlertAsync("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads mood statistics
    /// </summary>
    private async Task LoadStatisticsAsync()
    {
        var stats = await _moodDataService.GetMoodStatisticsAsync();
        
        TotalEntries = stats.TotalEntries.ToString();
        OverallAverage = stats.OverallAverageMood?.ToString("F1") ?? "N/A";
        Last7Days = stats.Last7DaysAverageMood?.ToString("F1") ?? "N/A";
        Last30Days = stats.Last30DaysAverageMood?.ToString("F1") ?? "N/A";
        Trend = stats.Trend;

        // Set trend color based on trend
        TrendColor = stats.Trend switch
        {
            "Improving" => Colors.Green,
            "Declining" => Colors.Red,
            "Stable" => Colors.Orange,
            _ => Colors.Gray
        };
    }

    /// <summary>
    /// Loads recent mood entries
    /// </summary>
    private async Task LoadRecentEntriesAsync()
    {
        var recentEntries = await _moodDataService.GetRecentMoodEntriesWithArchiveAsync(10);
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"LoadRecentEntriesAsync: Found {recentEntries.Count()} entries (including archived data if applicable)");
        
        RecentEntries.Clear();
        HasNoData = !recentEntries.Any();

        foreach (var entry in recentEntries)
        {
            RecentEntries.Add(entry);
            System.Diagnostics.Debug.WriteLine($"Added entry for {entry.Date}: Morning={entry.MorningMood}, Evening={entry.EveningMood}");
        }
        
        System.Diagnostics.Debug.WriteLine($"RecentEntries collection now has {RecentEntries.Count} items");
        
        // Manually raise PropertyChanged to notify the view that the collection has been updated
        OnPropertyChanged(nameof(RecentEntries));
        
        System.Diagnostics.Debug.WriteLine("PropertyChanged raised for RecentEntries");
    }

    /// <summary>
    /// Opens the visualization page
    /// </summary>
    private async Task OpenVisualizationAsync()
    {
        try
        {
            if (_visualizationNavigationHandler != null)
            {
                await _visualizationNavigationHandler();
            }
            else
            {
                await _navigationService.ShowAlertAsync("Navigation", "Visualization navigation handler not set", "OK");
            }
        }
        catch (Exception ex)
        {
            await _navigationService.ShowAlertAsync("Error", $"Failed to open visualization: {ex.Message}", "OK");
        }
    }

    #endregion
}