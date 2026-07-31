using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Processors;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Adapters;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the visualization page following MVVM pattern and SOLID principles
/// </summary>
public class VisualizationViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly INavigationService _navigationService;
    private readonly ILineGraphService _lineGraphService;
    private readonly IDateShim _dateShim;
    
    private MoodVisualizationData? _currentVisualization;
    private string _dateRangeText = "Loading...";
    private string _summaryText = "Loading summary...";
    private bool _isLoading = true;
    private bool _hasData = false;
    private ImageSource? _chartImageSource;

    public VisualizationViewModel(
        IMoodDataService moodDataService,
        INavigationService navigationService,
        ILineGraphService lineGraphService,
        IDateShim dateShim,
        bool autoLoad = true)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _lineGraphService = lineGraphService ?? throw new ArgumentNullException(nameof(lineGraphService));
        _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
        
        DailyDataItems = new ObservableCollection<DailyDataItemViewModel>();
        
        // Initialize commands
        RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
        BackToHistoryCommand = new RelayCommand(async () => await NavigateBackAsync());
        
        // Load data on initialization
        if (autoLoad)
        {
            _ = LoadVisualizationDataAsync();
        }
    }

    #region Properties

    /// <summary>
    /// The current visualization data
    /// </summary>
    public MoodVisualizationData? CurrentVisualization
    {
        get => _currentVisualization;
        private set => SetProperty(ref _currentVisualization, value);
    }

    /// <summary>
    /// Display text for the date range
    /// </summary>
    public string DateRangeText
    {
        get => _dateRangeText;
        private set => SetProperty(ref _dateRangeText, value);
    }

    /// <summary>
    /// Summary text for the visualization
    /// </summary>
    public string SummaryText
    {
        get => _summaryText;
        private set => SetProperty(ref _summaryText, value);
    }

    /// <summary>
    /// Indicates whether data is currently being loaded
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Indicates whether there is data available
    /// </summary>
    public bool HasData
    {
        get => _hasData;
        private set => SetProperty(ref _hasData, value);
    }

    /// <summary>
    /// Image source used by the chart host on the visualization page.
    /// </summary>
    public ImageSource? ChartImageSource
    {
        get => _chartImageSource;
        private set => SetProperty(ref _chartImageSource, value);
    }

    /// <summary>
    /// Collection of daily data items for display
    /// </summary>
    public ObservableCollection<DailyDataItemViewModel> DailyDataItems { get; }

    #endregion

    #region Commands

    /// <summary>
    /// Command to refresh the data
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Command to navigate back to history
    /// </summary>
    public ICommand BackToHistoryCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Loads visualization data when page appears
    /// </summary>
    public async Task OnAppearingAsync()
    {
        await LoadVisualizationDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads the visualization data asynchronously
    /// </summary>
    private async Task LoadVisualizationDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Show loading state
            DateRangeText = "Loading visualization...";
            SummaryText = "Loading summary...";
            DailyDataItems.Clear();
            ChartImageSource = null;

            // Get visualization data
            CurrentVisualization = await _moodDataService.GetTwoWeekVisualizationAsync();

            // Build chart image from the consolidated graph pipeline.
            var moodCollection = await _moodDataService.LoadMoodDataAsync();
            await UpdateChartImageAsync(moodCollection.Entries);
            
            // Update date range
            DateRangeText = $"{CurrentVisualization.StartDate:MMM dd} - {CurrentVisualization.EndDate:MMM dd, yyyy}";
            
            // Get detailed data for the list
            var dailyInfoList = await VisualizationDataAdapter.GetMoodDayInfoListAsync(_moodDataService);
            
            // Populate daily data items
            await PopulateDailyDataItemsAsync(dailyInfoList);
            
            // Update summary
            SummaryText = await VisualizationDataAdapter.GetVisualizationSummaryAsync(_moodDataService);
            
            HasData = CurrentVisualization.DailyValues?.Any(d => d.HasData) == true;
        }
        catch (Exception ex)
        {
            await HandleVisualizationErrorAsync(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Populates the daily data items collection
    /// </summary>
    private Task PopulateDailyDataItemsAsync(List<MoodDayInfo> dailyInfoList)
    {
        var items = dailyInfoList.Select(dayInfo => new DailyDataItemViewModel(
            dayInfo.Date,
            dayInfo.Value,
            dayInfo.HasData,
            GetColorForDay(dayInfo.Date),
            dayInfo.ValueDescription
        )).ToList();

        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            dispatcher.Dispatch(() =>
            {
                DailyDataItems.Clear();
                foreach (var item in items)
                {
                    DailyDataItems.Add(item);
                }
            });
            return Task.CompletedTask;
        }

        DailyDataItems.Clear();
        foreach (var item in items)
        {
            DailyDataItems.Add(item);
        }

        return Task.CompletedTask;
    }

    private async Task UpdateChartImageAsync(IEnumerable<MoodEntry> entries)
    {
        var dateRange = GetVisualizationDateRange();

        var entriesInRange = entries
            .Where(entry => entry.Date >= dateRange.StartDate && entry.Date <= dateRange.EndDate)
            .OrderBy(entry => entry.Date)
            .ToList();

        if (entriesInRange.Count == 0)
        {
            ChartImageSource = null;
            return;
        }

        var imageData = await _lineGraphService.GenerateGraphAsync(
            entriesInRange,
            GraphMode.Impact,
            dateRange,
            showDataPoints: true,
            showAxesAndGrid: true,
            showTitle: false,
            showTrendLine: false,
            lineColor: Colors.DarkBlue,
            width: 800,
            height: 220);

        ChartImageSource = imageData.Length == 0
            ? null
            : new StreamImageSource
            {
                Stream = _ => Task.FromResult<Stream>(new MemoryStream(imageData))
            };
    }

    private DateRangeInfo GetVisualizationDateRange()
    {
        var endDate = _dateShim.GetTodayDate().AddDays(-1);
        return new DateRangeInfo(DateRange.Last14Days, endDate);
    }

    /// <summary>
    /// Gets the color for a specific day based on visualization data
    /// </summary>
    private Color GetColorForDay(DateOnly date)
    {
        if (CurrentVisualization?.DailyValues != null)
        {
            var dayData = CurrentVisualization.DailyValues.FirstOrDefault(d => d.Date == date);
            return dayData?.Color ?? Colors.LightGray;
        }
        return Colors.LightGray;
    }

    /// <summary>
    /// Handles visualization loading errors
    /// </summary>
    private async Task HandleVisualizationErrorAsync(Exception ex)
    {
        var errorMessage = ex.Message.Contains("Sequence contains no elements") 
            ? "No mood data found. Start recording your daily moods to see visualizations here!"
            : $"Failed to load visualization: {ex.Message}";

        // Set friendly fallback text
        DateRangeText = "No data available";
        SummaryText = "Start recording your daily moods to see trends and visualizations!";
        HasData = false;
        ChartImageSource = null;
        
        // Create empty visualization for display
        await CreateEmptyVisualizationAsync();
    }

    /// <summary>
    /// Creates an empty visualization when no data is available
    /// </summary>
    private Task CreateEmptyVisualizationAsync()
    {
        var endDate = _dateShim.GetTodayDate().AddDays(-1); // End yesterday to prevent anchoring bias
        var startDate = endDate.AddDays(-13); // 14 days total

        var emptyDailyValues = new DailyMoodValue[14];
        for (int day = 0; day < 14; day++)
        {
            emptyDailyValues[day] = new DailyMoodValue
            {
                Date = startDate.AddDays(day),
                Value = null,
                HasData = false,
                Color = Colors.LightGray
            };
        }

        CurrentVisualization = new MoodVisualizationData
        {
            DailyValues = emptyDailyValues,
            DisplayValues = VisualizationDisplayValueFilter.BuildDisplayValues(emptyDailyValues),
            StartDate = startDate,
            EndDate = endDate,
            Width = 280,
            Height = 100,
            MaxAbsoluteValue = 1.0
        };

        if (Application.Current?.Dispatcher is { } dispatcher)
        {
            dispatcher.Dispatch(() =>
            {
                DailyDataItems.Clear();
                DailyDataItems.Add(new DailyDataItemViewModel(
                    endDate, // Show yesterday instead of today
                    null,
                    false,
                    Colors.Gray,
                    "No mood data available for the last 2 weeks.\nStart recording your daily moods to see trends here!"
                ));
            });
            return Task.CompletedTask;
        }

        DailyDataItems.Clear();
        DailyDataItems.Add(new DailyDataItemViewModel(
            endDate,
            null,
            false,
            Colors.Gray,
            "No mood data available for the last 2 weeks.\nStart recording your daily moods to see trends here!"
        ));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Refreshes the visualization data
    /// </summary>
    private async Task RefreshDataAsync()
    {
        await LoadVisualizationDataAsync();
    }

    /// <summary>
    /// Navigates back to the history page
    /// </summary>
    private async Task NavigateBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    #endregion
}

/// <summary>
/// ViewModel for individual daily data items
/// </summary>
public class DailyDataItemViewModel
{
    public DailyDataItemViewModel(DateOnly date, double? value, bool hasData, Color color, string description)
    {
        Date = date;
        Value = value;
        HasData = hasData;
        Color = color;
        Description = description;
        DateString = date.ToString("MM/dd");
        ValueString = hasData && value.HasValue ? $"Value: {value.Value:F1}" : "";
    }

    public DateOnly Date { get; }
    public double? Value { get; }
    public bool HasData { get; }
    public Color Color { get; }
    public string Description { get; }
    public string DateString { get; }
    public string ValueString { get; }
}