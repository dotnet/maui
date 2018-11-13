namespace Xamarin.Forms
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
	}
}