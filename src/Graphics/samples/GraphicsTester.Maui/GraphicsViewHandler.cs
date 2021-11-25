using System;
using Microsoft.Maui.Handlers;

namespace GraphicsTester.Maui
{
	public partial class GraphicsViewHandler
	{
		public static IPropertyMapper<IGraphicsView, GraphicsViewHandler> GraphicsViewMapper = new (ViewHandler.ViewMapper)
		{
		};

		public static CommandMapper<IGraphicsView, GraphicsViewHandler> GraphicsViewCommandMapper = new(ViewHandler.ViewCommandMapper)
		{
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

