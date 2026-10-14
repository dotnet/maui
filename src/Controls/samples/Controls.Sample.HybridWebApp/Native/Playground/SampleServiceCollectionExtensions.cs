using Microsoft.Extensions.DependencyInjection;
using Maui.HybridApp.Generated;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

static class SampleServiceCollectionExtensions
{
	public static IServiceCollection AddPlayground(this IServiceCollection services, string startupId, DateTimeOffset startedAt)
	{
		services.AddSingleton(new SampleStartupIdentity(startupId, startedAt));
		return services.AddMauiControlsSampleHybridWebAppActions();
	}
}
