using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static class HandlerMauiAppBuilderExtensions
	{
		public static MauiAppBuilder ConfigureMauiHandlers(this MauiAppBuilder builder, Action<IMauiHandlersCollection>? configureDelegate)
		{
			builder.Services.TryAddSingleton<IMauiHandlersServiceProvider, MauiHandlersServiceProvider>();
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<HandlerRegistration>(new HandlerRegistration(configureDelegate));
			}
			return builder;
		}

		internal class HandlerRegistration
		{
			private readonly Action<IMauiHandlersCollection> _registerAction;

			public HandlerRegistration(Action<IMauiHandlersCollection> registerAction)
			{
				_registerAction = registerAction;
			}

			internal void AddRegistration(IMauiHandlersCollection builder)
			{
				_registerAction(builder);
			}
		}
	}
}
