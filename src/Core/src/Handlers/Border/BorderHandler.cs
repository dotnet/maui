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

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Represents the view handler for the abstract <see cref="IBorderView"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class BorderHandler : IBorderHandler
	{
		public static IPropertyMapper<IBorderView, IBorderHandler> Mapper = new PropertyMapper<IBorderView, IBorderHandler>(ViewMapper)
		{
#if __ANDROID__
			[nameof(IBorderView.Height)] = MapHeight,
			[nameof(IBorderView.Width)] = MapWidth,
#endif
			[nameof(IBorderView.Background)] = MapBackground,
			[nameof(IBorderView.Content)] = MapContent,
			[nameof(IBorderView.Shape)] = MapStrokeShape,
			[nameof(IBorderView.Stroke)] = MapStroke,
			[nameof(IBorderView.StrokeThickness)] = MapStrokeThickness,
			[nameof(IBorderView.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(IBorderView.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(IBorderView.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(IBorderView.StrokeDashOffset)] = MapStrokeDashOffset,
			[nameof(IBorderView.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public static CommandMapper<IBorderView, BorderHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		private Size _lastSize;

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

		/// <inheritdoc />
		public override void PlatformArrange(Rect rect)
		{
			base.PlatformArrange(rect);

			if (_lastSize != rect.Size)
			{
				_lastSize = rect.Size;
				UpdateValue(nameof(IBorderStroke.Shape));
			}
		}

		/// <summary>
		/// Maps the abstract <see cref="IView.Background"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapBackground(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateBackground(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IBorderStroke.Shape"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeShape(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeShape(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.Stroke"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStroke(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStroke(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeThickness"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeThickness(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeThickness(border);
			MapBackground(handler, border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeLineCap"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeLineCap(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineCap(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeLineJoin"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeLineJoin(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeLineJoin(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeDashPattern"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeDashPattern(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashPattern(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeDashOffset"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeDashOffset(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeDashOffset(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IStroke.StrokeMiterLimit"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapStrokeMiterLimit(IBorderHandler handler, IBorderView border)
		{
			((PlatformView?)handler.PlatformView)?.UpdateStrokeMiterLimit(border);
		}

		/// <summary>
		/// Maps the abstract <see cref="IContentView.Content"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static void MapContent(IBorderHandler handler, IBorderView border)
		{
			UpdateContent(handler);
		}

		static partial void UpdateContent(IBorderHandler handler);

#if __ANDROID__
		/// <summary>
		/// Maps the abstract <see cref="IView.Width"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static partial void MapWidth(IBorderHandler handler, IBorderView border);

		/// <summary>
		/// Maps the abstract <see cref="IView.Height"/> property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="border">The associated <see cref="IBorderView"/> instance.</param>
		public static partial void MapHeight(IBorderHandler handler, IBorderView border);
#endif
	}
}
