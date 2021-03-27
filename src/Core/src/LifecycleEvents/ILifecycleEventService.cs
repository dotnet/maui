using System;
using System.Collections.Generic;

namespace Microsoft.Maui.LifecycleEvents
{
	public interface ILifecycleEventService
	{
		IEnumerable<TDelegate> GetDelegates<TDelegate>(string eventName)
			where TDelegate : Delegate;

		bool HasDelegates(string eventName);
	}
}