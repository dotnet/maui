using System;
using System.Collections.Generic;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleEventService
	{
		IEnumerable<TDelegate> GetEventDelegates<TDelegate>(string eventName)
			where TDelegate : Delegate;

		bool ContainsEvent(string eventName);
	}
}