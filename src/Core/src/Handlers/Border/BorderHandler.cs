#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif __ANDROID__
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.ContentPanel;
#elif NETSTANDARD
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : IViewHandler
	{
		public static IPropertyMapper<IBorderView, BorderHandler> BorderMapper = new PropertyMapper<IBorderView, BorderHandler>(ViewMapper)
		{
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

		public static CommandMapper<IBorderView, BorderHandler> BorderCommandMapper = new(ViewCommandMapper)
		{
		};

		public BorderHandler() : base(BorderMapper, BorderCommandMapper)
		{

		}

		protected BorderHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		public BorderHandler(IPropertyMapper? mapper = null) : base(mapper ?? BorderMapper)
		{

		}

		public static void MapBackground(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateBackground(border);
		}

		public static void MapStrokeShape(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		public static void MapStroke(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeThickness(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeLineCap(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineCap(border);
		}

		public static void MapStrokeLineJoin(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineJoin(border);
		}

		public static void MapStrokeDashPattern(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashPattern(border);
		}

		public static void MapStrokeDashOffset(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashOffset(border);
		}

		public static void MapStrokeMiterLimit(BorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeMiterLimit(border);
		}
	}
}
