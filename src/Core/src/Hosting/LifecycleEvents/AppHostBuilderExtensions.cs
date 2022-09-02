using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public class LifecycleEventRegistration
	{
		private readonly Action<ILifecycleBuilder> _registerAction;

		public LifecycleEventRegistration(Action<ILifecycleBuilder> registerAction)
		{
			_registerAction = registerAction;
		}

		internal void AddRegistration(ILifecycleBuilder effects)
		{
			_registerAction(effects);
		}
	}

	public static partial class MauiAppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureLifecycleEvents(this MauiAppBuilder builder, Action<ILifecycleBuilder>? configureDelegate)
		{
			builder.Services.TryAddSingleton<ILifecycleEventService>(sp => new LifecycleEventService(sp.GetServices<LifecycleEventRegistration>()));
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<LifecycleEventRegistration>(new LifecycleEventRegistration(configureDelegate));
			}

			return builder;
		}
	}
}