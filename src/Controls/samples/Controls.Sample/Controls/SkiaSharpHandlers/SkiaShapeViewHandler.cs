/*
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;

#if !NETSTANDARD
using Microsoft.Maui.Graphics.Skia.Views;
#else
using SkiaGraphicsView = System.Object;
#endif

namespace Maui.Controls.Sample.Controls
{
	public partial class SkiaShapeViewHandler : ViewHandler<IShapeView, SkiaGraphicsView>
	{
		public static IPropertyMapper<IShapeView, SkiaShapeViewHandler> ShapeViewMapper =
			new PropertyMapper<IShapeView, SkiaShapeViewHandler>(ViewHandler.ViewMapper)
			{
				[nameof(IShapeView.Shape)] = MapShape,
				[nameof(IShapeView.Aspect)] = MapShapeProperty,
				[nameof(IShapeView.Fill)] = MapShapeProperty,
				[nameof(IShapeView.Stroke)] = MapShapeProperty,
				[nameof(IShapeView.StrokeThickness)] = MapShapeProperty,
				[nameof(IShapeView.StrokeDashPattern)] = MapShapeProperty,
				[nameof(IShapeView.StrokeLineCap)] = MapShapeProperty,
				[nameof(IShapeView.StrokeLineJoin)] = MapShapeProperty,
				[nameof(IShapeView.StrokeMiterLimit)] = MapShapeProperty
			};

		public SkiaShapeViewHandler()
			: base(ShapeViewMapper)
		{
		}

		protected override SkiaGraphicsView CreateNativeView()
		{
#if __ANDROID__
			return new SkiaGraphicsView(Context);
#else
			return new SkiaGraphicsView();
#endif
		}

		public static void MapShape(SkiaShapeViewHandler handler, IShapeView shapeView)
		{
#if !NETSTANDARD
			handler.NativeView.Drawable = new ShapeDrawable(shapeView);
#endif
		}

		public static void MapShapeProperty(SkiaShapeViewHandler handler, IShapeView shapeView)
		{
#if __IOS__ || __MACCATALYST__
			handler.NativeView.SetNeedsDisplay();
#elif !NETSTANDARD
			handler.NativeView.Invalidate();
#endif
		}
	}
}
*/