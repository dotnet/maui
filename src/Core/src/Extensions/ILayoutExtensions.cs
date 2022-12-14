namespace Microsoft.Maui.Extensions
{
	// TODO: Make this public on .NET 8
	internal static class ILayoutExtensions
	{
		internal static void InvalidateChildrenIsEnabled(this ILayout layout)
		{
			foreach (var children in layout.GetChildren())
				children.Handler?.UpdateValue(nameof(IView.IsEnabled));
		}
	}
}