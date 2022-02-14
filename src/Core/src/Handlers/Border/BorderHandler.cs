#nullable enable
#if __IOS__ || MACCATALYST
using NativeView = Microsoft.Maui.Platform.ContentView;
#elif __ANDROID__
using NativeView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using NativeView = Microsoft.Maui.Platform.ContentPanel;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : IBorderHandler
	{
		public static IPropertyMapper<IBorderView, IBorderHandler> Mapper = new PropertyMapper<IBorderView, IBorderHandler>(ViewMapper)
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

		public static CommandMapper<IBorderView, BorderHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public BorderHandler() : base(Mapper, CommandMapper)
		{

		}

		protected BorderHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? ViewCommandMapper)
		{
		}

		public BorderHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		IBorderView IBorderHandler.VirtualView => VirtualView;

		NativeView IBorderHandler.NativeView => NativeView;

		public static void MapBackground(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateBackground(border);
		}

		public static void MapStrokeShape(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		public static void MapStroke(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeThickness(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeLineCap(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeLineCap(border);
		}

		public static void MapStrokeLineJoin(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeLineJoin(border);
		}

		public static void MapStrokeDashPattern(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeDashPattern(border);
		}

		public static void MapStrokeDashOffset(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeDashOffset(border);
		}

		public static void MapStrokeMiterLimit(IBorderHandler handler, IBorderView border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeMiterLimit(border);
		}
	}
}
