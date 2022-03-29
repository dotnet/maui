using System.Runtime.Versioning;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, W2DGraphicsView>
	{
		[SupportedOSPlatform("windows10.0.18362")]
		protected override W2DGraphicsView CreatePlatformView()
		{
			return new W2DGraphicsView();
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapBackground(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapShape(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.UpdateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapAspect(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapFill(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStroke(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeThickness(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeDashPattern(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeDashOffset(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeLineCap(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeLineJoin(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}

		[SupportedOSPlatform("windows10.0.18362")]
		public static void MapStrokeMiterLimit(IShapeViewHandler handler, IShapeView shapeView)
		{
			handler.PlatformView?.InvalidateShape(shapeView);
		}
	}
}