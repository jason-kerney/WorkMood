using System.Collections.ObjectModel;
using System.Windows.Input;
using WorkMood.MauiApp.Infrastructure;
using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;

namespace WorkMood.MauiApp.ViewModels;

/// <summary>
/// ViewModel for the Graph Page showing mood data visualization
/// </summary>
public class GraphViewModel : ViewModelBase
{
    private readonly IMoodDataService _moodDataService;
    private readonly ILineGraphService _lineGraphService;
    private readonly IDateShim _dateShim;

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
    private Color _selectedLineColor = Colors.Blue;
    private bool _isColorPickerVisible = false;
    private GraphMode _selectedGraphMode = GraphMode.Impact;
    private GraphModeItem _selectedGraphModeItem = null!;
    private double _availableContainerWidth = 800; // Default fallback
    private double _availableContainerHeight = 400; // Default fallback
    
    public GraphViewModel(IMoodDataService moodDataService, ILineGraphService lineGraphService) : this(moodDataService, lineGraphService, new DateShim()) { }
    
    public GraphViewModel(IMoodDataService moodDataService, ILineGraphService lineGraphService, IDateShim dateShim)
    {
        _moodDataService = moodDataService ?? throw new ArgumentNullException(nameof(moodDataService));
        _lineGraphService = lineGraphService ?? throw new ArgumentNullException(nameof(lineGraphService));
        _dateShim = dateShim ?? throw new ArgumentNullException(nameof(dateShim));
        DateRanges = new ObservableCollection<DateRangeItem>();
        InitializeDateRanges();

        GraphModes = new ObservableCollection<GraphModeItem>();
        InitializeGraphModes();

        _selectedDateRange = DateRanges.First();
        _selectedGraphModeItem = GraphModes.First();

        // Initialize commands
        ExportGraphCommand = new RelayCommand(async () => await ExportGraphAsync());
        ShareGraphCommand = new RelayCommand(async () => await ShareGraphAsync());
        LoadCustomBackgroundCommand = new RelayCommand(async () => await LoadCustomBackgroundAsync());
        ClearCustomBackgroundCommand = new RelayCommand(ClearCustomBackground);
        ToggleColorPickerCommand = new RelayCommand(ToggleColorPicker);
        SelectColorCommand = new RelayCommand<string>(SelectColor);
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
    
    private bool _showTrendLine = false;
    
    /// <summary>
    /// Whether to show the trend line on the graph
    /// </summary>
    public bool ShowTrendLine
    {
        get => _showTrendLine;
        set
        {
            if (SetProperty(ref _showTrendLine, value))
            {
                // Auto-update graph when trend line visibility changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Calculated width for the graph based on the selected date range
    /// </summary>
    public int GraphWidth => CalculateGraphWidth(_selectedDateRange?.DateRange ?? new DateRangeInfo(DateRange.Last7Days, _dateShim));
    
    /// <summary>
    /// Calculated width for export based on the selected date range
    /// </summary>
    public int ExportGraphWidth => CalculateExportGraphWidth(_selectedDateRange?.DateRange ?? new DateRangeInfo(DateRange.Last7Days, _dateShim));
    
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
    /// Selected color for the graph line
    /// </summary>
    public Color SelectedLineColor
    {
        get => _selectedLineColor;
        set
        {
            if (SetProperty(ref _selectedLineColor, value))
            {
                // Auto-update graph when color changes
                _ = UpdateGraphAsync();
            }
        }
    }
    
    /// <summary>
    /// Whether the inline color picker is visible
    /// </summary>
    public bool IsColorPickerVisible
    {
        get => _isColorPickerVisible;
        set => SetProperty(ref _isColorPickerVisible, value);
    }

    /// <summary>
    /// Selected graph mode (Impact or Average)
    /// </summary>
    public GraphMode SelectedGraphMode
    {
        get => _selectedGraphMode;
        set
        {
            if (SetProperty(ref _selectedGraphMode, value))
            {
                // Auto-update graph when graph mode changes
                _ = UpdateGraphAsync();
            }
        }
    }

    /// <summary>
    /// Collection of available graph modes for the picker
    /// </summary>
    public ObservableCollection<GraphModeItem> GraphModes { get; }

    /// <summary>
    /// Selected graph mode item for the picker
    /// </summary>
    public GraphModeItem SelectedGraphModeItem
    {
        get => _selectedGraphModeItem;
        set
        {
            if (SetProperty(ref _selectedGraphModeItem, value) && value != null)
            {
                SelectedGraphMode = value.GraphMode;
            }
        }
    }
    
    // RGB Color slider properties
    /// <summary>
    /// Red component of custom color (0-255)
    /// </summary>
    public double RedValue
    {
        get => _selectedLineColor.Red * 255;
        set => UpdateCustomColor((float)(value / 255.0), _selectedLineColor.Green, _selectedLineColor.Blue);
    }
    
    /// <summary>
    /// Green component of custom color (0-255)
    /// </summary>
    public double GreenValue
    {
        get => _selectedLineColor.Green * 255;
        set => UpdateCustomColor(_selectedLineColor.Red, (float)(value / 255.0), _selectedLineColor.Blue);
    }
    
    /// <summary>
    /// Blue component of custom color (0-255)
    /// </summary>
    public double BlueValue
    {
        get => _selectedLineColor.Blue * 255;
        set => UpdateCustomColor(_selectedLineColor.Red, _selectedLineColor.Green, (float)(value / 255.0));
    }
    
    /// <summary>
    /// Updates the custom color based on RGB values
    /// </summary>
    private void UpdateCustomColor(float red, float green, float blue)
    {
        var newColor = Color.FromRgba(red, green, blue, 1.0f);
        if (SelectedLineColor != newColor)
        {
            SelectedLineColor = newColor;
            OnPropertyChanged(nameof(RedValue));
            OnPropertyChanged(nameof(GreenValue));
            OnPropertyChanged(nameof(BlueValue));
        }
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
    public double DisplayWidth 
    {
        get
        {
            if (!HasCustomBackground)
            {
                // Scale regular graphs to fit available width
                return Math.Min(GraphWidth, _availableContainerWidth);
            }
                
            // Scale custom background to fit within actual available space while maintaining aspect ratio
            if (CustomBackgroundWidth <= _availableContainerWidth && CustomBackgroundHeight <= _availableContainerHeight)
                return CustomBackgroundWidth;
                
            double widthRatio = _availableContainerWidth / CustomBackgroundWidth;
            double heightRatio = _availableContainerHeight / CustomBackgroundHeight;
            double scale = Math.Min(widthRatio, heightRatio);
            
            return CustomBackgroundWidth * scale;
        }
    }
    
    /// <summary>
    /// Gets the display height for the image control - scaled appropriately for the view area
    /// </summary>
    public double DisplayHeight 
    {
        get
        {
            if (!HasCustomBackground)
            {
                // For regular graphs, scale to fit available height while maintaining reasonable proportions
                const double defaultAspectRatio = 2.0; // width:height ratio
                var calculatedHeight = _availableContainerHeight - 20; // Leave some margin
                var maxHeightForWidth = GraphWidth / defaultAspectRatio;
                return Math.Min(calculatedHeight, maxHeightForWidth);
            }
                
            // Scale custom background to fit within actual available space while maintaining aspect ratio
            if (CustomBackgroundWidth <= _availableContainerWidth && CustomBackgroundHeight <= _availableContainerHeight)
                return CustomBackgroundHeight;
                
            double widthRatio = _availableContainerWidth / CustomBackgroundWidth;
            double heightRatio = _availableContainerHeight / CustomBackgroundHeight;
            double scale = Math.Min(widthRatio, heightRatio);
            
            return CustomBackgroundHeight * scale;
        }
    }
    
    /// <summary>
    /// Updates the available container size and triggers display property updates
    /// </summary>
    public void UpdateContainerSize(double width, double height)
    {
        if (Math.Abs(_availableContainerWidth - width) > 1 || Math.Abs(_availableContainerHeight - height) > 1)
        {
            _availableContainerWidth = Math.Max(100, width - 40); // Account for padding/margins
            _availableContainerHeight = Math.Max(100, height - 40); // Account for padding/margins
            
            OnPropertyChanged(nameof(DisplayWidth));
            OnPropertyChanged(nameof(DisplayHeight));
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
    
    /// <summary>
    /// Command to toggle the inline color picker visibility
    /// </summary>
    public ICommand ToggleColorPickerCommand { get; }
    
    /// <summary>
    /// Command to select a color from the inline color picker
    /// </summary>
    public ICommand SelectColorCommand { get; }

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
            
            // Use consolidated method for all graph modes
            byte[] imageData;
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                imageData = await _lineGraphService.GenerateGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, CustomBackgroundPath, SelectedLineColor, EffectiveGraphWidth, EffectiveGraphHeight);
            }
            else
            {
                imageData = await _lineGraphService.GenerateGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, SelectedLineColor, EffectiveGraphWidth, EffectiveGraphHeight);
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

            // Use consolidated method for all graph modes
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                await _lineGraphService.SaveGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, filePath, CustomBackgroundPath, SelectedLineColor, exportWidth, exportHeight);
            }
            else
            {
                await _lineGraphService.SaveGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, filePath, SelectedLineColor, exportWidth, exportHeight);
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
            
            // Use consolidated method for all graph modes
            if (HasCustomBackground && !string.IsNullOrEmpty(CustomBackgroundPath))
            {
                await _lineGraphService.SaveGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, filePath, CustomBackgroundPath, SelectedLineColor, exportWidth, exportHeight);
            }
            else
            {
                await _lineGraphService.SaveGraphAsync(filteredEntries, SelectedGraphMode, _selectedDateRange.DateRange, _showDataPoints, _showAxesAndGrid, _showTitle, _showTrendLine, filePath, SelectedLineColor, exportWidth, exportHeight);
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
    private List<MoodEntry> FilterEntriesByDateRange(IEnumerable<MoodEntry> entries, DateRangeInfo dateRange)
    {
        var startDate = dateRange.StartDate;
        var endDate = dateRange.EndDate;

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
            DateRanges.Add(new DateRangeItem(range, _dateShim));
        }
    }

    /// <summary>
    /// Initializes the graph mode collection
    /// </summary>
    private void InitializeGraphModes()
    {
        GraphModes.Add(new GraphModeItem(GraphMode.Impact, "Impact (Change Over Day)"));
        GraphModes.Add(new GraphModeItem(GraphMode.Average, "Average (Daily Mood Level)"));
        GraphModes.Add(new GraphModeItem(GraphMode.RawData, "Raw Data (Individual Recordings)"));
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
    private int CalculateGraphWidth(DateRangeInfo dateRange)
    {
        // Base width for the smallest time increment (Last7Days)
        const int baseWidth = 800;
        const int incrementWidth = 128; // Additional width per time increment step
        
        return dateRange.DateRange switch
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
    private int CalculateExportGraphWidth(DateRangeInfo dateRange)
    {
        // Base width for export (higher resolution)
        const int baseWidth = 1200;
        const int incrementWidth = 192; // Additional width per time increment step for export (1.5x display increment)
        
        return dateRange.DateRange switch
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
    
    /// <summary>
    /// Toggles the visibility of the inline color picker
    /// </summary>
    private void ToggleColorPicker()
    {
        IsColorPickerVisible = !IsColorPickerVisible;
    }
    
    /// <summary>
    /// Selects a color from the inline color picker
    /// </summary>
    private void SelectColor(string? colorName)
    {
        if (string.IsNullOrEmpty(colorName)) return;
        
        try
        {
            // Show debugging message first
            ShowStatusMessage($"Attempting to select color: {colorName}");
            
            // Use a more direct approach with predefined colors
            Color selectedColor = colorName switch
            {
                "Blue" => Colors.Blue,
                "Red" => Colors.Red,
                "Green" => Colors.Green,
                "Orange" => Colors.Orange,
                "Purple" => Colors.Purple,
                "Brown" => Colors.Brown,
                "Pink" => Colors.Pink,
                "Cyan" => Colors.Cyan,
                "Magenta" => Colors.Magenta,
                "Yellow" => Colors.Yellow,
                "LimeGreen" => Colors.LimeGreen,
                "DarkBlue" => Colors.DarkBlue,
                _ => Colors.Blue // Default fallback
            };
            
            SelectedLineColor = selectedColor;
            IsColorPickerVisible = false; // Hide picker after selection
            ShowStatusMessage($"Color changed to: {colorName}");
        }
        catch (Exception ex)
        {
            ShowStatusMessage($"Error selecting color: {ex.Message}");
        }
    }
    
    #endregion
}

/// <summary>
/// Wrapper class for DateRange enum to provide display name
/// </summary>
public class DateRangeItem
{
    public DateRangeItem(DateRange dateRange, IDateShim dateShim)
    {
        DateRange = new DateRangeInfo(dateRange, dateShim);
        DisplayName = DateRange.DisplayName;
    }
    
    public DateRangeInfo DateRange { get; }
    public string DisplayName { get; }
}

/// <summary>
/// Wrapper class for GraphMode enum to provide display name
/// </summary>
public class GraphModeItem
{
    public GraphModeItem(GraphMode graphMode, string displayName)
    {
        GraphMode = graphMode;
        DisplayName = displayName;
    }
    
    public GraphMode GraphMode { get; }
    public string DisplayName { get; }
}
