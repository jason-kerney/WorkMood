using System.Linq;
using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.Shims;
using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Pages;
using WhatsYourVersion;

namespace WorkMood.MauiApp;

public static class MauiProgram
{
	public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
	{
		// Parse command line arguments for logging configuration
		var enableLogging = ShouldEnableLogging();
		
		var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
		builder.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register core services for dependency injection
		builder.Services.AddSingleton<IDataArchiveService, DataArchiveService>();
		builder.Services.AddSingleton<MoodDataService>();
		builder.Services.AddSingleton<IMoodDataService>(provider => provider.GetRequiredService<MoodDataService>());
		builder.Services.AddSingleton<ScheduleConfigService>();
		builder.Services.AddSingleton<AutoSaveCommand>();
		builder.Services.AddSingleton<MorningReminderCommand>();
		builder.Services.AddSingleton<EveningReminderCommand>();
		
		// Register shim services
		builder.Services.AddSingleton<IFolderShim, FolderShim>();
		builder.Services.AddSingleton<IBrowserShim, BrowserShim>();
		
		// Register new infrastructure services
		builder.Services.AddSingleton<ILoggingService>(serviceProvider =>
		{
			// Create logging service with explicit configuration
			var loggingService = new LoggingService();
			// Configure logging based on command line arguments
			loggingService.IsEnabled = enableLogging;
			return loggingService;
		});
		builder.Services.AddSingleton<IWindowActivationService, WindowActivationService>();
		builder.Services.AddSingleton<IBrowserService, BrowserService>();
		builder.Services.AddTransient<IMoodEntryViewFactory, MoodEntryViewFactory>();
		
		// Register graph-related services
		builder.Services.AddSingleton<ILineGraphService, LineGraphService>();
		
		// Register version retriever service
		builder.Services.AddSingleton<IVersionRetriever>((serviceProvider) => 
			new VersionRetriever(AssemblyWrapper.From<App>())
		);
		
		// Register ViewModels
		builder.Services.AddTransient<AboutViewModel>();
		builder.Services.AddTransient<GraphViewModel>();
		
		// Register Pages
		builder.Services.AddTransient<About>();
		builder.Services.AddTransient<Graph>();
		
		// Register dispatcher service with all commands
		builder.Services.AddSingleton<MoodDispatcherService>(serviceProvider =>
		{
			var scheduleConfigService = serviceProvider.GetRequiredService<ScheduleConfigService>();
			var autoSaveCommand = serviceProvider.GetRequiredService<AutoSaveCommand>();
			var morningReminderCommand = serviceProvider.GetRequiredService<MorningReminderCommand>();
			var eveningReminderCommand = serviceProvider.GetRequiredService<EveningReminderCommand>();
			return new MoodDispatcherService(scheduleConfigService, autoSaveCommand, morningReminderCommand, eveningReminderCommand);
		});
		
		// Register view models
		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<MoodRecordingViewModel>();
		
		// Register pages
		builder.Services.AddTransient<Main>();
		builder.Services.AddTransient<MoodRecording>();

		return builder.Build();
	}

	/// <summary>
	/// Determines if logging should be enabled based on command line arguments
	/// </summary>
	/// <returns>True if logging should be enabled, false otherwise</returns>
	private static bool ShouldEnableLogging()
	{
		try
		{
			// Get command line arguments from environment
			var args = Environment.GetCommandLineArgs();
			
			// Check for --log or -log parameter to enable logging
			var hasLog = args.Any(arg => 
				string.Equals(arg, "--log", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(arg, "-l", StringComparison.OrdinalIgnoreCase));
			
			if (hasLog) return true;
			
			// Default to enabled for now (will be changed to disabled soon)
			return true;
		}
		catch
		{
			// If there's any error parsing arguments, default to enabled
			return true;
		}
	}
}
