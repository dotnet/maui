#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// An items layout that arranges items in a grid with configurable columns or rows.
	/// </summary>
	/// <remarks>
	/// <see cref="GridItemsLayout"/> displays items in a grid format, with the number of columns (for vertical scrolling) 
	/// or rows (for horizontal scrolling) determined by the <see cref="Span"/> property.
	/// Use <see cref="HorizontalItemSpacing"/> and <see cref="VerticalItemSpacing"/> to control the spacing between items.
	/// This layout is commonly used in <see cref="CollectionView"/> for displaying items in a multi-column or multi-row format.
	/// </remarks>
	public class GridItemsLayout : ItemsLayout
	{
		/// <summary>Bindable property for <see cref="Span"/>.</summary>
		public static readonly BindableProperty SpanProperty =
			BindableProperty.Create(nameof(Span), typeof(int), typeof(GridItemsLayout), 1,
				validateValue: (bindable, value) => (int)value >= 1);

		/// <summary>
		/// Gets or sets the number of columns (for vertical orientation) or rows (for horizontal orientation) in the grid.
		/// </summary>
		/// <value>The number of columns or rows. Must be 1 or greater. The default is 1.</value>
		/// <remarks>
		/// For vertical scrolling grids, <see cref="Span"/> determines the number of columns.
		/// For horizontal scrolling grids, <see cref="Span"/> determines the number of rows.
		/// </remarks>
		public int Span
		{
			get => (int)GetValue(SpanProperty);
			set => SetValue(SpanProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GridItemsLayout"/> class with the specified orientation.
		/// </summary>
		/// <param name="orientation">The scroll orientation of the grid.</param>
		public GridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GridItemsLayout"/> class with the specified span and orientation.
		/// </summary>
		/// <param name="span">The number of columns (for vertical orientation) or rows (for horizontal orientation).</param>
		/// <param name="orientation">The scroll orientation of the grid.</param>
		public GridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) :
			base(orientation)
		{
			Span = span;
		}

		/// <summary>Bindable property for <see cref="VerticalItemSpacing"/>.</summary>
		public static readonly BindableProperty VerticalItemSpacingProperty =
			BindableProperty.Create(nameof(VerticalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <summary>
		/// Gets or sets the vertical spacing between items in the grid.
		/// </summary>
		/// <value>The vertical spacing in platform-specific units. Must be 0 or greater. The default is 0.</value>
		public double VerticalItemSpacing
		{
			get => (double)GetValue(VerticalItemSpacingProperty);
			set => SetValue(VerticalItemSpacingProperty, value);
		}

		/// <summary>Bindable property for <see cref="HorizontalItemSpacing"/>.</summary>
		public static readonly BindableProperty HorizontalItemSpacingProperty =
			BindableProperty.Create(nameof(HorizontalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <summary>
		/// Gets or sets the horizontal spacing between items in the grid.
		/// </summary>
		/// <value>The horizontal spacing in platform-specific units. Must be 0 or greater. The default is 0.</value>
		public double HorizontalItemSpacing
		{
			get => (double)GetValue(HorizontalItemSpacingProperty);
			set => SetValue(HorizontalItemSpacingProperty, value);
		}
	}
}
