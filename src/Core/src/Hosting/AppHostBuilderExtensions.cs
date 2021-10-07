using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureCoreServices(this MauiAppBuilder builder)
		{
#if WINDOWS
			builder.Services.TryAddTransient<IDispatcherProvider>(svcs => new DispatcherProvider());
#else
			builder.Services.TryAddTransient<IDispatcherProvider>(svcs => new SingletonDispatcherProvider());
#endif

			return builder;
		}
	}
}