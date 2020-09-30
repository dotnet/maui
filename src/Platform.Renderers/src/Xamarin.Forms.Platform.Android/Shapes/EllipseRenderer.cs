using Android.Content;
using Xamarin.Forms.Shapes;
using APath = Android.Graphics.Path;

namespace Xamarin.Forms.Platform.Android
{
	public class EllipseRenderer : ShapeRenderer<Ellipse, EllipseView>
	{
		public EllipseRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> args)
		{
			if (Control == null)
			{
				SetNativeControl(new EllipseView(Context));
			}

			base.OnElementChanged(args);
		}
	}

	public class EllipseView : ShapeView
	{
		public EllipseView(Context context) : base(context)
		{
			UpdateShape();
		}

		void UpdateShape()
		{
			var path = new APath();
			path.AddCircle(0, 0, 1, APath.Direction.Cw);
			UpdateShape(path);
		}
	}
}