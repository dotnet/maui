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
#if ANDROID
					// On Android, during navigation, fragments can be reused which means
					// the normal ViewAttachedToWindow events might not fire. This fixes
					// issue #29414 where navigating back to a page doesn't trigger the Loaded event.
					// If we have subscribers to the loaded event and the platform view is loaded,
					// we should force the loaded event to fire.
					if (_loaded is not null)
					{
						SendLoaded(false, true);
					}
					else
					{
						SendLoaded(false);
					}
#else
					SendLoaded(false);
#endif

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
					_loadedUnloadedToken = this.OnLoaded(SendLoaded);
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
