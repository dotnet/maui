using System;

namespace Microsoft.Maui;

internal sealed class WebViewRequestLifetime
{
	readonly object _gate = new();
	readonly Action<Action> _dispatch;
	readonly Action _completed;
	bool _active = true;

	internal WebViewRequestLifetime(Action<Action> dispatch, Action completed)
	{
		_dispatch = dispatch;
		_completed = completed;
	}

	internal void Invoke(Action callback, bool complete = false)
	{
		// Dispatch before acquiring the gate so a stopped task cannot deadlock the UI thread.
		_dispatch(() =>
		{
			lock (_gate)
			{
				if (!_active)
					return;

				if (complete)
					_active = false;

				try
				{
					callback();
				}
				finally
				{
					if (complete)
						_completed();
				}
			}
		});
	}

	internal void Stop()
	{
		lock (_gate)
		{
			if (!_active)
				return;

			_active = false;
			_completed();
		}
	}
}
