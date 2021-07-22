using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System;

namespace Microsoft.Maui.LifecycleEvents
{ 
	public static partial class AppHostBuilderExtensions
	{
		internal static IAppHostBuilder ConfigureCrossPlatformLifecycleEvents(this IAppHostBuilder builder) =>
			builder;
	}
}
