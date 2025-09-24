using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Pages;

namespace WorkMood.MauiApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Load schedule configuration on startup and trigger cleanup
		var scheduleConfigService = Handler?.MauiContext?.Services?.GetService<ScheduleConfigService>();
		if (scheduleConfigService != null)
		{
			// Load configuration and trigger cleanup in background - fire and forget
			_ = Task.Run(async () => 
			{
				try
				{
					var config = await scheduleConfigService.LoadScheduleConfigAsync();
					// Trigger cleanup by calling UpdateScheduleConfigAsync with current settings
					await scheduleConfigService.UpdateScheduleConfigAsync(config.MorningTime, config.EveningTime, newOverride: null);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"App startup schedule cleanup failed: {ex.Message}");
				}
			});
		}

		var mainPage = Handler?.MauiContext?.Services?.GetService<Main>();
		if (mainPage == null)
		{
			throw new InvalidOperationException("Main service not found. Ensure it's registered in MauiProgram.cs");
		}
		
		var navigationPage = new NavigationPage(mainPage);
		var window = new Window(navigationPage) 
		{ 
			Title = "WorkMood - Daily Mood Tracker" 
		};
		
		return window;
	}
}