using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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