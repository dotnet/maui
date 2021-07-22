using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using static Microsoft.Maui.Essentials.EssentialsExtensions;

namespace Maui.Controls.Sample.Controls
{
	static class BordelessEntryAppHostBuilderExtensions
	{
		public static MauiAppBuilder UseBordelessEntry(this MauiAppBuilder builder, Action<BordelessEntryServiceBuilder> configureDelegate = null)
		{
			builder.Services.AddSingleton<IMauiInitializeService, BorderlessEntryInitializer>();

			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<BorderlessEntryRegistration>(new BorderlessEntryRegistration(configureDelegate));
			}

			return builder;
		}
	}
}
