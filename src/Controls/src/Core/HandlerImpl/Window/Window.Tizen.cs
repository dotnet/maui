#nullable enable
using System;
using EWindow = ElmSharp.Window;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal EWindow NativeWindow =>
			(Handler?.PlatformView as EWindow) ?? throw new InvalidOperationException("Window should have a ElmSharp.Window set.");
	}
}