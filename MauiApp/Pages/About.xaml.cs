using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

/// <summary>
/// About page following MVVM pattern and SOLID principles
/// This page is only responsible for view-specific logic and delegates business logic to the ViewModel
/// </summary>
public partial class About : ContentPage
{
    private readonly AboutViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the About page with dependency injection
    /// </summary>
    /// <param name="viewModel">The view model for this page</param>
    public About(AboutViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }


}