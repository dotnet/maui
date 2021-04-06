using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AndroidLifecycleExtensions
	{
		public static ILifecycleBuilder AddAndroid(this ILifecycleBuilder builder, Action<IAndroidLifecycleBuilder> configureDelegate)
		{
			var lifecycle = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(lifecycle);

			return builder;
		}

		class LifecycleBuilder : IAndroidLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public LifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void AddEvent(string eventName, Delegate action) =>
				_builder.AddEvent(eventName, action);
		}
	}
}
