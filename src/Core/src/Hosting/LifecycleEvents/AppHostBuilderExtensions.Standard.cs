using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static IAppHostBuilder ConfigureCrossPlatformLifecycleEvents(this IAppHostBuilder builder) =>
			builder;
	}
}
