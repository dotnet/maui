using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.LifecycleEvents
{
	internal static class LifecycleBuilderExtensions
	{
		public static TLifecycleBuilder OnEvent<TDelegate, TLifecycleBuilder>(this TLifecycleBuilder builder, TDelegate del, [CallerMemberName] string? eventName = null)
			where TLifecycleBuilder : ILifecycleBuilder
			where TDelegate : Delegate
		{
			builder.Add(eventName ?? typeof(TDelegate).Name, del);

			return builder;
		}
	}
}