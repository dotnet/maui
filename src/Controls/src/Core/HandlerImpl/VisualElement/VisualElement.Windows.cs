#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		IDisposable? _loadedUnloadedToken;
		partial void HandlePlatformUnloadedLoaded()
		{
			_loadedUnloadedToken?.Dispose();
			_loadedUnloadedToken = null;

			// Window and this VisualElement both have a handler to work with
			if (Window?.Handler?.PlatformView != null &&
				Handler?.PlatformView is FrameworkElement view)
			{
				if (view.IsLoaded())
				{
					OnLoadedCore();
					_loadedUnloadedToken = view.OnUnloaded(OnUnloadedCore);
				}
				else
				{
					OnUnloadedCore();
					_loadedUnloadedToken = view.OnLoaded(OnLoadedCore);
				}

			}
			else
			{
				// My handler is still set but the window handler isn't set.
				// This means I'm starting to detach from the platform window
				// So we wait for the platform detatch events to fire before calling 
				// OnUnloaded
				if (Handler?.PlatformView is FrameworkElement detachingView &&
					detachingView.IsLoaded())
				{
					_loadedUnloadedToken = detachingView.OnUnloaded(OnUnloadedCore);
				}
				else
				{
					OnUnloadedCore();
				}
			}
		}
	}
}
