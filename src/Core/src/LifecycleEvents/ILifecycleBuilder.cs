using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleBuilder
	{
		void AddEvent<TDelegate>(string eventName, TDelegate action)
			where TDelegate : Delegate;
	}
}