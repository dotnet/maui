namespace Microsoft.Maui.Controls;

public static class LayoutExtensions
{
	public static void IgnoreLayoutSafeArea(this Layout layout)
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
