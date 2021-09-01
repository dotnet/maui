using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		readonly List<Window> _windows = new();

		IReadOnlyList<IWindow> IApplication.Windows => Windows;

		public IReadOnlyList<Window> Windows => _windows.AsReadOnly();

		IWindow IApplication.CreateWindow(IActivationState activationState)
		{
			var window = CreateWindow(activationState);

			if (_pendingMainPage != null && window.Page != null && window.Page != _pendingMainPage)
				throw new InvalidOperationException($"Both {nameof(MainPage)} was set and {nameof(Application.CreateWindow)} was overridden to provide a page.");

			if (!_windows.Contains(window))
				AddWindow(window);

			// clear out the pending main page as this will never be used again
			_pendingMainPage = null;

			return window;
		}

		public void ThemeChanged()
		{
			Current?.TriggerThemeChanged(new AppThemeChangedEventArgs(Current.RequestedTheme));
		}

		protected virtual Window CreateWindow(IActivationState activationState)
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