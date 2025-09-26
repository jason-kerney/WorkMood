using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the Graph Page showing mood data visualization
/// </summary>
public class GraphViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly ILineGraphService _lineGraphService;
    
    // Backing fields
    private DateRangeItem _selectedDateRange;
    private ImageSource? _graphImageSource;
    private bool _isLoading = false;
    private bool _hasGraphData = false;
    private bool _hasNoData = true;
    private string _statusMessage = string.Empty;
    private bool _hasStatusMessage = false;
    
    public GraphViewModel(IMoodDataService moodDataService, ILineGraphService lineGraphService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _lineGraphService = lineGraphService ?? throw new ArgumentNullException(nameof(lineGraphService));
        
        DateRanges = new ObservableCollection<DateRangeItem>();
        InitializeDateRanges();
        
        _selectedDateRange = DateRanges.First();
        
        // Initialize commands
        UpdateGraphCommand = new RelayCommand(async () => await UpdateGraphAsync());
        ExportGraphCommand = new RelayCommand(async () => await ExportGraphAsync());
        ShareGraphCommand = new RelayCommand(async () => await ShareGraphAsync());
    }
    
    #region Properties
    
    /// <summary>
    /// Collection of available date ranges
    /// </summary>
    public ObservableCollection<DateRangeItem> DateRanges { get; }
    
    /// <summary>
    /// Currently selected date range
    /// </summary>
    public DateRangeItem SelectedDateRange
    {
        get => _selectedDateRange;
        set
        {
            if (SetProperty(ref _selectedDateRange, value))
            {
                // Auto-update graph when date range changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Image source for the generated graph
    /// </summary>
    public ImageSource? GraphImageSource
    {
        get => _graphImageSource;
        set => SetProperty(ref _graphImageSource, value);
    }
    
    /// <summary>
    /// Whether the graph is currently loading
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    /// <summary>
    /// Whether there is graph data to display
    /// </summary>
    public bool HasGraphData
    {
        get => _hasGraphData;
        set => SetProperty(ref _hasGraphData, value);
    }
    
    /// <summary>
    /// Whether there is no data to display
    /// </summary>
    public bool HasNoData
    {
        get => _hasNoData;
        set => SetProperty(ref _hasNoData, value);
    }
    
    /// <summary>
    /// Status message to display to user
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    /// <summary>
    /// Whether to show the status message
    /// </summary>
    public bool HasStatusMessage
    {
        get => _hasStatusMessage;
        set => SetProperty(ref _hasStatusMessage, value);
    }
    
    #endregion
    
    #region Commands
    
    /// <summary>
    /// Command to update the graph with selected date range
    /// </summary>
    public ICommand UpdateGraphCommand { get; }
    
    /// <summary>
    /// Command to export the graph as PNG
    /// </summary>
    public ICommand ExportGraphCommand { get; }
    
    /// <summary>
    /// Command to share the graph
    /// </summary>
    public ICommand ShareGraphCommand { get; }
    
    #endregion
    
    #region Methods
    
    /// <summary>
    /// Loads initial data when the page appears
    /// </summary>
    public async Task LoadDataAsync()
    {
        await UpdateGraphAsync();
    }
    
    /// <summary>
    /// Updates the graph with the current date range selection
    /// </summary>
    private async Task UpdateGraphAsync()
    {
        IsLoading = true;
        ClearStatusMessage();
        
        try
        {
            var moodCollection = await _moodDataService.LoadMoodDataAsync();
            var filteredEntries = FilterEntriesByDateRange(moodCollection.Entries, _selectedDateRange.DateRange);
            
            if (!filteredEntries.Any())
            {
                HasGraphData = false;
                HasNoData = true;
                GraphImageSource = null;
                return;
            }
            
            var imageData = await _lineGraphService.GenerateLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, 800, 600);
            GraphImageSource = ImageSource.FromStream(() => new MemoryStream(imageData));
            
            HasGraphData = true;
            HasNoData = false;
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error generating graph: {ex.Message}");
            HasGraphData = false;
            HasNoData = true;
            GraphImageSource = null;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Exports the current graph as a PNG file
    /// </summary>
    private async Task ExportGraphAsync()
    {
        if (!HasGraphData)
        {
            ShowStatusMessage("No graph data to export.");
            return;
        }
        
        try
        {
            var moodCollection = await _moodDataService.LoadMoodDataAsync();
            var filteredEntries = FilterEntriesByDateRange(moodCollection.Entries, _selectedDateRange.DateRange);
            
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = $"mood-graph-{_selectedDateRange.DateRange.ToString().ToLower()}-{DateTime.Now:yyyyMMdd-HHmmss}.png";
            var filePath = Path.Combine(documentsPath, fileName);
            
            await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, filePath, 1200, 800);
            
            ShowStatusMessage($"Graph exported to: {filePath}");
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error exporting graph: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Shares the current graph
    /// </summary>
    private async Task ShareGraphAsync()
    {
        if (!HasGraphData)
        {
            ShowStatusMessage("No graph data to share.");
            return;
        }
        
        try
        {
            var moodCollection = await _moodDataService.LoadMoodDataAsync();
            var filteredEntries = FilterEntriesByDateRange(moodCollection.Entries, _selectedDateRange.DateRange);
            
            var tempPath = Path.GetTempPath();
            var fileName = $"mood-graph-{DateTime.Now:yyyyMMdd-HHmmss}.png";
            var filePath = Path.Combine(tempPath, fileName);
            
            await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, filePath, 1200, 800);
            
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share Mood Graph",
                File = new ShareFile(filePath)
            });
            
            ShowStatusMessage("Graph shared successfully!");
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error sharing graph: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Filters mood entries by the specified date range
    /// </summary>
    private List<MoodEntry> FilterEntriesByDateRange(IEnumerable<MoodEntry> entries, DateRange dateRange)
    {
        var startDate = dateRange.GetStartDate();
        var endDate = dateRange.GetEndDate();
        
        return entries
            .Where(e => e.Date >= startDate && e.Date <= endDate && e.Value.HasValue)
            .OrderBy(e => e.Date)
            .ToList();
    }
    
    /// <summary>
    /// Initializes the date range collection
    /// </summary>
    private void InitializeDateRanges()
    {
        var ranges = Enum.GetValues<DateRange>();
        foreach (var range in ranges)
        {
            DateRanges.Add(new DateRangeItem(range));
        }
    }
    
    /// <summary>
    /// Shows a status message to the user
    /// </summary>
    private void ShowStatusMessage(string message)
    {
        StatusMessage = message;
        HasStatusMessage = true;
        
        // Auto-clear status message after 5 seconds
        Task.Delay(5000).ContinueWith(_ => ClearStatusMessage());
    }
    
    /// <summary>
    /// Clears the status message
    /// </summary>
    private void ClearStatusMessage()
    {
        StatusMessage = string.Empty;
        HasStatusMessage = false;
    }
    
    #endregion
}

/// <summary>
/// Wrapper class for DateRange enum to provide display name
/// </summary>
public class DateRangeItem
{
    public DateRangeItem(DateRange dateRange)
    {
        DateRange = dateRange;
        DisplayName = dateRange.GetDisplayName();
    }
    
    public DateRange DateRange { get; }
    public string DisplayName { get; }
}