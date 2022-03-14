namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.GridItemsLayout']/Docs" />
	public class GridItemsLayout : ItemsLayout
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='SpanProperty']/Docs" />
		public static readonly BindableProperty SpanProperty =
			BindableProperty.Create(nameof(Span), typeof(int), typeof(GridItemsLayout), 1,
				validateValue: (bindable, value) => (int)value >= 1);

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='Span']/Docs" />
		public int Span
		{
			get => (int)GetValue(SpanProperty);
			set => SetValue(SpanProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public GridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public GridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) :
			base(orientation)
		{
			Span = span;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='VerticalItemSpacingProperty']/Docs" />
		public static readonly BindableProperty VerticalItemSpacingProperty =
			BindableProperty.Create(nameof(VerticalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='VerticalItemSpacing']/Docs" />
		public double VerticalItemSpacing
		{
			get => (double)GetValue(VerticalItemSpacingProperty);
			set => SetValue(VerticalItemSpacingProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='HorizontalItemSpacingProperty']/Docs" />
		public static readonly BindableProperty HorizontalItemSpacingProperty =
			BindableProperty.Create(nameof(HorizontalItemSpacing), typeof(double), typeof(GridItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <include file="../../../docs/Microsoft.Maui.Controls/GridItemsLayout.xml" path="//Member[@MemberName='HorizontalItemSpacing']/Docs" />
		public double HorizontalItemSpacing
		{
			get => (double)GetValue(HorizontalItemSpacingProperty);
			set => SetValue(HorizontalItemSpacingProperty, value);
		}
	}
}
