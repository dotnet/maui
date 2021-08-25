using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement : IView
	{
		Semantics _semantics;

		public Rectangle Frame
		{
			get => Bounds;
			set
			{
				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}

		new public IViewHandler Handler
		{
			get => base.Handler as IViewHandler;
			set => base.Handler = value;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			IsPlatformEnabled = Handler != null;
		}

		Paint IView.Background
		{
			get
			{
				if (!Brush.IsNullOrEmpty(Background))
					return Background;
				if (BackgroundColor.IsNotDefault())
					return new SolidColorBrush(BackgroundColor);
				return null;
			}
		}

		IShape IView.Clip => Clip;

		IView IView.Parent => Parent as IView;

		Paint IBorder.BorderBrush
		{
			get
			{
				if (!Brush.IsNullOrEmpty(BorderBrush))
					return BorderBrush;

				return null;
			}
		}

		public static readonly BindableProperty BorderBrushProperty = BindableProperty.Create(
			nameof(BorderBrush), typeof(Brush), typeof(VisualElement), null);

		[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
		public Brush BorderBrush
		{
			get => (Brush)GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
			nameof(BorderWidth), typeof(double), typeof(VisualElement), 0.0d);

		public double BorderWidth
		{
			get => (double)GetValue(BorderWidthProperty);
			set => SetValue(BorderWidthProperty, value);
		}

		double[] IBorder.BorderDashArray
		{
			get
			{
				if (BorderDashArray != null)
					return BorderDashArray.ToArray();

				return null;
			}
		}

		public static readonly BindableProperty BorderDashArrayProperty = BindableProperty.Create(
			nameof(BorderDashArray), typeof(DoubleCollection), typeof(VisualElement), null,
				defaultValueCreator: bindable => new DoubleCollection());

		public DoubleCollection BorderDashArray
		{
			get => (DoubleCollection)GetValue(BorderDashArrayProperty);
			set => SetValue(BorderDashArrayProperty, value);
		}

		public static readonly BindableProperty BorderDashOffsetProperty = BindableProperty.Create(
			nameof(BorderDashOffset), typeof(double), typeof(VisualElement), 0.0d);

		public double BorderDashOffset
		{
			get => (double)GetValue(BorderDashOffsetProperty);
			set => SetValue(BorderDashOffsetProperty, value);
		}

		public static readonly BindableProperty BorderShapeProperty = BindableProperty.Create(
		   nameof(BorderShape), typeof(IShape), typeof(VisualElement), null);

		[System.ComponentModel.TypeConverter(typeof(Shapes.BorderShapeTypeConverter))]
		public IShape BorderShape
		{
			get => (IShape)GetValue(BorderShapeProperty);
			set => SetValue(BorderShapeProperty, value);
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			Handler?.UpdateValue(propertyName);
		}

		public Size DesiredSize { get; protected set; }

		public void Arrange(Rectangle bounds)
		{
			Layout(bounds);
		}

		Size IView.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., ScrollView) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);
			return Frame.Size;
		}

		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		void IView.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride() => Handler?.Invoke(nameof(IView.InvalidateMeasure));

		void IView.InvalidateArrange()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// MeasureOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
		// the interface has to be explicitly implemented to avoid conflict with the old Measure method
		protected virtual Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		Maui.FlowDirection IView.FlowDirection => FlowDirection.ToPlatformFlowDirection();
		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => default;

		Visibility IView.Visibility => IsVisible.ToVisibility();

		Semantics IView.Semantics
		{
			get => _semantics;
		}

		// We don't want to initialize Semantics until someone explicitly 
		// wants to modify some aspect of the semantics class
		internal Semantics SetupSemantics() =>
			_semantics ??= new Semantics();

		double IView.Width => WidthRequest;
		double IView.Height => HeightRequest;

		Thickness IView.Margin => Thickness.Zero;
	}
}