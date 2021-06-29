using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class TizenLifecycleExtensions
	{
		public static ILifecycleBuilder AddTizen(this ILifecycleBuilder builder, Action<ITizenLifecycleBuilder> configureDelegate)
		{
			var lifecycle = new LifecycleBuilder(builder);

			configureDelegate?.Invoke(lifecycle);

			return builder;
		}

		class LifecycleBuilder : ITizenLifecycleBuilder
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