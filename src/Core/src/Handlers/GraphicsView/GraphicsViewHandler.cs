#nullable enable
#if __IOS__ || MACCATALYST || MONOANDROID || WINDOWS || TIZEN
#define PLATFORM
using PlatformView = Microsoft.Maui.Platform.PlatformTouchGraphicsView;
#else
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

		public GraphicsViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public GraphicsViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IGraphicsView IGraphicsViewHandler.VirtualView => VirtualView;

		PlatformView IGraphicsViewHandler.PlatformView => PlatformView;

		protected override void ConnectHandler(PlatformView platformView)
		{
#if PLATFORM
			platformView.Connect(VirtualView);
#endif
			base.ConnectHandler(platformView);
		}
		protected override void DisconnectHandler(PlatformView platformView)
		{
#if PLATFORM
			platformView.Disconnect();
#endif
			base.DisconnectHandler(platformView);
		}
	}
}