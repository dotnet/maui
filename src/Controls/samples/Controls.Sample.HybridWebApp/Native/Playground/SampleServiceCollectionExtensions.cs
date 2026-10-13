using Maui.HybridApp;
using Microsoft.Extensions.DependencyInjection;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

static class SampleServiceCollectionExtensions
{
	public static IServiceCollection AddPlayground(this IServiceCollection services, string startupId, DateTimeOffset startedAt)
	{
		services.AddScoped<SampleCounter>();
		services.AddSingleton(new SampleStartupIdentity(startupId, startedAt));
		return services
			.AddHybridProvider<SampleCounterProvider>()
			.AddHybridProvider<SampleEchoProvider>()
			.AddHybridProvider<SampleFailureProvider>()
			.AddHybridProvider<SampleDisabledProvider>()
			.AddHybridProvider<SampleWorkProvider>()
			.AddHybridProvider<SampleDiagnosticsProvider>();
	}
}
