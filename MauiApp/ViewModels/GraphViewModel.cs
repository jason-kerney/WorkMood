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
    private bool _showDataPoints = true;
    private bool _showAxesAndGrid = true;
    private bool _showTitle = true;
    private ImageSource? _customBackgroundSource;
    private bool _hasCustomBackground = false;
    private int _customBackgroundWidth = 0;
    private int _customBackgroundHeight = 0;
    private string _customBackgroundPath = string.Empty;
    
    public GraphViewModel(IMoodDataService moodDataService, ILineGraphService lineGraphService)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _lineGraphService = lineGraphService ?? throw new ArgumentNullException(nameof(lineGraphService));
        
        DateRanges = new ObservableCollection<DateRangeItem>();
        InitializeDateRanges();
        
        _selectedDateRange = DateRanges.First();
        
        // Initialize commands
        ExportGraphCommand = new RelayCommand(async () => await ExportGraphAsync());
        ShareGraphCommand = new RelayCommand(async () => await ShareGraphAsync());
        LoadCustomBackgroundCommand = new RelayCommand(async () => await LoadCustomBackgroundAsync());
        ClearCustomBackgroundCommand = new RelayCommand(ClearCustomBackground);
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
                // Notify that GraphWidth has changed
                OnPropertyChanged(nameof(GraphWidth));
                OnPropertyChanged(nameof(ExportGraphWidth));
                
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
    
    /// <summary>
    /// Whether to show individual data points on the graph
    /// </summary>
    public bool ShowDataPoints
    {
        get => _showDataPoints;
        set
        {
            if (SetProperty(ref _showDataPoints, value))
            {
                // Auto-update graph when data points visibility changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Whether to show axes and grid lines on the graph
    /// </summary>
    public bool ShowAxesAndGrid
    {
        get => _showAxesAndGrid;
        set
        {
            if (SetProperty(ref _showAxesAndGrid, value))
            {
                // Auto-update graph when axes and grid visibility changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Whether to show the title on the graph
    /// </summary>
    public bool ShowTitle
    {
        get => _showTitle;
        set
        {
            if (SetProperty(ref _showTitle, value))
            {
                // Auto-update graph when title visibility changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Calculated width for the graph based on the selected date range
    /// </summary>
    public int GraphWidth => CalculateGraphWidth(_selectedDateRange?.DateRange ?? DateRange.Last7Days);
    
    /// <summary>
    /// Calculated width for export based on the selected date range
    /// </summary>
    public int ExportGraphWidth => CalculateExportGraphWidth(_selectedDateRange?.DateRange ?? DateRange.Last7Days);
    
    /// <summary>
    /// Custom background image source
    /// </summary>
    public ImageSource? CustomBackgroundSource
    {
        get => _customBackgroundSource;
        set => SetProperty(ref _customBackgroundSource, value);
    }
    
    /// <summary>
    /// Whether a custom background is loaded
    /// </summary>
    public bool HasCustomBackground
    {
        get => _hasCustomBackground;
        set 
        { 
            if (SetProperty(ref _hasCustomBackground, value))
            {
                OnPropertyChanged(nameof(EffectiveGraphWidth));
                OnPropertyChanged(nameof(EffectiveGraphHeight));
                OnPropertyChanged(nameof(DisplayWidth));
            }
        }
    }
    
    /// <summary>
    /// Width of the custom background image
    /// </summary>
    public int CustomBackgroundWidth
    {
        get => _customBackgroundWidth;
        set 
        { 
            if (SetProperty(ref _customBackgroundWidth, value))
            {
                OnPropertyChanged(nameof(EffectiveGraphWidth));
                OnPropertyChanged(nameof(EffectiveGraphHeight));
                OnPropertyChanged(nameof(DisplayWidth));
            }
        }
    }
    
    /// <summary>
    /// Height of the custom background image
    /// </summary>
    public int CustomBackgroundHeight
    {
        get => _customBackgroundHeight;
        set 
        { 
            if (SetProperty(ref _customBackgroundHeight, value))
            {
                OnPropertyChanged(nameof(EffectiveGraphWidth));
                OnPropertyChanged(nameof(EffectiveGraphHeight));
                OnPropertyChanged(nameof(DisplayWidth));
            }
        }
    }
    
    /// <summary>
    /// Path to the custom background image file
    /// </summary>
    public string CustomBackgroundPath
    {
        get => _customBackgroundPath;
        set => SetProperty(ref _customBackgroundPath, value);
    }
    
    /// <summary>
    /// Gets the effective graph width - either custom background width or calculated width
    /// </summary>
    public int EffectiveGraphWidth => HasCustomBackground ? CustomBackgroundWidth : GraphWidth;
    
    /// <summary>
    /// Gets the effective graph height - either custom background height or default height
    /// </summary>
    public int EffectiveGraphHeight => HasCustomBackground ? CustomBackgroundHeight : 900;
    
    /// <summary>
    /// Gets the display width for the image control - scaled appropriately for the view area
    /// </summary>
    public int DisplayWidth 
    {
        get
        {
            if (!HasCustomBackground)
                return GraphWidth;
                
            // Scale the background to fit within reasonable display bounds while maintaining aspect ratio
            const int maxDisplayWidth = 1000;
            const int maxDisplayHeight = 400;
            
            if (CustomBackgroundWidth <= maxDisplayWidth && CustomBackgroundHeight <= maxDisplayHeight)
                return CustomBackgroundWidth;
                
            double widthRatio = (double)maxDisplayWidth / CustomBackgroundWidth;
            double heightRatio = (double)maxDisplayHeight / CustomBackgroundHeight;
            double scale = Math.Min(widthRatio, heightRatio);
            
            return (int)(CustomBackgroundWidth * scale);
        }
    }
    
    #endregion
    
    #region Commands
    
    /// <summary>
    /// Command to export the graph as PNG
    /// </summary>
    public ICommand ExportGraphCommand { get; }
    
    /// <summary>
    /// Command to share the graph
    /// </summary>
    public ICommand ShareGraphCommand { get; }
    
    /// <summary>
    /// Command to load a custom background image
    /// </summary>
    public ICommand LoadCustomBackgroundCommand { get; }
    
    /// <summary>
    /// Command to clear the custom background image
    /// </summary>
    public ICommand ClearCustomBackgroundCommand { get; }
    
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
            
            byte[] imageData;
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                imageData = await _lineGraphService.GenerateLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, CustomBackgroundPath, EffectiveGraphWidth, EffectiveGraphHeight);
            }
            else
            {
                imageData = await _lineGraphService.GenerateLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, EffectiveGraphWidth, EffectiveGraphHeight);
            }
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
            
            var exportWidth = HasCustomBackground ? CustomBackgroundWidth : ExportGraphWidth;
            var exportHeight = HasCustomBackground ? CustomBackgroundHeight : 900;
            
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, filePath, CustomBackgroundPath, exportWidth, exportHeight);
            }
            else
            {
                await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, filePath, exportWidth, exportHeight);
            }
            
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
            
            var exportWidth = HasCustomBackground ? CustomBackgroundWidth : ExportGraphWidth;
            var exportHeight = HasCustomBackground ? CustomBackgroundHeight : 900;
            
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, filePath, CustomBackgroundPath, exportWidth, exportHeight);
            }
            else
            {
                await _lineGraphService.SaveLineGraphAsync(filteredEntries, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, filePath, exportWidth, exportHeight);
            }
            
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
    
    /// <summary>
    /// Calculates the graph width based on the date range with incremental increases
    /// </summary>
    /// <param name="dateRange">The selected date range</param>
    /// <returns>Calculated width in pixels</returns>
    private int CalculateGraphWidth(DateRange dateRange)
    {
        // Base width for the smallest time increment (Last7Days)
        const int baseWidth = 800;
        const int incrementWidth = 128; // Additional width per time increment step
        
        return dateRange switch
        {
            DateRange.Last7Days => baseWidth,
            DateRange.Last14Days => baseWidth + incrementWidth,
            DateRange.LastMonth => baseWidth + (incrementWidth * 2),
            DateRange.Last3Months => baseWidth + (incrementWidth * 3),
            DateRange.Last6Months => baseWidth + (incrementWidth * 4),
            DateRange.LastYear => baseWidth + (incrementWidth * 5),
            DateRange.Last2Years => baseWidth + (incrementWidth * 6),
            DateRange.Last3Years => baseWidth + (incrementWidth * 7),
            _ => baseWidth
        };
    }
    
    /// <summary>
    /// Calculates the export graph width based on the date range with incremental increases
    /// </summary>
    /// <param name="dateRange">The selected date range</param>
    /// <returns>Calculated width in pixels for export</returns>
    private int CalculateExportGraphWidth(DateRange dateRange)
    {
        // Base width for export (higher resolution)
        const int baseWidth = 1200;
        const int incrementWidth = 192; // Additional width per time increment step for export (1.5x display increment)
        
        return dateRange switch
        {
            DateRange.Last7Days => baseWidth,
            DateRange.Last14Days => baseWidth + incrementWidth,
            DateRange.LastMonth => baseWidth + (incrementWidth * 2),
            DateRange.Last3Months => baseWidth + (incrementWidth * 3),
            DateRange.Last6Months => baseWidth + (incrementWidth * 4),
            DateRange.LastYear => baseWidth + (incrementWidth * 5),
            DateRange.Last2Years => baseWidth + (incrementWidth * 6),
            DateRange.Last3Years => baseWidth + (incrementWidth * 7),
            _ => baseWidth
        };
    }
    
    /// <summary>
    /// Loads a custom background image from the user's file system
    /// </summary>
    private async Task LoadCustomBackgroundAsync()
    {
        try
        {
            var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.image" } },
                { DevicePlatform.Android, new[] { "image/*" } },
                { DevicePlatform.WinUI, new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff" } },
                { DevicePlatform.macOS, new[] { "png", "jpg", "jpeg", "bmp", "tiff" } }
            });

            var pickOptions = new PickOptions
            {
                PickerTitle = "Select Background Image",
                FileTypes = fileTypes
            };

            var file = await FilePicker.PickAsync(pickOptions);
            if (file != null)
            {
                // Load the image to get its dimensions
                using var stream = await file.OpenReadAsync();
                var imageSource = ImageSource.FromStream(() => stream);
                
                // For getting actual image dimensions, we'll need to load it differently
                // For now, we'll use a default size and the service can determine actual dimensions
                CustomBackgroundSource = ImageSource.FromFile(file.FullPath);
                CustomBackgroundPath = file.FullPath;
                HasCustomBackground = true;
                
                // Uncheck all options when custom background is loaded
                _showDataPoints = false;
                _showAxesAndGrid = false;
                _showTitle = false;
                OnPropertyChanged(nameof(ShowDataPoints));
                OnPropertyChanged(nameof(ShowAxesAndGrid));
                OnPropertyChanged(nameof(ShowTitle));
                
                // We'll set dimensions when the service processes the image
                // For now, set reasonable defaults
                CustomBackgroundWidth = 1200;
                CustomBackgroundHeight = 800;
                
                // Trigger graph update
                await UpdateGraphAsync();
                
                ShowStatusMessage("Custom background loaded successfully!");
            }
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error loading background: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clears the custom background and returns to normal graph sizing
    /// </summary>
    private void ClearCustomBackground()
    {
        HasCustomBackground = false;
        CustomBackgroundSource = null;
        CustomBackgroundPath = string.Empty;
        CustomBackgroundWidth = 0;
        CustomBackgroundHeight = 0;
        
        ShowStatusMessage("Custom background cleared.");
        
        // Trigger graph update to return to normal sizing
        _ = UpdateGraphAsync();
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