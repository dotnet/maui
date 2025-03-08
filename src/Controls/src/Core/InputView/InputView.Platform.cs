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
			if (view is not InputView inputView || handler?.PlatformView is null)
				return;

			if (!inputView.IsVisible && inputView.IsSoftInputShowing())
			{
				inputView.HideSoftInputAsync(CancellationToken.None);
			}
		}
#endif
	}
}
