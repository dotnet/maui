using System.Reflection;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Essentials.AI;
#if ENABLE_OPENAI_CLIENT
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
#endif

namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static bool UseCloudAI = true;

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

		// Register Language Preference Service
		builder.Services.AddSingleton<LanguagePreferenceService>();

		// Register AI Chat Clients
		#if ENABLE_OPENAI_CLIENT
		if (UseCloudAI)
		{
			var aiSection = builder.Configuration.GetSection("AI");
			var chatClient = new ChatClient(
				credential: new ApiKeyCredential(aiSection["ApiKey"] ?? throw new InvalidOperationException("API Key not found in user secrets.")),
				model: aiSection["DeploymentName"] ?? throw new InvalidOperationException("Deployment Name not found in user secrets."),
				options: new OpenAIClientOptions()
				{
					Endpoint = new(aiSection["Endpoint"] ?? throw new InvalidOperationException("Endpoint not found in user secrets.")),
				});

			builder.Services.AddKeyedSingleton<IChatClient>("cloud-model", (sp, _) =>
			{
				var lf = sp.GetRequiredService<ILoggerFactory>();
				return chatClient.AsIChatClient()
					.AsBuilder()
					.UseLogging(lf)
					.UseFunctionInvocation()
					.Build();
			});
		}
		#endif

#if IOS || MACCATALYST
#pragma warning disable CA1416 // Validate platform compatibility - this sample requires iOS/macCatalyst 26.0+
		// Register the base Apple Intelligence client
		builder.Services.AddSingleton<AppleIntelligenceChatClient>();
		
		// Register the Apple Intelligence client as IChatClient to allow direct use
		builder.Services.AddSingleton<IChatClient>(sp => sp.GetRequiredService<AppleIntelligenceChatClient>());

		// Register the Agent Framework wrapper as the default IChatClient
		// This prevents double tool invocation when using Microsoft Agent Framework
		builder.Services.AddKeyedSingleton<IChatClient>("local-model", (sp, _) =>
		{
			var appleClient = sp.GetRequiredService<AppleIntelligenceChatClient>();
			var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
			return new NonFunctionInvokingChatClient(appleClient, loggerFactory, sp);
		});
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
		builder.Services.AddSingleton<LandmarkDataService>(sp => LandmarkDataService.Instance);
		builder.Services.AddTransient<ItineraryService>();
		builder.Services.AddTransient<TaggingService>();
		builder.Services.AddHttpClient<WeatherService>();

		// Configure Logging
		builder.Services.AddLogging();
		builder.Logging.AddDebug();
		builder.Logging.AddConsole();
#if DEBUG
		builder.Logging.SetMinimumLevel(LogLevel.Trace);
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
