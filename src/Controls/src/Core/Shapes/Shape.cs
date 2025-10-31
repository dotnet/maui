using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Shape']/Docs/*" />
	public abstract partial class Shape : View, IShapeView, IShape
	{
		WeakBrushChangedProxy? _fillProxy = null;
		WeakBrushChangedProxy? _strokeProxy = null;
		EventHandler? _fillChanged, _strokeChanged;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Shape.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Shape()
		{
		}

		~Shape()
		{
			_fillProxy?.Unsubscribe();
			_strokeProxy?.Unsubscribe();
		}

		public abstract PathF GetPath();

		double _fallbackWidth;
		double _fallbackHeight;

		/// <summary>Bindable property for <see cref="Fill"/>.</summary>
		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(Shape), null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
						(bindable as Shape)?.StopNotifyingFillChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
						(bindable as Shape)?.NotifyFillChanges();
				});

		/// <summary>Bindable property for <see cref="Stroke"/>.</summary>
		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Shape), null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
						(bindable as Shape)?.StopNotifyingStrokeChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
						(bindable as Shape)?.NotifyStrokeChanges();
				});

		/// <summary>Bindable property for <see cref="StrokeThickness"/>.</summary>
		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Shape), 1.0);

		/// <summary>Bindable property for <see cref="StrokeDashArray"/>.</summary>
		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape), null,
				defaultValueCreator: bindable => new DoubleCollection());

		/// <summary>Bindable property for <see cref="StrokeDashOffset"/>.</summary>
		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Shape), 0.0);

		/// <summary>Bindable property for <see cref="StrokeLineCap"/>.</summary>
		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Shape), PenLineCap.Flat);

		/// <summary>Bindable property for <see cref="StrokeLineJoin"/>.</summary>
		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Shape), PenLineJoin.Miter);

		/// <summary>Bindable property for <see cref="StrokeMiterLimit"/>.</summary>
		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Shape), 10.0);

		/// <summary>Bindable property for <see cref="Aspect"/>.</summary>
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

		void NotifyFillChanges()
		{
			var fill = Fill;

			if (fill is ImmutableBrush)
				return;

			if (fill is not null)
			{
				SetInheritedBindingContext(fill, BindingContext);
				_fillChanged ??= (sender, e) => OnPropertyChanged(nameof(Fill));
				_fillProxy ??= new();
				_fillProxy.Subscribe(fill, _fillChanged);

				OnParentResourcesChanged(this.GetMergedResources());
				((IElementDefinition)this).AddResourcesChangedListener(fill.OnParentResourcesChanged);
			}
		}

		void StopNotifyingFillChanges()
		{
			var fill = Fill;

			if (fill is ImmutableBrush)
				return;

			if (fill is not null)
			{
				((IElementDefinition)this).RemoveResourcesChangedListener(fill.OnParentResourcesChanged);

				SetInheritedBindingContext(fill, null);
				_fillProxy?.Unsubscribe();
			}
		}

		void NotifyStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				SetInheritedBindingContext(stroke, BindingContext);
				_strokeChanged ??= (sender, e) => OnPropertyChanged(nameof(Stroke));
				_strokeProxy ??= new();
				_strokeProxy.Subscribe(stroke, _strokeChanged);

				OnParentResourcesChanged(this.GetMergedResources());
				((IElementDefinition)this).AddResourcesChangedListener(stroke.OnParentResourcesChanged);
			}
		}

		void StopNotifyingStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				((IElementDefinition)this).RemoveResourcesChangedListener(stroke.OnParentResourcesChanged);

				SetInheritedBindingContext(stroke, null);
				_strokeProxy?.Unsubscribe();
			}
		}

		PathF IShape.PathForBounds(Graphics.Rect viewBounds)
		{
			_fallbackHeight = viewBounds.Height;
			_fallbackWidth = viewBounds.Width;

			var path = GetPath();

			TransformPathForBounds(path, viewBounds);

			return path;
		}

		internal void TransformPathForBounds(PathF path, Graphics.Rect viewBounds)
		{
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

				float widthScale = float.IsNaN(calculatedWidth) || float.IsInfinity(calculatedWidth) ? 0 : calculatedWidth;
				float heightScale = float.IsNaN(calculatedHeight) || float.IsInfinity(calculatedHeight) ? 0 : calculatedHeight;

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
		}

		protected override void OnBindingContextChanged()
		{
			PropagateBindingContextToBrush();

			base.OnBindingContextChanged();
		}

		void PropagateBindingContextToBrush()
		{
			if (Fill is not null)
				SetInheritedBindingContext(Fill, BindingContext);

			if (Stroke is not null)
				SetInheritedBindingContext(Stroke, BindingContext);
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
			scaleX = double.IsNaN(scaleX) || double.IsInfinity(scaleX) ? 0 : scaleX;
			scaleY = double.IsNaN(scaleY) || double.IsInfinity(scaleY) ? 0 : scaleY;

			switch (Aspect)
			{
				case Stretch.None:
					result.Height += pathBounds.Y;
					result.Width += pathBounds.X;
					break;

				case Stretch.Fill:
					if (!double.IsInfinity(heightConstraint) || HeightRequest > 0)
					{
						result.Height = HeightRequest < 0 ? heightConstraint : HeightRequest;
					}

					if (!double.IsInfinity(widthConstraint) || WidthRequest > 0)
					{
						result.Width = WidthRequest < 0 ? widthConstraint : WidthRequest;
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

			return result;
		}

		internal virtual double WidthForPathComputation
		{
			get
			{
				var width = Width;

				// If the shape has never been arranged, then Width won't actually have a value;
				// use the fallback value instead.
				return width == -1 ? _fallbackWidth : width;
			}
		}

		internal virtual double HeightForPathComputation
		{
			get
			{
				var height = Height;

				// If the shape has never been arranged, then Height won't actually have a value;
				// use the fallback value instead.
				return height == -1 ? _fallbackHeight : height;
			}
		}
	}
}