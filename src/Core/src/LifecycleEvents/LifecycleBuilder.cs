using System;
using System.Collections.Generic;

namespace Microsoft.Maui.LifecycleEvents
{
	public class LifecycleBuilder : ILifecycleBuilder
	{
		readonly List<(string EventName, Delegate EventAction)> _events = new List<(string, Delegate)>();

		public void Add(string eventName, Delegate action)
		{
			_events.Add((eventName, action));
		}

		public ILifecycleEventService Build()
		{
			var services = new LifecycleEventService();

			foreach (var (name, action) in _events)
				services.Add(name, action);

			return services;
		}
	}
}