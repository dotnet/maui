namespace Microsoft.Maui.Controls
{
	public class GridItemsLayout : ItemsLayout
	{
		public static readonly BindableProperty SpanProperty =
			BindableProperty.Create(nameof(Span), typeof(int), typeof(GridItemsLayout), 1,
				validateValue: (bindable, value) => (int)value >= 1);

		public int Span
		{
			get => (int)GetValue(SpanProperty);
			set => SetValue(SpanProperty, value);
		}

		public GridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		public GridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) :
			base(orientation)
		{
			Span = span;
		}

		public static readonly BindableProperty VerticalItemSpacingProperty =
			BindableProperty.Create(nameof(VerticalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		public double VerticalItemSpacing
		{
			get => (double)GetValue(VerticalItemSpacingProperty);
			set => SetValue(VerticalItemSpacingProperty, value);
		}

		public static readonly BindableProperty HorizontalItemSpacingProperty =
			BindableProperty.Create(nameof(HorizontalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		public double HorizontalItemSpacing
		{
			get => (double)GetValue(HorizontalItemSpacingProperty);
			set => SetValue(HorizontalItemSpacingProperty, value);
		}
	}
}