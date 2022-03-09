#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Graphics.Platform.PlatformGraphicsView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Graphics.Platform.PlatformGraphicsView;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Graphics.Win2D.W2DGraphicsView;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : IGraphicsViewHandler
	{
		public static IPropertyMapper<IGraphicsView, IGraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, IGraphicsViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IGraphicsView.Drawable)] = MapDrawable,
			[nameof(IView.FlowDirection)] = MapFlowDirection
		};

		public static CommandMapper<IGraphicsView, IGraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IGraphicsView.Invalidate)] = MapInvalidate
		};

		public GraphicsViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public GraphicsViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IGraphicsView IGraphicsViewHandler.VirtualView => VirtualView;

		PlatformView IGraphicsViewHandler.PlatformView => PlatformView;
	}
}