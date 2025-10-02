using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class Graph : ContentPage
{
    private readonly GraphViewModel _viewModel;

    public Graph(GraphViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataAsync();
    }
    
    /// <summary>
    /// Handles size changes of the graph container to update viewport-based scaling
    /// </summary>
    private void GraphContainer_SizeChanged(object? sender, EventArgs e)
    {
        if (sender is Border border)
        {
            _viewModel.UpdateContainerSize(border.Width, border.Height);
        }
    }
}