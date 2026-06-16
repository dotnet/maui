using System;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
#if !(ANDROID || IOS)
		internal void UpdateFocusForView(InputView iv)
		{

		}

		internal void UpdatePage(ContentPage contentPage)
		{
		}
#endif
	}
}
