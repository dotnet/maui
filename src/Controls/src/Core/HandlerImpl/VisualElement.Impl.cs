using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement : IFrameworkElement
	{
		Semantics _semantics;

		public Rectangle Frame => Bounds;

		new public IViewHandler Handler
		{
			get => base.Handler as IViewHandler;
			set => base.Handler = value;
		}

		private protected override void OnHandlerSet()
		{
			base.OnHandlerSet();

			IsPlatformEnabled = Handler != null;
		}

		Paint IFrameworkElement.Background
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

		IShape IFrameworkElement.Clip => Clip;

		IFrameworkElement IFrameworkElement.Parent => Parent as IFrameworkElement;

		public static readonly BindableProperty BorderBrushProperty = BindableProperty.Create(
			nameof(BorderBrush), typeof(Paint), typeof(VisualElement), null);

		public Paint BorderBrush
		{
			get => (Paint)GetValue(BorderBrushProperty);
			set => SetValue(BorderBrushProperty, value);
		}

		public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
			nameof(BorderWidth), typeof(double), typeof(VisualElement), 0.0d);

		public double BorderWidth
		{
			get => (double)GetValue(BorderWidthProperty);
			set => SetValue(BorderWidthProperty, value);
		}

		public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
		   nameof(CornerRadius), typeof(CornerRadius), typeof(VisualElement), new CornerRadius());

		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
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

		Size IFrameworkElement.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., Layout) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rectangle bounds)
		{
			Bounds = this.ComputeFrame(bounds);
			return Frame.Size;
		}

		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		void IFrameworkElement.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride() => Handler?.UpdateValue(nameof(IFrameworkElement.InvalidateMeasure));

		void IFrameworkElement.InvalidateArrange()
		{
		}

		Size IFrameworkElement.Measure(double widthConstraint, double heightConstraint)
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

		Maui.FlowDirection IFrameworkElement.FlowDirection => FlowDirection.ToPlatformFlowDirection();
		Primitives.LayoutAlignment IFrameworkElement.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IFrameworkElement.VerticalLayoutAlignment => default;

		Visibility IFrameworkElement.Visibility => IsVisible.ToVisibility();

		Semantics IFrameworkElement.Semantics
		{
			get => _semantics;
		}

		// We don't want to initialize Semantics until someone explicitly 
		// wants to modify some aspect of the semantics class
		internal Semantics SetupSemantics() =>
			_semantics ??= new Semantics();

		double IFrameworkElement.Width => WidthRequest;
		double IFrameworkElement.Height => HeightRequest;
	}
}