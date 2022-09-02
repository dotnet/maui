namespace Microsoft.Maui.Platform
{
	public static class LayoutCanvasExtensions
	{
		public static void UpdateClipsToBounds(this LayoutViewGroup layoutViewGroup, ILayout layout)
		{
			if (layout.ClipsToBounds)
			{
				layoutViewGroup.ClippingMode = Tizen.NUI.ClippingModeType.ClipChildren;
			}
			else
			{
				layoutViewGroup.ClippingMode = Tizen.NUI.ClippingModeType.Disabled;
			}
		}
	}
}