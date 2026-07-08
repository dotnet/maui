namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	public static class RendererFactory
	{
		public static IVisualElementRenderer CreateRenderer(VisualElement element)
		{
			return Platform.CreateRenderer(element);
		}
	}
}
