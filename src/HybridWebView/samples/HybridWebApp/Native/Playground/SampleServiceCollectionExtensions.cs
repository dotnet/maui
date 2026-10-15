using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.HybridWebApps.Generated;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

static class SampleServiceCollectionExtensions
{
	public static IServiceCollection AddPlayground(this IServiceCollection services, string startupId, DateTimeOffset startedAt)
	{
		services.AddSingleton(new SampleStartupIdentity(startupId, startedAt));
		services.AddSingleton(new SharedCounterTarget());
		return services.AddMauiControlsSampleHybridWebAppActions();
	}
}
