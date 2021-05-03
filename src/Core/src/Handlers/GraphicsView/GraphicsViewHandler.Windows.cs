using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, UserControl>
	{
		protected override UserControl CreateNativeView() => new UserControl();

		[MissingMapper]
		public static void MapDrawable(IViewHandler handler, IGraphicsView graphicsView) { }
	}
}