namespace Microsoft.Maui.Controls;

internal static class LayoutExtensions
{
	internal static void IgnoreLayoutSafeArea(this Layout layout)
	{
#pragma warning disable CS0618 // Type or member is obsolete
		layout.IgnoreSafeArea = true;
#pragma warning restore CS0618 // Type or member is obsolete

		foreach (var child in layout.Children)
		{
			if (child is Layout childLayout)
			{
				IgnoreLayoutSafeArea(childLayout);
			}
			else if (child is IContentView contentView && contentView.Content is Layout contentLayout)
			{
				IgnoreLayoutSafeArea(contentLayout);
			}
		}
	}
}
