using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers.Memory
{
	public class MemoryTestFixture : IDisposable
	{
		readonly Dictionary<Type, (WeakReference handler, WeakReference view)> _handlers = new();

		public MemoryTestFixture()
		{
		}

		public void AddReferences(Type handlerType, (WeakReference handler, WeakReference view) value) =>
			_handlers.Add(handlerType, value);

		public bool HasType(Type handlerType) => _handlers.ContainsKey(handlerType);

		public bool DoReferencesStillExist(Type handlerType)
		{
			(WeakReference weakHandler, WeakReference weakView) = _handlers[handlerType];

			return weakHandler.IsAlive || weakView.IsAlive;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_handlers.Clear();
		}
	}
}
