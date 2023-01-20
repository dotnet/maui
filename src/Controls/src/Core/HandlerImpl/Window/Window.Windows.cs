#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UI.Xaml.Window NativeWindow =>
			(Handler?.PlatformView as UI.Xaml.Window) ?? throw new InvalidOperationException("Window Handler should have a Window set.");
	}
}
