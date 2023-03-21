#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif __ANDROID__
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.ContentPanel;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : IBorderHandler
	{
		public static IPropertyMapper<IBorderView, IBorderHandler> Mapper = new PropertyMapper<IBorderView, IBorderHandler>(ViewMapper)
		{
#if __ANDROID__
			[nameof(IContentView.Height)] = MapHeight,
			[nameof(IContentView.Width)] = MapWidth,
#endif
			[nameof(IContentView.Background)] = MapBackground,
			[nameof(IContentView.Content)] = MapContent,
			[nameof(IBorderStroke.Shape)] = MapStrokeShape,
			[nameof(IBorderStroke.Stroke)] = MapStroke,
			[nameof(IBorderStroke.StrokeThickness)] = MapStrokeThickness,
			[nameof(IBorderStroke.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(IBorderStroke.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(IBorderStroke.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(IBorderStroke.StrokeDashOffset)] = MapStrokeDashOffset,
			[nameof(IBorderStroke.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public static CommandMapper<IBorderView, BorderHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public BorderHandler() : base(Mapper, CommandMapper)
		{

		}

		public BorderHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public BorderHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IBorderView IBorderHandler.VirtualView => VirtualView;

		PlatformView IBorderHandler.PlatformView => PlatformView;

		public static void MapBackground(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateBackground(border);
		}

		public static void MapStrokeShape(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		public static void MapStroke(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeThickness(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeLineCap(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineCap(border);
		}

		public static void MapStrokeLineJoin(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineJoin(border);
		}

		public static void MapStrokeDashPattern(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashPattern(border);
		}

		public static void MapStrokeDashOffset(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashOffset(border);
		}

		public static void MapStrokeMiterLimit(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeMiterLimit(border);
		}
	}
}
