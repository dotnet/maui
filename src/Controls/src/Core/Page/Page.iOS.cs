using System;
using Microsoft.Maui.Controls.PlatformConfiguration;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls;

public partial class Page
{
	static void MapUpdateSafeAreaInsets(IViewHandler handler, IView view)
	{
		if (view is Page page && handler.PlatformView is UIView uiView)
		{
			var insets = uiView.SafeAreaInsets.ToThickness();
			page.On<iOS>().SetSafeAreaInsets(insets);
		}
	}
}
