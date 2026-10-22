using System;
using System.Threading;
using Microsoft.Maui.Diagnostics;

namespace Microsoft.Maui.Controls.Diagnostics
{
	internal sealed class NativeElementSubscriptionWatcher<T> : IDisposable
		where T : class
	{
		readonly Action<T> _callback;
		readonly WeakReference<T> _target;
		int _disposed;

		NativeElementSubscriptionWatcher(T target, Action<T> callback)
		{
			_target = new WeakReference<T>(target);
			_callback = callback;
		}

		public static NativeElementSubscriptionWatcher<T> Attach(T target, Action<T> callback)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (callback is null)
				throw new ArgumentNullException(nameof(callback));

			var watcher = new NativeElementSubscriptionWatcher<T>(target, callback);
			NativeElementDiagnostics.SubscriptionAdded += watcher.OnSubscriptionAdded;
			return watcher;
		}

		void OnSubscriptionAdded()
		{
			if (Volatile.Read(ref _disposed) != 0)
				return;

			if (_target.TryGetTarget(out var target))
				_callback(target);
			else
				Dispose();
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
				return;

			NativeElementDiagnostics.SubscriptionAdded -= OnSubscriptionAdded;
		}
	}
}
