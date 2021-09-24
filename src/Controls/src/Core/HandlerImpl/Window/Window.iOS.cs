#nullable enable
using System;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UIWindow NativeWindow =>
			(Handler?.NativeView as UIWindow) ?? throw new InvalidOperationException("Window should have a UIWindow set.");
	}
}