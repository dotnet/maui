#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler
	{
		public static IPropertyMapper<IGraphicsView, GraphicsViewHandler> GraphicsViewMapper = new PropertyMapper<IGraphicsView, GraphicsViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IGraphicsView.Drawable)] = MapDrawable
		};

		public static CommandMapper<IGraphicsView, GraphicsViewHandler> GraphicsViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(IGraphicsView.Invalidate)] = MapInvalidate
		};

		public GraphicsViewHandler() : base(GraphicsViewMapper, GraphicsViewCommandMapper)
		{

		}

		public GraphicsViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? GraphicsViewMapper, commandMapper ?? GraphicsViewCommandMapper)
		{

		}
	}
}