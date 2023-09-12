#nullable disable

namespace Microsoft.Maui.Controls.Platform
{
	internal static class InputViewExtensions
	{
		internal static void ShowKeyboardIfFocused(this IViewHandler handler, IView view)
		{
			if (view is VisualElement ve && ve.IsFocused && handler is IPlatformViewHandler platformViewHandler)
			{
				platformViewHandler.PlatformView.ShowSoftInput();
			}
		}
	}
}