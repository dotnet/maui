using System;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class UIEdgeInsetsExtensions
{
	public static Thickness ToThickness(this UIEdgeInsets insets) => new(insets.Left, insets.Top, insets.Right, insets.Bottom);
}
