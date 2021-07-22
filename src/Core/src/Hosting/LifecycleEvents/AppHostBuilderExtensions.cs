using System;
using Microsoft.Extensions.DependencyInjection;

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
			builder.Services.AddSingleton<ILifecycleEventService, LifecycleEventService>();
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<LifecycleEventRegistration>(new LifecycleEventRegistration(configureDelegate));
			}

			return builder;
		}
	}
}