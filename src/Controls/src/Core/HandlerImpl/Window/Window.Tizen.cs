using System;
using NWindow = Tizen.NUI.Window;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal NWindow NativeWindow =>
			(Handler?.PlatformView as NWindow) ?? throw new InvalidOperationException("Window should have a Tizen.NUI.Window set.");
	}
}