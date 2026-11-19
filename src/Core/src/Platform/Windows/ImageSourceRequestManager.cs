#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform
{
	// Used on the UI thread. Each consumer keeps its own manager so independent image properties do not cancel one another.
	internal sealed class ImageSourceRequestManager
	{
		readonly ConditionalWeakTable<DependencyObject, RequestState> _requests = new();

		internal long Begin(DependencyObject target)
		{
			var state = _requests.GetOrCreateValue(target);
			var version = Interlocked.Increment(ref state.Version);
			var cleanup = state.Cleanup;
			state.Cleanup = null;
			cleanup?.Invoke();
			return version;
		}

		internal bool IsCurrent(DependencyObject target, long version) =>
			_requests.TryGetValue(target, out var state) && Volatile.Read(ref state.Version) == version;

		internal void SetCleanup(DependencyObject target, long version, Action? cleanup)
		{
			if (_requests.TryGetValue(target, out var state) && Volatile.Read(ref state.Version) == version)
				state.Cleanup = cleanup;
		}

		sealed class RequestState
		{
			internal long Version;
			internal Action? Cleanup;
		}
	}
}
