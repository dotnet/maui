namespace Microsoft.Maui.Controls
{
	public partial class BoxView
	{
		public static void MapBackground(IShapeViewHandler handler, IShapeView boxView)
		{
			// If Background is not null, will use Color for the Shape background
			// and Background for the ShapeView background.
			if (boxView.Background is not null && boxView.Fill is not null)
			{
				handler.UpdateValue(nameof(IViewHandler.ContainerView));
				handler.ToPlatform().UpdateBackground(boxView);

				handler.PlatformView?.InvalidateShape(boxView);
			}
		}
	}
}