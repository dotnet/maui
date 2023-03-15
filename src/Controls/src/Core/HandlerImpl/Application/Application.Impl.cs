using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		internal const string MauiWindowIdKey = "__MAUI_WINDOW_ID__";

		readonly List<Window> _windows = new();
		readonly Dictionary<string, WeakReference<Window>> _requestedWindows = new();
		ILogger<Application>? _logger;

		ILogger<Application>? Logger =>
			_logger ??= Handler?.MauiContext?.CreateLogger<Application>();

		IReadOnlyList<IWindow> IApplication.Windows => _windows;

		public IReadOnlyList<Window> Windows => _windows;

		IWindow IApplication.CreateWindow(IActivationState? activationState)
		{
			Window? window = null;

			// try get the window that is pending
			if (activationState?.State?.TryGetValue(MauiWindowIdKey, out var requestedWindowId) ?? false)
			{
				if (requestedWindowId != null && _requestedWindows.TryGetValue(requestedWindowId, out var r))
				{
					if (r.TryGetTarget(out var w))
					{
						window = w;
					}
					_requestedWindows.Remove(requestedWindowId);
				}
			}

			// create a new one if there is no pending windows
			if (window == null)
			{
				window = CreateWindow(activationState);

				if (_singleWindowMainPage != null && window.Page != null && window.Page != _singleWindowMainPage)
					throw new InvalidOperationException($"Both {nameof(MainPage)} was set and {nameof(Application.CreateWindow)} was overridden to provide a page.");
			}

			// make sure it is added to the windows list
			if (!_windows.Contains(window))
				AddWindow(window);

			return window;
		}

		void IApplication.OpenWindow(IWindow window)
		{
			if (window is Window cwindow)
				OpenWindow(cwindow);
		}

		void IApplication.CloseWindow(IWindow window)
		{
			Handler?.Invoke(nameof(IApplication.CloseWindow), window);
		}

		internal void RemoveWindow(Window window)
		{
			// Do not attempt to close the "MainPage" window
			if (_singleWindowMainPage != null && window.Page == _singleWindowMainPage)
				return;

			// Window was closed, stop tracking it
			if (window is null)
				return;

			if (window is NavigableElement ne)
				ne.NavigationProxy.Inner = null;

			if (window is Element windowElement)
			{
				var oldIndex = InternalChildren.IndexOf(windowElement);
				InternalChildren.RemoveAt(oldIndex);
				windowElement.Parent = null;
				OnChildRemoved(windowElement, oldIndex);
			}

			_windows.Remove(window);
		}

		public virtual void OpenWindow(Window window)
		{
			var id = Guid.NewGuid().ToString("n");
			_requestedWindows.Add(id, new WeakReference<Window>(window));

			var state = new PersistedState
			{
				[MauiWindowIdKey] = id
			};

			Handler?.Invoke(nameof(IApplication.OpenWindow), new OpenWindowRequest(State: state));
		}

		public virtual void CloseWindow(Window window)
		{
			Handler?.Invoke(nameof(IApplication.CloseWindow), window);
		}

		void IApplication.ThemeChanged()
		{
			PlatformAppTheme = AppInfo.RequestedTheme;
		}

		protected virtual Window CreateWindow(IActivationState? activationState)
		{
			if (Windows.Count > 1)
				throw new NotImplementedException($"Either set {nameof(MainPage)} or override {nameof(Application.CreateWindow)}.");

			if (Windows.Count > 0)
				return Windows[0];

			if (_singleWindowMainPage != null)
				return new Window(_singleWindowMainPage);

			throw new NotImplementedException($"Either set {nameof(MainPage)} or override {nameof(Application.CreateWindow)}.");
		}

		void AddWindow(Window window)
		{
			_windows.Add(window);

			if (window is Element windowElement)
			{
				windowElement.Parent = this;
				InternalChildren.Add(windowElement);
				OnChildAdded(windowElement);
			}

			if (window is NavigableElement ne)
				ne.NavigationProxy.Inner = NavigationProxy;

			// Once the window has been attached to the application. 
			// The window will finish propagating events like `Appearing`.
			//
			// I could fire this from 'OnParentSet` inside Window but
			// I'd rather wait until Application is done wiring itself
			// up to the window before triggering any down stream life cycle
			// events.
			window.FinishedAddingWindowToApplication(this);
		}
	}
}
