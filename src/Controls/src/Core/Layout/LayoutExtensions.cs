namespace Microsoft.Maui.Controls;

internal static class LayoutExtensions
{
	internal static void IgnoreLayoutSafeArea(this Layout layout)
	{
		layout.IgnoreSafeArea = true;

		foreach (var child in layout.Children)
		{
			if (child is Layout childLayout)
			{
				IgnoreLayoutSafeArea(childLayout);
			}
		}
	}
}
