using System;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public abstract partial class Shape : View, IShapeView, IShape
	{
		public Shape()
		{
		}

		public abstract PathF GetPath();

		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Shape), 1.0);

		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape), null,
				defaultValueCreator: bindable => new DoubleCollection());

		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Shape), 0.0);

		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Shape), PenLineCap.Flat);

		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Shape), PenLineJoin.Miter);

		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Shape), 10.0);

		public static readonly BindableProperty AspectProperty =
			BindableProperty.Create(nameof(Aspect), typeof(Stretch), typeof(Shape), Stretch.None);

		public Brush Fill
		{
			set { SetValue(FillProperty, value); }
			get { return (Brush)GetValue(FillProperty); }
		}

		public Brush Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public DoubleCollection StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
		}

		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		public Stretch Aspect
		{
			set { SetValue(AspectProperty, value); }
			get { return (Stretch)GetValue(AspectProperty); }
		}

		IShape IShapeView.Shape => this;

		PathAspect IShapeView.Aspect
			=> Aspect switch {
				Stretch.Fill => PathAspect.Stretch,
				Stretch.Uniform => PathAspect.AspectFit,
				Stretch.UniformToFill => PathAspect.AspectFill,
				Stretch.None => PathAspect.Center,
				_ => PathAspect.None
			};

		Paint IShapeView.Fill => Fill;

		Paint IShapeView.Stroke => Stroke;

		LineCap IShapeView.StrokeLineCap =>
			StrokeLineCap switch
			{
				PenLineCap.Flat => LineCap.Butt,
				PenLineCap.Round => LineCap.Round,
				PenLineCap.Square => LineCap.Square,
				_ => LineCap.Butt
			};

		LineJoin IShapeView.StrokeLineJoin =>
			StrokeLineJoin switch
			{
				PenLineJoin.Round => LineJoin.Round,
				PenLineJoin.Bevel => LineJoin.Bevel,
				PenLineJoin.Miter => LineJoin.Miter,
				_ => LineJoin.Round
			};

		public float[] StrokeDashPattern
			=> StrokeDashArray?.Select(a => (float)a)?.ToArray();

		float IShapeView.StrokeMiterLimit => (float)StrokeMiterLimit;

		static void OnBrushChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Shape)bindable).UpdateBrushParent((Brush)newValue);
		}

		void UpdateBrushParent(Brush brush)
		{
			if (brush != null)
				brush.Parent = this;
		}


		PathF IShape.PathForBounds(Graphics.Rectangle bounds)
		{
			var path = GetPath();

			// Scale the path if needed depending on specified aspect
			if (Aspect != Stretch.None)
			{
				var pathWidth = (float)Width;
				var pathHeight = (float)Height;

				var viewWidth = (float)bounds.Width;// 0f;
				var viewHeight = (float)bounds.Height;// 0f;

				// If one dimension is 0, we have nothing to display anyway
				if (pathWidth > 0 && pathHeight > 0)
				{
					if (Aspect == Stretch.Fill)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;

						path.Transform(AffineTransform.GetScaleInstance(scaleX, scaleY));
					}
					else if (Aspect == Stretch.UniformToFill)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;
						var scaleDimension = Math.Max(scaleX, scaleY);

						path.Transform(AffineTransform.GetScaleInstance(scaleDimension, scaleDimension));
					}
					else if (Aspect == Stretch.Uniform)
					{
						var scaleX = viewWidth / pathWidth;
						var scaleY = viewHeight / pathHeight;
						var scaleDimension = Math.Min(scaleX, scaleY);

						path.Transform(AffineTransform.GetScaleInstance(scaleDimension, scaleDimension));
					}
				}
			}

			return path;
		}
	}
}