using System;
using System.Threading;

namespace Microsoft.Maui.Controls
{
	public partial class InputView
	{
#if ANDROID || IOS
		internal static void MapIsFocused(IViewHandler handler, IView view)
		{
			if (view is InputView iv)
			{
				handler
					?.GetService<HideSoftInputOnTappedChangedManager>()
					?.UpdateFocusForView(iv);
			}
		}

		internal static void MapIsVisible(IViewHandler handler, IView view)
		{
			if (view is not InputView inputView || handler?.PlatformView == null)
			{
				return;
			}

			// Prevent input queuing when InputView is hidden
			// Dismiss soft keyboard on Android/iOS to stop background input processing
			if (!inputView.IsVisible && inputView.IsSoftInputShowing())
			{
				inputView.HideSoftInputAsync(CancellationToken.None);
			}
		}
#endif
	}
}
