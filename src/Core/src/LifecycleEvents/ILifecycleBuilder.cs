using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleBuilder
	{
		void AddEvent(string eventName, Delegate action);
	}
}