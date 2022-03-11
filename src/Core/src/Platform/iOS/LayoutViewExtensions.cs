namespace Microsoft.Maui.Platform
{
	public static class LayoutViewExtensions
	{
		public static void UpdateClipsToBounds(this LayoutView layoutView, ILayout layout)
		{
			layoutView.ClipsToBounds = layout.ClipsToBounds;
		}
	}
}