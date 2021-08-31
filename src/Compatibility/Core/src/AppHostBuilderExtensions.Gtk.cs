#nullable enable

using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls.Compatibility;
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{

	public static partial class AppHostBuilderExtensions
	{

		internal static MauiAppBuilder ConfigureCompatibilityLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddGtk(OnConfigureLifeCycle));

		static void OnConfigureLifeCycle(IGtkLifecycleBuilder gtk)
		{
			gtk
			   .OnMauiContextCreated((mauiContext) =>
				{
					// This is the final Init that sets up the real context from the application.

					var state = new ActivationState(mauiContext);
					Forms.Init(state);
				});
		}

	}

}