using System;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class UIEdgeInsetsExtensions
{
	internal static Thickness ToThickness(this UIEdgeInsets insets) => new(insets.Left, insets.Top, insets.Right, insets.Bottom);
}
