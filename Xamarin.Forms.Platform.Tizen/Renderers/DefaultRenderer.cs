using Xamarin.Forms.Shapes;
using ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen
{
	public sealed class DefaultRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			if (NativeView == null)
			{
				var control = new ELayout(Forms.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}

	public class EllipseRenderer : ShapeRenderer { }

	public class LineRenderer : ShapeRenderer { }

	public class PathRenderer : ShapeRenderer { }

	public class PolygonRenderer : ShapeRenderer { }

	public class PolylineRenderer : ShapeRenderer { }

	public class RectangleRenderer : ShapeRenderer { }

	public class ShapeRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			Log.Info("Use skia render mode (InitializationOptions.UseSkiaSharp=true) to use Shape.");
			if (NativeView == null)
			{
				var control = new ELayout(Forms.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}
}
