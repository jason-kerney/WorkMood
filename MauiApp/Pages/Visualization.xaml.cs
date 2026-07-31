using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class Visualization : ContentPage
{
    private VisualizationViewModel? _viewModel;

    public Visualization(
        IMoodDataService moodDataService,
        INavigationService navigationService,
        ILineGraphService lineGraphService,
        IDateShim dateShim)
    {
        InitializeComponent();

        // Initialize ViewModel with dependencies
        _viewModel = new VisualizationViewModel(
            moodDataService ?? throw new ArgumentNullException(nameof(moodDataService)),
            navigationService ?? throw new ArgumentNullException(nameof(navigationService)),
            lineGraphService ?? throw new ArgumentNullException(nameof(lineGraphService)),
            dateShim ?? throw new ArgumentNullException(nameof(dateShim))
        );

        BindingContext = _viewModel;
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
}