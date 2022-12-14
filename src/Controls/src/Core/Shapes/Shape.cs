#nullable enable
using System;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Shape']/Docs/*" />
	public abstract partial class Shape : View, IShapeView, IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Shape()
		{
		}

		public abstract PathF GetPath();

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='FillProperty']/Docs/*" />
		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeProperty']/Docs/*" />
		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeThicknessProperty']/Docs/*" />
		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Shape), 1.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeDashArrayProperty']/Docs/*" />
		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape), null,
				defaultValueCreator: bindable => new DoubleCollection());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeDashOffsetProperty']/Docs/*" />
		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Shape), 0.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeLineCapProperty']/Docs/*" />
		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Shape), PenLineCap.Flat);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeLineJoinProperty']/Docs/*" />
		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Shape), PenLineJoin.Miter);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeMiterLimitProperty']/Docs/*" />
		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Shape), 10.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='AspectProperty']/Docs/*" />
		public static readonly BindableProperty AspectProperty =
			BindableProperty.Create(nameof(Aspect), typeof(Stretch), typeof(Shape), Stretch.None);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='Fill']/Docs/*" />
		public Brush Fill
		{
			set { SetValue(FillProperty, value); }
			get { return (Brush)GetValue(FillProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='Stroke']/Docs/*" />
		public Brush Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush)GetValue(StrokeProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeThickness']/Docs/*" />
		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeDashArray']/Docs/*" />
		public DoubleCollection StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeDashOffset']/Docs/*" />
		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeLineCap']/Docs/*" />
		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeLineJoin']/Docs/*" />
		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='StrokeMiterLimit']/Docs/*" />
		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='Aspect']/Docs/*" />
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

		PathF IShape.PathForBounds(Graphics.Rect viewBounds)
		{
			if (HeightRequest < 0 && WidthRequest < 0)
			{
				Frame = viewBounds;
			}

			var path = GetPath();

#if !(NETSTANDARD || !PLATFORM)

			// TODO: not using this.GetPath().Bounds.Size;
			//       since default GetBoundsByFlattening(0.001) returns incorrect results for curves
			RectF pathBounds = path.GetBoundsByFlattening(1);

			viewBounds.X += StrokeThickness / 2;
			viewBounds.Y += StrokeThickness / 2;
			viewBounds.Width -= StrokeThickness;
			viewBounds.Height -= StrokeThickness;

			Matrix3x2 transform;

			if (Aspect == Stretch.None)
			{
				bool requireAdjustX = viewBounds.Left > pathBounds.Left;
				bool requireAdjustY = viewBounds.Top > pathBounds.Top;

				if (requireAdjustX || requireAdjustY)
				{
					transform = Matrix3x2.CreateTranslation(
						(float)(pathBounds.X + viewBounds.Left - pathBounds.Left),
						(float)(pathBounds.Y + viewBounds.Top - pathBounds.Top));
				}
				else
				{
					transform = Matrix3x2.Identity;
				}
			}
			else
			{
				transform = Matrix3x2.Identity;

				float calculatedWidth = (float)(viewBounds.Width / pathBounds.Width);
				float calculatedHeight = (float)(viewBounds.Height / pathBounds.Height);

				float widthScale = float.IsNaN(calculatedWidth) ? 0 : calculatedWidth;
				float heightScale = float.IsNaN(calculatedHeight) ? 0 : calculatedHeight;

				switch (Aspect)
				{
					case Stretch.None:
						break;

					case Stretch.Fill:
						transform *= Matrix3x2.CreateScale(widthScale, heightScale);

						transform *= Matrix3x2.CreateTranslation(
							(float)(viewBounds.Left - widthScale * pathBounds.Left),
							(float)(viewBounds.Top - heightScale * pathBounds.Top));
						break;

					case Stretch.Uniform:
						float minScale = Math.Min(widthScale, heightScale);

						transform *= Matrix3x2.CreateScale(minScale, minScale);

						transform *= Matrix3x2.CreateTranslation(
							(float)(viewBounds.Left - minScale * pathBounds.Left +
							(viewBounds.Width - minScale * pathBounds.Width) / 2),
							(float)(viewBounds.Top - minScale * pathBounds.Top +
							(viewBounds.Height - minScale * pathBounds.Height) / 2));
						break;

					case Stretch.UniformToFill:
						float maxScale = Math.Max(widthScale, heightScale);

						transform *= Matrix3x2.CreateScale(maxScale, maxScale);

						transform *= Matrix3x2.CreateTranslation(
							(float)(viewBounds.Left - maxScale * pathBounds.Left),
							(float)(viewBounds.Top - maxScale * pathBounds.Top));
						break;
				}
			}

			if (!transform.IsIdentity)
				path.Transform(transform);
#endif

			return path;
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			var result = base.MeasureOverride(widthConstraint, heightConstraint);

			if (result.Width != 0 && result.Height != 0)
			{
				return result;
			}

			// TODO: not using this.GetPath().Bounds.Size;
			//       since default GetBoundsByFlattening(0.001) returns incorrect results for curves
			RectF pathBounds = this.GetPath().GetBoundsByFlattening(1);
			SizeF boundsByFlattening = pathBounds.Size;

			result.Height = boundsByFlattening.Height;
			result.Width = boundsByFlattening.Width;

			widthConstraint -= StrokeThickness;
			heightConstraint -= StrokeThickness;

			double scaleX = widthConstraint / result.Width;
			double scaleY = heightConstraint / result.Height;
			scaleX = double.IsNaN(scaleX) ? 0 : scaleX;
			scaleY = double.IsNaN(scaleY) ? 0 : scaleY;

			switch (Aspect)
			{
				case Stretch.None:
					result.Height += pathBounds.Y;
					result.Width += pathBounds.X;
					break;

				case Stretch.Fill:
					if (!double.IsInfinity(heightConstraint))
					{
						result.Height = heightConstraint;
					}

					if (!double.IsInfinity(widthConstraint))
					{
						result.Width = widthConstraint;
					}
					break;

				case Stretch.Uniform:
					double minScale = Math.Min(scaleX, scaleY);
					if (!double.IsInfinity(minScale))
					{
						result.Height *= minScale;
						result.Width *= minScale;
					}
					break;

				case Stretch.UniformToFill:
					scaleX = double.IsInfinity(scaleX) ? 0 : scaleX;
					scaleY = double.IsInfinity(scaleY) ? 0 : scaleY;
					double maxScale = Math.Max(scaleX, scaleY);

					if (maxScale != 0)
					{
						result.Height *= maxScale;
						result.Width *= maxScale;
					}
					break;
			}

			result.Height += StrokeThickness;
			result.Width += StrokeThickness;

			DesiredSize = result;
			return result;
		}
	}
}
