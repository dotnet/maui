using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;

#if ENABLE_OPENAI_CLIENT
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
#endif

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var appBuilder = MauiApp.CreateBuilder();

		appBuilder.TryAddOpenAI();

		appBuilder
			.ConfigureTests(new TestOptions
			{
				Assemblies =
				{
					typeof(MauiProgram).Assembly
				},
			})
			.UseHeadlessRunner(new HeadlessRunnerOptions
			{
				RequiresUIContext = true,
			})
			.UseVisualRunner();

		return appBuilder.Build();
	}

	private static void TryAddOpenAI(this MauiAppBuilder builder)
	{
#if ENABLE_OPENAI_CLIENT

		var assembly = Assembly.GetExecutingAssembly();
		var stream = assembly.GetManifestResourceStream("Microsoft.Maui.Essentials.AI.DeviceTests.secrets.json");

		builder.Configuration
			.AddJsonStream(stream ?? throw new InvalidOperationException("User secrets file not found as embedded resource."));
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
		builder.Services.AddSingleton(client);

		var embeddings = new EmbeddingClient(
			credential: new ApiKeyCredential(apikey),
			model: embeddingModel,
			options: new OpenAIClientOptions()
			{
				Endpoint = endpoint,
			});
		builder.Services.AddSingleton(embeddings);

#endif
	}
}