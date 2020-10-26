using SkiaSharp;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class EllipseRenderer : ShapeRenderer<Ellipse, EllipseView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> e)
		{
			if (Control == null)
			{
				SetNativeControl(new EllipseView());
			}

			base.OnElementChanged(e);
		}
	}

	public class EllipseView : ShapeView
	{
		public EllipseView() : base()
		{
			UpdateShape();
		}

		void UpdateShape()
		{
			var path = new SKPath();
			path.AddCircle(0, 0, 1);
			UpdateShape(path);
		}
	}
}