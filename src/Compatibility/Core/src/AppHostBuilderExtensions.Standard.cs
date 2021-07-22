using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System;

namespace Microsoft.Maui.Controls.Hosting
{ 
	public static partial class AppHostBuilderExtensions
	{
		internal static IAppHostBuilder ConfigureCompatibilityLifecycleEvents(this IAppHostBuilder builder) =>
			builder;
	}
}
