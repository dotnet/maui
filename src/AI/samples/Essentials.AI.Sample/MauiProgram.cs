using System.Reflection;
using Maui.Controls.Sample.AI;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ClientModel;

#if ENABLE_OPENAI_CLIENT
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
#endif

namespace Maui.Controls.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder.Configuration.AddUserSecrets();

		builder.UseMauiApp<App>();

#if IOS || ANDROID || MACCATALYST
		builder.UseMauiMaps();
#endif

		builder.ConfigureFonts(fonts =>
		{
			fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
		});

		// Register AI agents and workflow
		builder.AddOpenAIServices();
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

	private static Stream? AddUserSecrets(this ConfigurationManager manager)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream("Maui.Essentials.AI.Sample.secrets.json");
		if (stream is not null)
		{
			manager.AddJsonStream(stream);
		}
		return stream;
	}

	private static MauiAppBuilder AddOpenAIServices(this MauiAppBuilder builder)
	{
#if ENABLE_OPENAI_CLIENT
		// Create AI clients
		var aiSection = builder.Configuration.GetSection("AI");
		var apikey = aiSection["ApiKey"] ?? throw new InvalidOperationException("API Key not found in user secrets.");
		var endpoint = new Uri(aiSection["Endpoint"] ?? throw new InvalidOperationException("Endpoint not found in user secrets."));
		var chatModel = aiSection["DeploymentName"] ?? throw new InvalidOperationException("Deployment Name not found in user secrets.");
		var embeddingModel = aiSection["EmbeddingDeploymentName"] ?? throw new InvalidOperationException("Embedding Deployment Name not found in user secrets.");
		var client = new ChatClient(
			credential: new ApiKeyCredential(apikey),
			model: chatModel,
			options: new OpenAIClientOptions()
			{
				Endpoint = endpoint,
			});
		var embeddings = new EmbeddingClient(
			credential: new ApiKeyCredential(apikey),
			model: embeddingModel,
			options: new OpenAIClientOptions()
			{
				Endpoint = endpoint,
			});

		// Add generic chat client for simple inference
		builder.Services.AddSingleton<IChatClient>(provider =>
		{
			var lf = provider.GetRequiredService<ILoggerFactory>();
			var realClient = client.AsIChatClient()
				.AsBuilder()
				.UseLogging(lf)
				.Build();
			return realClient;
		});

		// Add chat client for local model with function calling
		// TODO: Replace with actual local model client when available
		builder.Services.AddKeyedSingleton<IChatClient>("local-model", (provider, _) =>
		{
			var lf = provider.GetRequiredService<ILoggerFactory>();
			var realClient = client.AsIChatClient()
				.AsBuilder()
				.UseLogging(lf)
				.UseFunctionInvocation()
				.Use(cc => new BufferedChatClient(cc))
				.Build();
			return realClient;
		});

		// Add chat client for cloud model without function calling
		builder.Services.AddKeyedSingleton<IChatClient>("cloud-model", (provider, _) =>
		{
			var lf = provider.GetRequiredService<ILoggerFactory>();
			var realClient = client.AsIChatClient()
				.AsBuilder()
				.UseLogging(lf)
				.Use(cc => new BufferedChatClient(cc))
				.Build();
			return realClient;
		});

		// Add embedding generation client
		builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
		{
			var lf = provider.GetRequiredService<ILoggerFactory>();
			var realClient = embeddings.AsIEmbeddingGenerator()
				.AsBuilder()
				.UseLogging(lf)
				.Build();
			return realClient;
		});
#endif

		return builder;
	}
}
