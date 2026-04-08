using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class StorageSettingsPage : ContentPage
{
    public StorageSettingsPage(StorageSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
