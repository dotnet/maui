#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.Application']/Docs" />
	public partial class Application : IApplication
	{
		const string MauiWindowIdKey = "__MAUI_WINDOW_ID__";

		readonly List<Window> _windows = new();
		readonly Dictionary<string, Window> _requestedWindows = new();
		ILogger<Application>? _logger;

		ILogger<Application>? Logger =>
			_logger ??= Handler?.MauiContext?.CreateLogger<Application>();

		IReadOnlyList<IWindow> IApplication.Windows => _windows;

		/// <include file="../../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='Windows']/Docs" />
		public IReadOnlyList<Window> Windows => _windows;

		IWindow IApplication.CreateWindow(IActivationState? activationState)
		{
			Window? window = null;

			// try get the window that is pending
			if (activationState?.State?.TryGetValue(MauiWindowIdKey, out var requestedWindowId) ?? false)
			{
				if (requestedWindowId != null && _requestedWindows.TryGetValue(requestedWindowId, out var w))
					window = w;
			}

			// create a new one if there is no pending windows
			if (window == null)
			{
				window = CreateWindow(activationState);

				if (_pendingMainPage != null && window.Page != null && window.Page != _pendingMainPage)
					throw new InvalidOperationException($"Both {nameof(MainPage)} was set and {nameof(Application.CreateWindow)} was overridden to provide a page.");

				// clear out the pending main page as this will never be used again
				_pendingMainPage = null;
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
			// Window was closed, stop tracking it
			if (window is null)
				return;

			if (window is NavigableElement ne)
				ne.NavigationProxy.Inner = null;

			if (window is Element windowElement)
			{
				var oldIndex = InternalChildren.IndexOf(windowElement);
				InternalChildren.Remove(windowElement);
				windowElement.Parent = null;
				OnChildRemoved(windowElement, oldIndex);
			}

			_windows.Remove(window);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='OpenWindow']/Docs" />
		public virtual void OpenWindow(Window window)
		{
			var id = Guid.NewGuid().ToString();

			_requestedWindows[id] = window;

			var state = new PersistedState
			{
				[MauiWindowIdKey] = id
			};

			Handler?.Invoke(nameof(IApplication.OpenWindow), new OpenWindowRequest(State: state));
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='CloseWindow']/Docs" />
		public virtual void CloseWindow(Window window)
		{
			Handler?.Invoke(nameof(IApplication.CloseWindow), window);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Application.xml" path="//Member[@MemberName='ThemeChanged']/Docs" />
		void IApplication.ThemeChanged()
		{
			if (UserAppTheme != AppTheme.Unspecified)
				return;

			TriggerThemeChangedActual();
		}

		protected virtual Window CreateWindow(IActivationState? activationState)
		{
			if (Windows.Count > 0)
				return Windows[0];

			if (_pendingMainPage != null)
				return new Window(_pendingMainPage);

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
		}
	}
}