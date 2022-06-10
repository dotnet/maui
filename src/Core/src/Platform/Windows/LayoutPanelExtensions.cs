#nullable enable

namespace Microsoft.Maui.Platform
{
	public static class LayoutPanelExtensions
	{
		public static void UpdateClipsToBounds(this LayoutPanel layoutPanel, ILayout layout)
		{
			layoutPanel.ClipsToBounds = layout.ClipsToBounds;
			layoutPanel.InvalidateArrange();
		}
	}
}
