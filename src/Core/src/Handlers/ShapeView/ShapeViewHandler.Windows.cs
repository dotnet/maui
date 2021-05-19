using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler : ViewHandler<IShapeView, FrameworkElement>
	{
		protected override Control CreateNativeView()
		{
			return new UserControl();
		}

		[MissingMapper]
		public static void MapShape(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapFill(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStroke(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeThickness(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeDashPattern(ShapeViewHandler handler, IShapeView shapeView) { }
	
		[MissingMapper]
		public static void MapStrokeLineCap(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeLineJoin(ShapeViewHandler handler, IShapeView shapeView) { }
		
		[MissingMapper]
		public static void MapStrokeMiterLimit(ShapeViewHandler handler, IShapeView shapeView) { }
	}
}