using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.ViewModels;

namespace WorkMood.MauiApp.Pages;

public partial class StorageSettingsPage : ContentPage
{
    public StorageSettingsPage(
        IMoodDataService moodDataService,
        IFolderPickerShim folderPickerShim,
        IPathValidationShim pathValidationShim)
    {
        InitializeComponent();

        var navigationService = new NavigationService(this);
        BindingContext = new StorageSettingsViewModel(
            moodDataService,
            navigationService,
            folderPickerShim,
            pathValidationShim);
    }
}
