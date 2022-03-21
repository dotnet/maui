using System;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.Compatibility.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCompatibilityLifecycleEvents(this MauiAppBuilder builder) =>
			builder;
	}
}
