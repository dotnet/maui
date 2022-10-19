#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Content))]
	public class Border : View, IContentView, IBorderView, IPaddingElement
	{
		float[]? _strokeDashPattern;
		ReadOnlyCollection<Element>? _logicalChildren;

		internal ObservableCollection<Element> InternalChildren { get; } = new();

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(Border), null, propertyChanged: ContentChanged);

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		public View? Content
		{
			get { return (View?)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		public static readonly BindableProperty StrokeShapeProperty =
			BindableProperty.Create(nameof(StrokeShape), typeof(IShape), typeof(Border), new Rectangle());

		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Border), null);

		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Border), 1.0, propertyChanged: StrokeThicknessChanged);

		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Border), null,
				defaultValueCreator: bindable => new DoubleCollection());

		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Border), 0.0);

		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Border), PenLineCap.Flat);

		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Border), PenLineJoin.Miter);

		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Border), 10.0);

		[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
		public IShape? StrokeShape
		{
			set { SetValue(StrokeShapeProperty, value); }
			get { return (IShape?)GetValue(StrokeShapeProperty); }
		}

		public Brush? Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush?)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public DoubleCollection? StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection?)GetValue(StrokeDashArrayProperty); }
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

		IShape? IBorderStroke.Shape => StrokeShape;

		Paint? IStroke.Stroke => Stroke;

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

		public float[]? StrokeDashPattern
		{
			get
			{
				if (StrokeDashArray is INotifyCollectionChanged oldCollection)
					oldCollection.CollectionChanged -= OnStrokeDashArrayChanged;

				_strokeDashPattern = StrokeDashArray?.ToFloatArray();

				if (StrokeDashArray is INotifyCollectionChanged newCollection)
					newCollection.CollectionChanged += OnStrokeDashArrayChanged;

				return _strokeDashPattern;
			}
		}

		float IStroke.StrokeDashOffset => (float)StrokeDashOffset;

		float IStroke.StrokeMiterLimit => (float)StrokeMiterLimit;


		object? IContentView.Content => Content;

		IView? IContentView.PresentedContent => Content;

		public Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			bounds = bounds.Inset(StrokeThickness);
			this.ArrangeContent(bounds);
			return bounds.Size;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding + StrokeThickness;
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}

		public static void ContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Border border)
			{
				if (oldValue is Element oldElement)
				{
					int index = border.InternalChildren.IndexOf(oldElement);
					if (border.InternalChildren.Remove(oldElement))
					{
						border.OnChildRemoved(oldElement, index);
					}
				}

				if (newValue is Element newElement)
				{
					border.InternalChildren.Add(newElement);
					border.OnChildAdded(newElement);
				}
			}

			((IBorderView)bindable).InvalidateMeasure();
		}

		public static void StrokeThicknessChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IBorderView)bindable).InvalidateMeasure();
		}

		public void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			(this as IBorderView).InvalidateMeasure();
		}

		public Thickness PaddingDefaultValueCreator()
		{
			return Thickness.Zero;
		}

		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == HeightProperty.PropertyName ||
				propertyName == WidthProperty.PropertyName ||
				propertyName == StrokeShapeProperty.PropertyName)
				Handler?.UpdateValue(nameof(IBorderStroke.Shape));
		}

		void OnStrokeDashArrayChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
		}
	}
}