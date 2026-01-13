using System.Reflection;
using Maui.Controls.Sample.AI;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Essentials.AI;

namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder.Configuration
			.AddJsonStream(GetUserSecretsStream() ?? throw new InvalidOperationException("User secrets file not found as embedded resource."));

		builder.UseMauiApp<App>();

#if IOS || ANDROID || MACCATALYST
		builder.UseMauiMaps();
#endif

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
		});

		// Register AI Chat Clients
#if IOS || MACCATALYST
#pragma warning disable CA1416 // Validate platform compatibility - this sample requires iOS/macCatalyst 26.0+

		// Register the base Apple Intelligence client
		builder.Services.AddSingleton<AppleIntelligenceChatClient>();

		// Register the Apple Intelligence client as IChatClient to allow direct use
		builder.Services.AddSingleton<IChatClient>(sp =>
		{
			var appleClient = sp.GetRequiredService<AppleIntelligenceChatClient>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return appleClient
				.AsBuilder()
				.UseLogging(loggerFactory)
				.Build();
		});

		// Register the Agent Framework wrapper as "local-model"
		builder.Services.AddKeyedSingleton<IChatClient>("local-model", (sp, _) =>
		{
			var appleClient = sp.GetRequiredService<AppleIntelligenceChatClient>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return appleClient
				.AsBuilder()
				.UseLogging(loggerFactory)
				// This prevents double tool invocation when using Microsoft Agent Framework
				// TODO: workaround for https://github.com/dotnet/extensions/issues/7204
				.Use(cc => new NonFunctionInvokingChatClient(cc, loggerFactory, sp))
				.Build();
		});

		// Register "cloud-model" with buffering
		builder.Services.AddKeyedSingleton<IChatClient>("cloud-model", (sp, _) =>
		{
			// TODO: Add OpenAI/Azure support for better translation quality
			var appleClient = sp.GetRequiredService<AppleIntelligenceChatClient>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return appleClient
				.AsBuilder()
				.UseLogging(loggerFactory)
				.Use(cc => new BufferedChatClient(cc))
				.Build();
		});

		builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, NLContextualEmbeddingGenerator>();

#pragma warning restore CA1416
#endif

		// Register AI agents and workflow
		builder.AddItineraryWorkflow();

		// Register Pages
		builder.Services.AddTransient<LandmarksPage>();
		builder.Services.AddTransient<TripPlanningPage>();

		// Register ViewModels
		builder.Services.AddTransient<LandmarksViewModel>();
		builder.Services.AddTransient<TripPlanningViewModel>();

		// Register Services
		builder.Services.AddSingleton<DataService>();
		builder.Services.AddSingleton<LanguagePreferenceService>();
		builder.Services.AddTransient<ItineraryService>();
		builder.Services.AddTransient<TaggingService>();
		builder.Services.AddHttpClient<WeatherService>();

		// Configure Logging
		builder.Services.AddLogging();
		builder.Logging.AddDebug();
		builder.Logging.AddConsole();
#if DEBUG
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
		builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

		return builder.Build();
	}

	private static Stream? GetUserSecretsStream()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream("Maui.Essentials.AI.Sample.secrets.json");
		return stream;
	}
}
