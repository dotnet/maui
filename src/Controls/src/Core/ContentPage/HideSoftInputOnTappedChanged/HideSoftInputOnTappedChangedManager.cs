using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		List<ContentPage> _contentPages = new List<ContentPage>();

		bool FeatureEnabled
		{
			get
			{
				foreach (var page in _contentPages)
				{
					if (page.HideSoftInputOnTapped && page.HasNavigatedTo)
						return true;
				}
				return false;
			}
		}

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
