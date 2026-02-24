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
					SendLoaded(false);

					// If SendLoaded caused the unloaded tokens to wire up
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					_loadedUnloadedToken = this.OnUnloaded(SendUnloaded);
				}
				else
				{
					SendUnloaded(false);

					// If SendUnloaded caused the unloaded tokens to wire up
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					if (Handler is not null)
					{
						_loadedUnloadedToken = this.OnLoaded(SendLoaded);
					}
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
					_loadedUnloadedToken = this.OnUnloaded(SendUnloaded);
				}
				else
				{
					SendUnloaded();
				}
			}
		}
	}
}
