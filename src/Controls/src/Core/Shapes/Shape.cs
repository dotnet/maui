#nullable enable
using System;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
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
			=> Aspect switch
			{
				Stretch.Fill => PathAspect.Stretch,
				Stretch.Uniform => PathAspect.AspectFit,
				Stretch.UniformToFill => PathAspect.AspectFill,
				Stretch.None => PathAspect.None,
				_ => PathAspect.None
			};

		Paint IShapeView.Fill => Fill;

		Paint IStroke.Stroke => Stroke;

		LineCap IStroke.StrokeLineCap =>
			StrokeLineCap switch
			{
				PenLineCap.Flat => LineCap.Butt,
				PenLineCap.Round => LineCap.Round,
				PenLineCap.Square => LineCap.Square,
				_ => LineCap.Butt
			};

		LineJoin IStroke.StrokeLineJoin =>
			StrokeLineJoin switch
			{
				PenLineJoin.Round => LineJoin.Round,
				PenLineJoin.Bevel => LineJoin.Bevel,
				PenLineJoin.Miter => LineJoin.Miter,
				_ => LineJoin.Round
			};

		public float[] StrokeDashPattern
			=> StrokeDashArray.Select(a => (float)a).ToArray();

		float IStroke.StrokeDashOffset => (float)StrokeDashOffset;

		float IStroke.StrokeMiterLimit => (float)StrokeMiterLimit;

		static void OnBrushChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Shape)bindable).UpdateBrushParent((Brush)newValue);
		}

		void UpdateBrushParent(Brush brush)
		{
			if (brush != null)
				brush.Parent = this;
		}

		PathF IShape.PathForBounds(Graphics.Rectangle viewBounds)
		{
			if (HeightRequest < 0 && WidthRequest < 0)
				Frame = viewBounds;

			var path = GetPath();

#if !NETSTANDARD

			RectangleF pathBounds = viewBounds;

			try
			{
				pathBounds = path.GetBoundsByFlattening();
			}
			catch (Exception exc)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<Shape>()?.LogWarning(exc,"Exception while getting shape Bounds");
			}

			var transform = Matrix3x2.Identity;

			if (Aspect != Stretch.None)
			{
				viewBounds.X += StrokeThickness / 2;
				viewBounds.Y += StrokeThickness / 2;
				viewBounds.Width -= StrokeThickness;
				viewBounds.Height -= StrokeThickness;

				float factorX = (float)viewBounds.Width / pathBounds.Width;
				float factorY = (float)viewBounds.Height / pathBounds.Height;

				if (Aspect == Stretch.Uniform)
				{
					var factor = Math.Min(factorX, factorY);

					var width = pathBounds.Width * factor;
					var height = pathBounds.Height * factor;

					var translateX = (float)((viewBounds.Width - width) / 2 + viewBounds.X);
					var translateY = (float)((viewBounds.Height - height) / 2 + viewBounds.Y);

					transform = Matrix3x2.CreateTranslation(-pathBounds.X, -pathBounds.Y);
					transform *= Matrix3x2.CreateTranslation(translateX, translateY);
					transform *= Matrix3x2.CreateScale(factor, factor);
				}
				else if (Aspect == Stretch.UniformToFill)
				{
					var factor = (float)Math.Max(factorX, factorY);

					transform = Matrix3x2.CreateScale(factor, factor);

					var translateX = (float)(viewBounds.Left - factor * pathBounds.Left);
					var translateY = (float)(viewBounds.Top - factor * pathBounds.Top);

					transform *= Matrix3x2.CreateTranslation(translateX, translateY);
				}
				else if (Aspect == Stretch.Fill)
				{
					transform = Matrix3x2.CreateScale(factorX, factorY);

					var translateX = (float)(viewBounds.Left - factorX * pathBounds.Left);
					var translateY = (float)(viewBounds.Top - factorY * pathBounds.Top);

					transform *= Matrix3x2.CreateTranslation(translateX, translateY);
				}

				if (!transform.IsIdentity)
					path.Transform(transform);
			}
#endif

			return path;
		}
	}
}