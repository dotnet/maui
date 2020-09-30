using System.ComponentModel;
using Android.Content;
using Xamarin.Forms.Shapes;
using APath = Android.Graphics.Path;

namespace Xamarin.Forms.Platform.Android
{
	public class LineRenderer : ShapeRenderer<Line, LineView>
	{
		public LineRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Line> args)
		{
			if (Control == null)
			{
				SetNativeControl(new LineView(Context));
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateX1();
				UpdateY1();
				UpdateX2();
				UpdateY2();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Line.X1Property.PropertyName)
				UpdateX1();
			else if (args.PropertyName == Line.Y1Property.PropertyName)
				UpdateY1();
			else if (args.PropertyName == Line.X2Property.PropertyName)
				UpdateX2();
			else if (args.PropertyName == Line.Y2Property.PropertyName)
				UpdateY2();
		}

		void UpdateX1()
		{
			Control.UpdateX1((float)Element.X1);
		}

		void UpdateY1()
		{
			Control.UpdateY1((float)Element.Y1);
		}

		void UpdateX2()
		{
			Control.UpdateX2((float)Element.X2);
		}

		void UpdateY2()
		{
			Control.UpdateY2((float)Element.Y2);
		}
	}

	public class LineView : ShapeView
	{
		float _x1, _y1, _x2, _y2;

		public LineView(Context context) : base(context)
		{
		}

		void UpdateShape()
		{
			var path = new APath();
			path.MoveTo(_x1, _y1);
			path.LineTo(_x2, _y2);
			UpdateShape(path);
		}

		public void UpdateX1(float x1)
		{
			_x1 = _density * x1;
			UpdateShape();
		}

		public void UpdateY1(float y1)
		{
			_y1 = _density * y1;
			UpdateShape();
		}

		public void UpdateX2(float x2)
		{
			_x2 = _density * x2;
			UpdateShape();
		}

		public void UpdateY2(float y2)
		{
			_y2 = _density * y2;
			UpdateShape();
		}
	}
}