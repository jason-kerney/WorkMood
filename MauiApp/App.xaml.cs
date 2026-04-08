using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace WorkMood.MauiApp;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;

	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var moodDataService = _serviceProvider.GetRequiredService<MoodDataService>();
		var scheduleConfigService = _serviceProvider.GetRequiredService<IScheduleConfigService>();
		var mainPage = _serviceProvider.GetRequiredService<Main>();

		// Load configuration and trigger cleanup in background - fire and forget.
		_ = Task.Run(async () =>
		{
			try
			{
				var config = await scheduleConfigService.LoadScheduleConfigAsync();
				moodDataService.InitializeDataFilePath(config.CustomMoodDataPath);
				await scheduleConfigService.UpdateScheduleConfigAsync(config.MorningTime, config.EveningTime, newOverride: null);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"App startup schedule cleanup failed: {ex.Message}");
			}
		});
		
		var navigationPage = new NavigationPage(mainPage);
		var window = new Window(navigationPage) 
		{ 
			Title = "WorkMood - Daily Mood Tracker" 
		};
		
		return window;
	}
}