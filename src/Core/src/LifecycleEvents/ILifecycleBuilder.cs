using System;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleBuilder
	{
		void Add(string eventName, Delegate action);
	}
}