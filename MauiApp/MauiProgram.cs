using WorkMood.MauiApp.Services;
using WorkMood.MauiApp.ViewModels;
using WorkMood.MauiApp.Pages;
using WhatsYourVersion;

namespace WorkMood.MauiApp;

public static class MauiProgram
{
	public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
	{
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
		builder.Services.AddSingleton<ScheduleConfigService>();
		builder.Services.AddSingleton<AutoSaveCommand>();
		builder.Services.AddSingleton<MorningReminderCommand>();
		builder.Services.AddSingleton<EveningReminderCommand>();
		
		// Register new infrastructure services
		builder.Services.AddSingleton<ILoggingService, LoggingService>();
		builder.Services.AddSingleton<IWindowActivationService, WindowActivationService>();
		builder.Services.AddSingleton<IBrowserService, BrowserService>();
		
		// Register version retriever service
		builder.Services.AddSingleton<IVersionRetriever>((serviceProvider) => 
			new VersionRetriever(AssemblyWrapper.From<App>())
		);
		
		// Register ViewModels
		builder.Services.AddTransient<AboutViewModel>();
		
		// Register Pages
		builder.Services.AddTransient<About>();
		
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
}
