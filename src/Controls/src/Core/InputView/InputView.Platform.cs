using System;

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
#endif
	}
}
