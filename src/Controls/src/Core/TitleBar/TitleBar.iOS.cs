using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class TitleBar
	{
		internal override void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			base.OnIsVisibleChanged(oldValue, newValue);

#if MACCATALYST
			if (Handler?.MauiContext?.GetPlatformWindow() is UIWindow platWindow
				&& platWindow.RootViewController is WindowViewController windowViewController)
			{
				windowViewController.SetTitleBarVisibility(newValue);
				windowViewController.View?.SetNeedsLayout();
			}
#endif
		}
	}
}
