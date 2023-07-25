using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#endif

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
			if (Window?.Handler?.PlatformView is not null &&
				Handler?.PlatformView is PlatformView view)
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
				// So we wait for the platform detach events to fire before calling 
				// OnUnloaded
				if (Handler?.PlatformView is PlatformView detachingView &&
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
