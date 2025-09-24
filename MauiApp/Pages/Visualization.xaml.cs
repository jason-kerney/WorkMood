using Microsoft.Maui.Graphics;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Processors;
using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Graphics;
using System.ComponentModel;

namespace WorkMood.MauiApp.Pages;

public partial class Visualization : ContentPage
{
    private VisualizationViewModel? _viewModel;

    public Visualization(
        IMoodDataService? moodDataService = null, 
        INavigationService? navigationService = null)
    {
        InitializeComponent();
        
        // Initialize ViewModel with dependencies
        _viewModel = new VisualizationViewModel(
            moodDataService ?? new MoodDataService(),
            navigationService ?? new NavigationService(this)
        );
        
        BindingContext = _viewModel;
        
        // Subscribe to property changes to update visualization
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }
    
    /// <summary>
    /// Gets the current ViewModel (for testing purposes)
    /// </summary>
    public VisualizationViewModel? ViewModel => _viewModel;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Notify ViewModel that the page is appearing
        if (_viewModel != null)
        {
            await _viewModel.OnAppearingAsync();
        }
    }

    /// <summary>
    /// Creates the line graph view for visualization
    /// This method remains in code-behind as it's view-specific logic
    /// </summary>
    public View CreateLineGraphView(MoodVisualizationData visualizationData)
    {
        var graphicsView = new GraphicsView
        {
            HeightRequest = 100,
            BackgroundColor = Colors.Transparent,
            Drawable = new EnhancedLineGraphDrawable(visualizationData)
        };
        
        return graphicsView;
    }
    
    /// <summary>
    /// Helper method for legacy code compatibility
    /// Creates a visualization grid programmatically if needed for custom scenarios
    /// </summary>
    public void CreateVisualization(MoodVisualizationData visualizationData)
    {
        if (visualizationData?.DailyValues == null) return;
        
        // Clear existing visualization
        VisualizationGrid.Children.Clear();
        
        // Add day labels (row 0)
        for (int day = 0; day < 14; day++)
        {
            var dailyValue = visualizationData.DailyValues[day];
            var dayLabel = new Label
            {
                Text = dailyValue.Date.ToString("dd"),
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(2, 0, 2, 2)
            };
            
            Grid.SetColumn(dayLabel, day);
            Grid.SetRow(dayLabel, 0);
            VisualizationGrid.Children.Add(dayLabel);
        }
        
        // Add visualization line graph (row 1) using enhanced drawable
        var lineGraphView = CreateLineGraphView(visualizationData);
        Grid.SetColumn(lineGraphView, 0);
        Grid.SetColumnSpan(lineGraphView, 14);
        Grid.SetRow(lineGraphView, 1);
        VisualizationGrid.Children.Add(lineGraphView);
        
        // Add week labels (row 2)
        var week1Label = new Label
        {
            Text = "Week 1",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetColumn(week1Label, 0);
        Grid.SetColumnSpan(week1Label, 7);
        Grid.SetRow(week1Label, 2);
        VisualizationGrid.Children.Add(week1Label);
        
        var week2Label = new Label
        {
            Text = "Week 2",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetColumn(week2Label, 7);
        Grid.SetColumnSpan(week2Label, 7);
        Grid.SetRow(week2Label, 2);
        VisualizationGrid.Children.Add(week2Label);
    }
    
    /// <summary>
    /// Creates daily data list programmatically
    /// This method remains for backward compatibility but ViewModel is preferred
    /// </summary>
    public void CreateDailyDataList(List<MoodDayInfo> dailyInfoList)
    {
        // This method is deprecated since we now use CollectionView with data binding
        // Keeping for backward compatibility only
    }
    
    /// <summary>
    /// Helper method to convert hex string to Color
    /// </summary>
    private static Color GetColorFromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7) return Colors.LightGray;
        
        try
        {
            return Color.FromArgb(hex);
        }
        catch
        {
            return Colors.LightGray;
        }
    }

    #region Legacy Event Handlers - Deprecated, use ViewModel commands instead
    
    [Obsolete("Use ViewModel.RefreshCommand instead")]
    private void OnRefreshClicked(object sender, EventArgs e)
    {
        if (_viewModel?.RefreshCommand.CanExecute(null) == true)
        {
            _viewModel.RefreshCommand.Execute(null);
        }
    }

    [Obsolete("Use ViewModel.BackToHistoryCommand instead")]
    private void OnBackToHistoryClicked(object sender, EventArgs e)
    {
        if (_viewModel?.BackToHistoryCommand.CanExecute(null) == true)
        {
            _viewModel.BackToHistoryCommand.Execute(null);
        }
    }

    /// <summary>
    /// Handles property changes from the ViewModel to update the visualization
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisualizationViewModel.CurrentVisualization))
        {
            // Update the visualization when data changes
            if (_viewModel?.CurrentVisualization != null)
            {
                Dispatcher.Dispatch(() => CreateVisualization(_viewModel.CurrentVisualization));
            }
        }
    }

    /// <summary>
    /// Clean up event subscriptions
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
    
    #endregion
}