#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		readonly List<Window> _windows = new();

		IReadOnlyList<IWindow> IApplication.Windows => Windows;

		public IReadOnlyList<Window> Windows => _windows.AsReadOnly();

		readonly Dictionary<string, Window> _requestedWindows = new();

		IWindow IApplication.CreateWindow(IActivationState activationState)
		{
			var requestedWindowId = activationState.State?["__MAUI_WINDOW_ID__"];

			Window window;

			if (requestedWindowId != null && _requestedWindows.ContainsKey(requestedWindowId))
				window = _requestedWindows[requestedWindowId];
			else
				window = CreateWindow(activationState);

			if (_pendingMainPage != null && window.Page != null && window.Page != _pendingMainPage)
				throw new InvalidOperationException($"Both {nameof(MainPage)} was set and {nameof(Application.CreateWindow)} was overridden to provide a page.");

			if (!_windows.Contains(window))
				AddWindow(window);

			// clear out the pending main page as this will never be used again
			_pendingMainPage = null;

			return window;
		}

		void IApplication.OpenWindow(IWindow window)
		{
			if (window is Window cwindow)
				OpenWindow(cwindow);
		}

		protected virtual void OpenWindow(Window window)
		{
			var id = Guid.NewGuid().ToString();

			_requestedWindows[id] = window;

#if IOS || MACCATALYST
			var userInfo = new Foundation.NSMutableDictionary();
			userInfo.SetValueForKey(new Foundation.NSString(id), new Foundation.NSString("__MAUI_WINDOW_ID__"));

			var userActivity = new Foundation.NSUserActivity("__MAUI_DEFAULT_SCENE_CONFIGURATION__");
			userActivity.AddUserInfoEntries(userInfo);

			UIKit.UIApplication.SharedApplication.RequestSceneSessionActivation(
				null,
				userActivity,
				null,
				err =>
				{
					Console.WriteLine(err.Description);
				});
#endif
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