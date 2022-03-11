using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class iOSLifecycleExtensions
	{
		public static ILifecycleBuilder AddiOS(this ILifecycleBuilder builder, Action<IiOSLifecycleBuilder> configureDelegate)
		{
			var lifecycle = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(lifecycle);

			return builder;
		}

		class LifecycleBuilder : IiOSLifecycleBuilder
		{
			readonly ILifecycleBuilder _builder;

			public LifecycleBuilder(ILifecycleBuilder builder)
			{
				_builder = builder;
			}

			public void AddEvent<TDelegate>(string eventName, TDelegate action)
				where TDelegate : Delegate
			{
				_builder.AddEvent(eventName, action);
			}
		}
	}
}