using SkiaSharp;
using Path = Xamarin.Forms.Shapes.Path;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class PathRenderer : ShapeRenderer<Path, PathView>
	{
		public PathRenderer() : base()
		{
			RegisterPropertyHandler(Path.DataProperty, UpdateData);
			RegisterPropertyHandler(Path.RenderTransformProperty, UpdateRenderTransform);
		}
		protected override void OnElementChanged(ElementChangedEventArgs<Path> e)
		{
			if (Control == null)
			{
				SetNativeControl(new PathView());
			}

			base.OnElementChanged(e);
		}

		void UpdateData()
		{
			Control.UpdateData(Element.Data.ToSKPath());
		}

		void UpdateRenderTransform()
		{
			UpdateData();

			if (Element.RenderTransform != null)
			{
				Control.UpdateTransform(Element.RenderTransform.ToSkia());
			}
		}
	}

	public class PathView : ShapeView
	{
		public PathView() : base()
		{
		}

		public void UpdateData(SKPath path)
		{
			UpdateShape(path);
		}

		public void UpdateTransform(SKMatrix transform)
		{
			UpdateShapeTransform(transform);
		}
	}
}
