using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.LifecycleEvents
{
	public class LifecycleEventService : ILifecycleEventService
	{
		readonly Dictionary<string, List<Delegate>> _mapper = new Dictionary<string, List<Delegate>>();

		public void AddEvent(string eventName, Delegate action)
		{
			if (!_mapper.TryGetValue(eventName, out var delegates) && delegates == null)
				_mapper[eventName] = delegates = new List<Delegate>();

			delegates.Add(action);
		}

		public IEnumerable<TDelegate> GetEventDelegates<TDelegate>(string eventName)
			where TDelegate : Delegate
		{
			if (_mapper.TryGetValue(eventName, out var delegates) && delegates != null)
				return delegates.OfType<TDelegate>();

			return Enumerable.Empty<TDelegate>();
		}

		public bool ContainsEvent(string eventName) =>
			_mapper.TryGetValue(eventName, out var delegates) && delegates?.Count > 0;
	}
}