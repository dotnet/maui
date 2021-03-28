using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class IosLifecycleExtensions
	{
		public static IAppHostBuilder ConfigureIosLifecycleEvents(this IAppHostBuilder builder, Action<HostBuilderContext, IIosLifecycleBuilder> configureDelegate)
		{
			builder.ConfigureLifecycleEvents((ctx, lifecycleBuilder) =>
			{
				lifecycleBuilder.AddIos(iOSBuilder => configureDelegate?.Invoke(ctx, iOSBuilder));
			});

			return builder;
		}

		public static ILifecycleBuilder AddIos(this ILifecycleBuilder builder, Action<IIosLifecycleBuilder> configureDelegate)
		{
			var iOS = new IosLifecycleBuilder(builder);

			configureDelegate?.Invoke(iOS);

			return builder;
		}

		class IosLifecycleBuilder : IIosLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public IosLifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void Add(string eventName, Delegate action) =>
				_builder.Add(eventName, action);

			public ILifecycleEventService Build() =>
				_builder.Build();
		}
	}
}