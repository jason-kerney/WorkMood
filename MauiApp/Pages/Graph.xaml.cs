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
}