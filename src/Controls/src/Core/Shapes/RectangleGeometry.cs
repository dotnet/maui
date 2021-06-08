using Rect = Microsoft.Maui.Graphics.Rectangle;
namespace Microsoft.Maui.Controls.Shapes
{
	public class RectangleGeometry : Geometry
	{
		public RectangleGeometry()
		{

		}

		public RectangleGeometry(Rect rect)
		{
			Rect = rect;
		}

		public static readonly BindableProperty RectProperty =
			BindableProperty.Create(nameof(Rect), typeof(Rect), typeof(RectangleGeometry), new Rect());

		public Rect Rect
		{
			set { SetValue(RectProperty, value); }
			get { return (Rect)GetValue(RectProperty); }
		}

		public override Graphics.PathF PathForBounds(Rect rect)
		{
			var path = new Graphics.PathF();

			path.AppendRectangle(rect);

			return path;
		}
	}
}