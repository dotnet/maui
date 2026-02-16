using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.LifecycleEvents
{
	public class LifecycleEventService : ILifecycleEventService, ILifecycleBuilder
	{
		readonly Dictionary<string, List<Delegate>> _mapper = new(StringComparer.Ordinal);

		public LifecycleEventService(IEnumerable<LifecycleEventRegistration> registrations)
		{
			if (registrations != null)
			{
				foreach (var registrationAction in registrations)
				{
					registrationAction.AddRegistration(this);
				}
			}
		}

		public void AddEvent<TDelegate>(string eventName, TDelegate action)
			where TDelegate : Delegate
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

		internal void RemoveEvent<TDelegate>(string eventName, TDelegate action)
				where TDelegate : Delegate
		{
			if (_mapper.TryGetValue(eventName, out var delegates) && delegates != null)
			{
				if (delegates.Remove(action) && delegates.Count == 0)
					_mapper.Remove(eventName);
			}
		}
	}
}