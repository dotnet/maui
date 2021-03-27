using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycleExtensions
	{
		public static IAppHostBuilder ConfigureAndroidLifecycleEvents(this IAppHostBuilder builder, Action<HostBuilderContext, IAndroidLifecycleBuilder> configureDelegate)
		{
			builder.ConfigureLifecycleEvents((ctx, lifecycleBuilder) =>
			{
				lifecycleBuilder.AddAndroid(androidBuilder => configureDelegate?.Invoke(ctx, androidBuilder));
			});

			return builder;
		}

		public static ILifecycleBuilder AddAndroid(this ILifecycleBuilder builder, Action<IAndroidLifecycleBuilder> configureDelegate)
		{
			var android = new AndroidLifecycleBuilder(builder);

			configureDelegate?.Invoke(android);

			return builder;
		}

		class AndroidLifecycleBuilder : IAndroidLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public AndroidLifecycleBuilder(ILifecycleBuilder builder)
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
