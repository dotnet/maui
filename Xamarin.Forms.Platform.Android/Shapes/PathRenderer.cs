using System.ComponentModel;
using Android.Content;
using AMatrix = Android.Graphics.Matrix;
using APath = Android.Graphics.Path;
using Path = Xamarin.Forms.Shapes.Path;

namespace Xamarin.Forms.Platform.Android
{
	public class PathRenderer : ShapeRenderer<Path, PathView>
	{
		public PathRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Path> args)
		{
			if (Control == null)
			{
				SetNativeControl(new PathView(Context));
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateData();
				UpdateRenderTransform();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Path.DataProperty.PropertyName)
				UpdateData();
			else if (args.PropertyName == Path.RenderTransformProperty.PropertyName)
			{
				UpdateData();
				UpdateRenderTransform();
			}
		}

		void UpdateData()
		{
			Control.UpdateData(Element.Data.ToAPath(Context));
		}

		void UpdateRenderTransform()
		{
			if (Element.RenderTransform != null)
			{
				var density = Resources.DisplayMetrics.Density;
				Control.UpdateTransform(Element.RenderTransform.ToAndroid(density));
			}
		}
	}

	public class PathView : ShapeView
	{
		public PathView(Context context) : base(context)
		{
		}

		public void UpdateData(APath path)
		{
			UpdateShape(path);
		}

		public void UpdateTransform(AMatrix transform)
		{
			UpdateShapeTransform(transform);
		}
	}
}