namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout)
		{
			if (layout.CascadeInputTransparent)
			{
				handler.PlatformView?.UpdateInputTransparent(handler, layout);
			}
			else
			{
				// TODO. need to fix
				// If CascadeInputTransparent is true, child should be got an event
				// But, in NUI, if Container Sensitive was false, children also can't get an event
				// So, We can't set Sensitive to false, and we should pass through an event into below view
				// To pass through an event, on LayoutHandler.tizen I will ovrride OnHitTest method, and returning false when InputTransparent was true
			}
			layout.UpdateDescendantInputTransparent();
		}
	}
}
