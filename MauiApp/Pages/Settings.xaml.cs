using WorkMood.MauiApp.Models;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.ViewModels;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace WorkMood.MauiApp.Pages;

/// <summary>
/// Settings page for configuring mood tracking schedule - refactored to follow MVVM pattern
/// </summary>
public partial class Settings : ContentPage
{
    private readonly SettingsPageViewModel _viewModel;
    
    // Keep the old fields temporarily for compatibility during transition
    private readonly ScheduleConfigService _scheduleConfigService;

    public Settings(ScheduleConfigService scheduleConfigService)
    {
        _scheduleConfigService = scheduleConfigService;
        
        InitializeComponent();
        
        // Create dependencies following Dependency Inversion Principle
    var navigationService = new NavigationService(this);
        
        // Create and set the ViewModel
        _viewModel = new SettingsPageViewModel(scheduleConfigService, navigationService);
        BindingContext = _viewModel;
    }

    public Settings()
    {
        _scheduleConfigService = new ScheduleConfigService();
        
        InitializeComponent();
        
        // Fallback constructor for design-time or when DI is not available
    var navigationService = new NavigationService(this);
        
        _viewModel = new SettingsPageViewModel(_scheduleConfigService, navigationService);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load configuration when page appears
        await _viewModel.LoadConfigurationAsync();
        
        // Wire up TimePicker value changed events to update ViewModel properties
        MorningTimePicker.PropertyChanged += OnMorningTimePickerChanged;
        EveningTimePicker.PropertyChanged += OnEveningTimePickerChanged;
        
        // Sync initial TimePicker values with ViewModel
        MorningTimePicker.Time = _viewModel.MorningTime;
        EveningTimePicker.Time = _viewModel.EveningTime;
        NewOverrideDatePicker.Date = _viewModel.NewOverrideDate;
        NewOverrideMorningTimePicker.Time = _viewModel.NewOverrideMorningTime;
        NewOverrideEveningTimePicker.Time = _viewModel.NewOverrideEveningTime;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unwire events to prevent memory leaks
        MorningTimePicker.PropertyChanged -= OnMorningTimePickerChanged;
        EveningTimePicker.PropertyChanged -= OnEveningTimePickerChanged;
    }

    /// <summary>
    /// Handles morning time picker changes to sync with ViewModel
    /// This is view-specific logic for handling TimePicker binding limitations
    /// </summary>
    private void OnMorningTimePickerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            _viewModel.MorningTime = MorningTimePicker.Time;
        }
    }

    /// <summary>
    /// Handles evening time picker changes to sync with ViewModel
    /// This is view-specific logic for handling TimePicker binding limitations
    /// </summary>
    private void OnEveningTimePickerChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            _viewModel.EveningTime = EveningTimePicker.Time;
        }
    }
}