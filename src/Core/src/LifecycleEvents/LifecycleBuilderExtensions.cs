using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class LifecycleBuilderExtensions
	{
		public static ILifecycleBuilder Add(this ILifecycleBuilder builder, string eventName, Action action) =>
			builder.OnEvent(action, eventName);

		internal static TLifecycleBuilder OnEvent<TDelegate, TLifecycleBuilder>(this TLifecycleBuilder builder, TDelegate del, [CallerMemberName] string? eventName = null)
			where TLifecycleBuilder : ILifecycleBuilder
			where TDelegate : Delegate
		{
			builder.Add(eventName ?? typeof(TDelegate).Name, del);

			return builder;
		}
	}
}