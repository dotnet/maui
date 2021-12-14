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
	public partial class BorderHandler : IViewHandler
	{
		public static IPropertyMapper<IBorder, BorderHandler> BorderMapper = new PropertyMapper<IBorder, BorderHandler>(ViewMapper)
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

		public static CommandMapper<IBorder, BorderHandler> BorderCommandMapper = new(ViewCommandMapper)
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

		public static void MapBackground(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateBackground(border);
		}

		public static void MapStrokeShape(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		public static void MapStroke(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeThickness(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		public static void MapStrokeLineCap(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeLineCap(border);
		}

		public static void MapStrokeLineJoin(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeLineJoin(border);
		}

		public static void MapStrokeDashPattern(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeDashPattern(border);
		}

		public static void MapStrokeDashOffset(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeDashOffset(border);
		}

		public static void MapStrokeMiterLimit(BorderHandler handler, IBorder border)
		{
			((NativeView?)handler.NativeView)?.UpdateStrokeMiterLimit(border);
		}
	}
}
