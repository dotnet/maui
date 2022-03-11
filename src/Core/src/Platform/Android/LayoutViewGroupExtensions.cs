namespace Microsoft.Maui.Platform
{
	public static class LayoutViewGroupExtensions
	{
		public static void UpdateClipsToBounds(this LayoutViewGroup layoutViewGroup, ILayout layout)
		{
			layoutViewGroup.ClipsToBounds = layout.ClipsToBounds;
			layoutViewGroup.RequestLayout();
		}
	}
}
