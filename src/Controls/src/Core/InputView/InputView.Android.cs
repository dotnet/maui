#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class InputView
	{
		internal static void MapFocus(IViewHandler handler, IView view, object args)
		{
			handler.ShowKeyboardIfFocused(view);
		}
	}
}
