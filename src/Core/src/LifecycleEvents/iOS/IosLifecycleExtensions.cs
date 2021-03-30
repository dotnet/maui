using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class IosLifecycleExtensions
	{
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

			public void AddEvent(string eventName, Delegate action) =>
				_builder.AddEvent(eventName, action);
		}
	}
}