using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder;

		internal static MauiAppBuilder ConfigureWindowEvents(this MauiAppBuilder builder) =>
			builder;
	}
}
