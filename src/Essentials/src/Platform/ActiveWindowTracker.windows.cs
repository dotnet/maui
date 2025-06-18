#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel
{
	class ActiveWindowTracker
	{
		readonly IWindowStateManager _windowStateManager;

		WindowMessageManager? _currentWindowManager;

		public ActiveWindowTracker(IWindowStateManager windowStateManager)
		{
			_windowStateManager = windowStateManager;
		}

		public event EventHandler<WindowMessageEventArgs>? WindowMessage;

		public void Start()
		{
			var window = _windowStateManager.GetActiveWindow();
			OnActiveWindowChanged(window);

			_windowStateManager.ActiveWindowChanged += OnActiveWindowChanged;
		}

		public void Stop()
		{
			OnActiveWindowChanged(null);

			_windowStateManager.ActiveWindowChanged -= OnActiveWindowChanged;
		}

		void OnActiveWindowChanged(object? sender, EventArgs e)
		{
			var window = _windowStateManager?.GetActiveWindow();
			OnActiveWindowChanged(window);
		}

		void OnActiveWindowChanged(UI.Xaml.Window? window)
		{
			if (_currentWindowManager is not null)
			{
				_currentWindowManager.WindowMessage -= OnWindowMessage;
				_currentWindowManager = null;
			}

			if (window is not null)
			{
				_currentWindowManager = WindowMessageManager.Get(window);
				_currentWindowManager.WindowMessage += OnWindowMessage;
			}
		}

		void OnWindowMessage(object? sender, WindowMessageEventArgs e) =>
			WindowMessage?.Invoke(sender, e);
	}
}
