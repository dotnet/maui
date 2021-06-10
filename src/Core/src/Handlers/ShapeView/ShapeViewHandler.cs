using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class ShapeViewHandler
	{
		public static PropertyMapper<IShapeView, ShapeViewHandler> ShapeViewMapper = new PropertyMapper<IShapeView, ShapeViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(IShapeView.Aspect)] = MapAspect,
			[nameof(IShapeView.Fill)] = MapFill,
			[nameof(IShapeView.Stroke)] = MapStroke,
			[nameof(IShapeView.StrokeThickness)] = MapStrokeThickness,
			[nameof(IShapeView.StrokeDashPattern)] = MapStrokeDashPattern,
			[nameof(IShapeView.StrokeLineCap)] = MapStrokeLineCap,
			[nameof(IShapeView.StrokeLineJoin)] = MapStrokeLineJoin,
			[nameof(IShapeView.StrokeMiterLimit)] = MapStrokeMiterLimit
		};

		public ShapeViewHandler() : base(ShapeViewMapper)
		{

		}

		public ShapeViewHandler(PropertyMapper mapper) : base(mapper ?? ShapeViewMapper)
		{

		}
	}
}