using System;
using Microsoft.Maui.Controls.PlatformConfiguration;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

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

public static class UIEdgeInsetsExtension
{
	public static Thickness ToThickness(this UIEdgeInsets insets) => new(insets.Left, insets.Top, insets.Right, insets.Bottom);
}


