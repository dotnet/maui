#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler
	{
		public static PropertyMapper<IGraphicsView, GraphicsViewHandler> GraphicsViewMapper = new PropertyMapper<IGraphicsView, GraphicsViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IGraphicsView.Drawable)] = MapDrawable
		};

		public GraphicsViewHandler() : base(GraphicsViewMapper)
		{

		}

		public GraphicsViewHandler(PropertyMapper? mapper = null) : base(mapper ?? GraphicsViewMapper)
		{

		}
	}
}