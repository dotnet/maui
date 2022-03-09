#nullable enable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UIWindow NativeWindow =>
			(Handler?.PlatformView as UIWindow) ?? throw new InvalidOperationException("Window should have a UIWindow set.");

		internal float PlatformDisplayDensity
			=> (float)((Handler?.PlatformView as UIWindow)?.Screen?.Scale ?? new nfloat(1.0f)).Value;
	}
}