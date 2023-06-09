using System;
using UIKit;

namespace Microsoft.Maui.Controls.Platform;

public static class UIEdgeInsetsExtension
{
	public static Thickness ToThickness(this UIEdgeInsets insets) => new(insets.Left, insets.Top, insets.Right, insets.Bottom);
}
