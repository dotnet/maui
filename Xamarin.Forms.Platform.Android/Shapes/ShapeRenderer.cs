using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Xamarin.Forms.Shapes;
using AColor = Android.Graphics.Color;
using AMatrix = Android.Graphics.Matrix;
using APath = Android.Graphics.Path;
using AView = Android.Views.View;
using Shape = Xamarin.Forms.Shapes.Shape;

namespace Xamarin.Forms.Platform.Android
{
	public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
		 where TShape : Shape
		 where TNativeShape : ShapeView
	{
		double _height;
		double _width;

		public ShapeRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<TShape> args)
		{
			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateSize();
				UpdateAspect();
				UpdateFill();
				UpdateStroke();
				UpdateStrokeThickness();
				UpdateStrokeDashArray();
				UpdateStrokeDashOffset();
				UpdateStrokeLineCap();
				UpdateStrokeLineJoin();
				UpdateStrokeMiterLimit();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == VisualElement.HeightProperty.PropertyName)
			{
				_height = Element.Height;
				UpdateSize();
			}
			else if (args.PropertyName == VisualElement.WidthProperty.PropertyName)
			{
				_width = Element.Width;
				UpdateSize();
			}
			else if (args.PropertyName == Shape.AspectProperty.PropertyName)
				UpdateAspect();
			else if (args.PropertyName == Shape.FillProperty.PropertyName)
				UpdateFill();
			else if (args.PropertyName == Shape.StrokeProperty.PropertyName)
				UpdateStroke();
			else if (args.PropertyName == Shape.StrokeThicknessProperty.PropertyName)
				UpdateStrokeThickness();
			else if (args.PropertyName == Shape.StrokeDashArrayProperty.PropertyName)
				UpdateStrokeDashArray();
			else if (args.PropertyName == Shape.StrokeDashOffsetProperty.PropertyName)
				UpdateStrokeDashOffset();
			else if (args.PropertyName == Shape.StrokeLineCapProperty.PropertyName)
				UpdateStrokeLineCap();
			else if (args.PropertyName == Shape.StrokeLineJoinProperty.PropertyName)
				UpdateStrokeLineJoin();
			else if (args.PropertyName == Shape.StrokeMiterLimitProperty.PropertyName)
				UpdateStrokeMiterLimit();
		}

		public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			if (Element != null)
			{
				return Control.GetDesiredSize();
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		void UpdateSize()
		{
			Control.UpdateSize(_width, _height);
		}

		void UpdateAspect()
		{
			Control.UpdateAspect(Element.Aspect);
		}

		void UpdateFill()
		{
			Control.UpdateFill(Element.Fill);
		}

		void UpdateStroke()
		{
			Control.UpdateStroke(Element.Stroke);
		}

		void UpdateStrokeThickness()
		{
			Control.UpdateStrokeThickness((float)Element.StrokeThickness);
		}

		void UpdateStrokeDashArray()
		{
			Control.UpdateStrokeDashArray(Element.StrokeDashArray.ToArray());
		}

		void UpdateStrokeDashOffset()
		{
			Control.UpdateStrokeDashOffset((float)Element.StrokeDashOffset);
		}

		void UpdateStrokeLineCap()
		{
			PenLineCap lineCap = Element.StrokeLineCap;
			Paint.Cap aLineCap = Paint.Cap.Butt;

			switch (lineCap)
			{
				case PenLineCap.Flat:
					aLineCap = Paint.Cap.Butt;
					break;
				case PenLineCap.Square:
					aLineCap = Paint.Cap.Square;
					break;
				case PenLineCap.Round:
					aLineCap = Paint.Cap.Round;
					break;
			}

			Control.UpdateStrokeLineCap(aLineCap);
		}

		void UpdateStrokeLineJoin()
		{
			PenLineJoin lineJoin = Element.StrokeLineJoin;
			Paint.Join aLineJoin = Paint.Join.Miter;

			switch (lineJoin)
			{
				case PenLineJoin.Miter:
					aLineJoin = Paint.Join.Miter;
					break;
				case PenLineJoin.Bevel:
					aLineJoin = Paint.Join.Bevel;
					break;
				case PenLineJoin.Round:
					aLineJoin = Paint.Join.Round;
					break;
			}

			Control.UpdateStrokeLineJoin(aLineJoin);
		}

		void UpdateStrokeMiterLimit()
		{
			Control.UpdateStrokeMiterLimit((float)Element.StrokeMiterLimit);
		}
	}

	public class ShapeView : AView
	{
		readonly ShapeDrawable _drawable;
		protected float _density;

		APath _path;
		readonly RectF _pathFillBounds;
		readonly RectF _pathStrokeBounds;

		Brush _stroke;
		Brush _fill;

		Shader _strokeShader;
		Shader _fillShader;

		float _strokeWidth;
		float[] _strokeDash;
		float _strokeDashOffset;

		Stretch _aspect;

		AMatrix _transform;

		public ShapeView(Context context) : base(context)
		{
			_drawable = new ShapeDrawable(null);
			_drawable.Paint.AntiAlias = true;

			_density = Resources.DisplayMetrics.Density;

			_pathFillBounds = new RectF();
			_pathStrokeBounds = new RectF();

			_aspect = Stretch.None;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (_path == null)
				return;

			AMatrix transformMatrix = CreateMatrix();

			_path.Transform(transformMatrix);
			transformMatrix.MapRect(_pathFillBounds);
			transformMatrix.MapRect(_pathStrokeBounds);

			if (_fill != null)
			{
				_drawable.Paint.SetStyle(Paint.Style.Fill);

				if (_fill is GradientBrush fillGradientBrush)
				{
					if (fillGradientBrush is LinearGradientBrush linearGradientBrush)
						_fillShader = CreateLinearGradient(linearGradientBrush, _pathFillBounds);

					if (fillGradientBrush is RadialGradientBrush radialGradientBrush)
						_fillShader = CreateRadialGradient(radialGradientBrush, _pathFillBounds);

					_drawable.Paint.SetShader(_fillShader);
				}
				else
				{
					AColor fillColor = Color.Default.ToAndroid();

					if (_fill is SolidColorBrush solidColorBrush && solidColorBrush.Color != Color.Default)
						fillColor = solidColorBrush.Color.ToAndroid();

					_drawable.Paint.Color = fillColor;
				}

				_drawable.Draw(canvas);
				_drawable.Paint.SetShader(null);
			}

			if (_stroke != null)
			{
				_drawable.Paint.SetStyle(Paint.Style.Stroke);

				if (_stroke is GradientBrush strokeGradientBrush)
				{
					UpdatePathStrokeBounds();

					if (strokeGradientBrush is LinearGradientBrush linearGradientBrush)
						_strokeShader = CreateLinearGradient(linearGradientBrush, _pathStrokeBounds);

					if (strokeGradientBrush is RadialGradientBrush radialGradientBrush)
						_strokeShader = CreateRadialGradient(radialGradientBrush, _pathStrokeBounds);

					_drawable.Paint.SetShader(_strokeShader);
				}
				else
				{
					AColor strokeColor = Color.Default.ToAndroid();

					if (_stroke is SolidColorBrush solidColorBrush && solidColorBrush.Color != Color.Default)
						strokeColor = solidColorBrush.Color.ToAndroid();

					_drawable.Paint.Color = strokeColor;
				}

				_drawable.Draw(canvas);
				_drawable.Paint.SetShader(null);
			}

			AMatrix inverseTransformMatrix = new AMatrix();
			transformMatrix.Invert(inverseTransformMatrix);
			_path.Transform(inverseTransformMatrix);
			inverseTransformMatrix.MapRect(_pathFillBounds);
			inverseTransformMatrix.MapRect(_pathStrokeBounds);
		}

		public void UpdateShape(APath path)
		{
			_path = path;
			UpdatePathShape();
		}

		public void UpdateShapeTransform(AMatrix matrix)
		{
			_transform = matrix;
			_path.Transform(_transform);
			Invalidate();
		}

		public SizeRequest GetDesiredSize()
		{
			if (_path != null)
			{
				return new SizeRequest(new Size(
					Math.Max(0, _pathStrokeBounds.Right),
					Math.Max(0, _pathStrokeBounds.Bottom)));
			}

			return new SizeRequest();
		}

		public void UpdateAspect(Stretch aspect)
		{
			_aspect = aspect;
			_fillShader = null;
			_strokeShader = null;
			Invalidate();
		}

		public void UpdateFill(Brush fill)
		{
			_fill = fill;
			_fillShader = null;
			Invalidate();
		}

		public void UpdateStroke(Brush stroke)
		{
			_stroke = stroke;
			_strokeShader = null;
			Invalidate();
		}

		public void UpdateStrokeThickness(float strokeWidth)
		{
			_strokeWidth = _density * strokeWidth;
			_drawable.Paint.StrokeWidth = _strokeWidth;
			UpdateStrokeDash();
		}

		public void UpdateStrokeDashArray(float[] dash)
		{
			_strokeDash = dash;
			UpdateStrokeDash();
		}

		public void UpdateStrokeDashOffset(float strokeDashOffset)
		{
			_strokeDashOffset = strokeDashOffset;
			UpdateStrokeDash();
		}

		public void UpdateStrokeDash()
		{
			if (_strokeDash != null && _strokeDash.Length > 1)
			{
				float[] strokeDash = new float[_strokeDash.Length];

				for (int i = 0; i < _strokeDash.Length; i++)
					strokeDash[i] = _strokeDash[i] * _strokeWidth;

				_drawable.Paint.SetPathEffect(new DashPathEffect(strokeDash, _strokeDashOffset * _strokeWidth));
			}
			else
				_drawable.Paint.SetPathEffect(null);

			UpdatePathStrokeBounds();
		}

		public void UpdateStrokeLineCap(Paint.Cap strokeCap)
		{
			_drawable.Paint.StrokeCap = strokeCap;
			UpdatePathStrokeBounds();
		}

		public void UpdateStrokeLineJoin(Paint.Join strokeJoin)
		{
			_drawable.Paint.StrokeJoin = strokeJoin;
			Invalidate();
		}

		public void UpdateStrokeMiterLimit(float strokeMiterLimit)
		{
			_drawable.Paint.StrokeMiter = strokeMiterLimit * 2;
			UpdatePathStrokeBounds();
		}

		public void UpdateSize(double width, double height)
		{
			_drawable.SetBounds(0, 0, (int)(width * _density), (int)(height * _density));
			UpdatePathShape();
		}

		protected void UpdatePathShape()
		{
			if (_path != null && !_drawable.Bounds.IsEmpty)
				_drawable.Shape = new PathShape(_path, _drawable.Bounds.Width(), _drawable.Bounds.Height());
			else
				_drawable.Shape = null;

			if (_path != null)
			{
				using (APath fillPath = new APath())
				{
					_drawable.Paint.StrokeWidth = 0.01f;
					_drawable.Paint.SetStyle(Paint.Style.Stroke);
					_drawable.Paint.GetFillPath(_path, fillPath);
					fillPath.ComputeBounds(_pathFillBounds, false);
					_drawable.Paint.StrokeWidth = _strokeWidth;
				}
			}
			else
			{
				_pathFillBounds.SetEmpty();
			}

			_fillShader = null;
			UpdatePathStrokeBounds();
		}

		AMatrix CreateMatrix()
		{
			AMatrix matrix = new AMatrix();

			RectF drawableBounds = new RectF(_drawable.Bounds);
			float halfStrokeWidth = _drawable.Paint.StrokeWidth / 2;

			drawableBounds.Left += halfStrokeWidth;
			drawableBounds.Top += halfStrokeWidth;
			drawableBounds.Right -= halfStrokeWidth;
			drawableBounds.Bottom -= halfStrokeWidth;

			switch (_aspect)
			{
				case Stretch.None:
					break;
				case Stretch.Fill:
					matrix.SetRectToRect(_pathFillBounds, drawableBounds, AMatrix.ScaleToFit.Fill);
					break;
				case Stretch.Uniform:
					matrix.SetRectToRect(_pathFillBounds, drawableBounds, AMatrix.ScaleToFit.Center);
					break;
				case Stretch.UniformToFill:
					float widthScale = drawableBounds.Width() / _pathFillBounds.Width();
					float heightScale = drawableBounds.Height() / _pathFillBounds.Height();
					float maxScale = Math.Max(widthScale, heightScale);
					matrix.SetScale(maxScale, maxScale);
					matrix.PostTranslate(
						drawableBounds.Left - maxScale * _pathFillBounds.Left,
						drawableBounds.Top - maxScale * _pathFillBounds.Top);
					break;
			}

			return matrix;
		}

		void UpdatePathStrokeBounds()
		{
			if (_path != null)
			{
				using (APath strokePath = new APath())
				{
					_drawable.Paint.SetStyle(Paint.Style.Stroke);
					_drawable.Paint.GetFillPath(_path, strokePath);
					strokePath.ComputeBounds(_pathStrokeBounds, false);
				}
			}
			else
			{
				_pathStrokeBounds.SetEmpty();
			}

			_strokeShader = null;
			Invalidate();
		}

		LinearGradient CreateLinearGradient(LinearGradientBrush linearGradientBrush, RectF pathBounds)
		{
			if (_path == null)
				return null;

			int[] colors = new int[linearGradientBrush.GradientStops.Count];
			float[] offsets = new float[linearGradientBrush.GradientStops.Count];

			for (int index = 0; index < linearGradientBrush.GradientStops.Count; index++)
			{
				colors[index] = linearGradientBrush.GradientStops[index].Color.ToAndroid();
				offsets[index] = linearGradientBrush.GradientStops[index].Offset;
			}

			Shader.TileMode tilemode = Shader.TileMode.Clamp;

			using (RectF gradientBounds = new RectF(pathBounds))
			{
				return new
					LinearGradient(
					(float)linearGradientBrush.StartPoint.X * gradientBounds.Width() + gradientBounds.Left,
					(float)linearGradientBrush.StartPoint.Y * gradientBounds.Height() + gradientBounds.Top,
					(float)linearGradientBrush.EndPoint.X * gradientBounds.Width() + gradientBounds.Left,
					(float)linearGradientBrush.EndPoint.Y * gradientBounds.Height() + gradientBounds.Top,
					colors,
					offsets,
					tilemode);
			}
		}

		RadialGradient CreateRadialGradient(RadialGradientBrush radialGradientBrush, RectF pathBounds)
		{
			if (_path == null)
				return null;

			int gradientStopsCount = radialGradientBrush.GradientStops.Count;
			AColor centerColor = gradientStopsCount > 0 ? radialGradientBrush.GradientStops[0].Color.ToAndroid() : Color.Default.ToAndroid();
			AColor edgeColor = gradientStopsCount > 0 ? radialGradientBrush.GradientStops[gradientStopsCount - 1].Color.ToAndroid() : Color.Default.ToAndroid();

			float[] offsets = new float[radialGradientBrush.GradientStops.Count];

			for (int index = 0; index < radialGradientBrush.GradientStops.Count; index++)
				offsets[index] = radialGradientBrush.GradientStops[index].Offset;

			Shader.TileMode tilemode = Shader.TileMode.Clamp;

			using (RectF gradientBounds = new RectF(pathBounds))
			{
				return new RadialGradient(
					(float)radialGradientBrush.Center.X * gradientBounds.Width() + gradientBounds.Left,
					(float)radialGradientBrush.Center.Y * gradientBounds.Height() + gradientBounds.Top,
					(float)radialGradientBrush.Radius * Math.Max(gradientBounds.Height(), gradientBounds.Width()),
					centerColor,
					edgeColor,
					tilemode);
			}
		}
	}
}