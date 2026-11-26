using System;
using System.Collections.Generic;
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
			ConfigureMauiHandlers(builder.Services, configureDelegate);
			return builder;
		}

		public static IServiceCollection ConfigureMauiHandlers(this IServiceCollection services, Action<IMauiHandlersCollection>? configureDelegate)
		{
			if (configureDelegate != null)
			{
				services.AddSingleton<HandlerRegistration>(new HandlerRegistration(configureDelegate));
			}

			services.TryAddSingleton<IMauiHandlersFactory>(sp => new MauiHandlersFactory(sp.GetRequiredService<IMauiHandlersCollection>()));
			services.TryAddSingleton<IMauiHandlersCollection>(sp => new HandlerServiceBuilder(
				sp.GetServices<HandlerRegistration>(),
				sp.GetService<ElementHandlerDefaultsRegistration>() is not null));

			return services;
		}

		internal static MauiAppBuilder EnableElementHandlerDefaults(this MauiAppBuilder builder)
		{
			builder.Services.TryAddSingleton<ElementHandlerDefaultsRegistration>();
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

		internal class HandlerServiceBuilder : MauiServiceCollection, IMauiHandlersCollection, IElementHandlerDefaultsCollection
		{
			public HandlerServiceBuilder(IEnumerable<HandlerRegistration> registrationActions, bool elementHandlerDefaultsEnabled)
			{
				ElementHandlerDefaultsEnabled = elementHandlerDefaultsEnabled;

				if (registrationActions != null)
				{
					foreach (var effectRegistration in registrationActions)
					{
						effectRegistration.AddRegistration(this);
					}
				}
				HotReload.MauiHotReloadHelper.RegisterHandlers(this);
			}

			public bool ElementHandlerDefaultsEnabled { get; }
		}
	}

	internal sealed class ElementHandlerDefaultsRegistration
	{
	}

	internal interface IElementHandlerDefaultsCollection
	{
		bool ElementHandlerDefaultsEnabled { get; }
	}
}
