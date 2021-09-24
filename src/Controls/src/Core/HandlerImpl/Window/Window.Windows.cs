using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UI.Xaml.Window NativeWindow =>
			(Handler?.NativeView as UI.Xaml.Window) ?? throw new InvalidOperationException("Window Handler should have a Window set.");
	}
}
