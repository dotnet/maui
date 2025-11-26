#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.LinearItemsLayout']/Docs/*" />
	public class LinearItemsLayout : ItemsLayout
	{
		/// <param name="orientation">The orientation parameter.</param>
		public LinearItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='Vertical']/Docs/*" />
		public static readonly IItemsLayout Vertical = CreateVerticalDefault();
		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='Horizontal']/Docs/*" />
		public static readonly IItemsLayout Horizontal = CreateHorizontalDefault();

		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='CarouselVertical']/Docs/*" />
		public static readonly IItemsLayout CarouselVertical = CreateCarouselVerticalDefault();

		internal static readonly LinearItemsLayout CarouselDefault = CreateCarouselHorizontalDefault();

		/// <summary>Bindable property for <see cref="ItemSpacing"/>.</summary>
		public static readonly BindableProperty ItemSpacingProperty =
			BindableProperty.Create(nameof(ItemSpacing), typeof(double), typeof(LinearItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='ItemSpacing']/Docs/*" />
		public double ItemSpacing
		{
			get => (double)GetValue(ItemSpacingProperty);
			set => SetValue(ItemSpacingProperty, value);
		}

		internal static LinearItemsLayout CreateVerticalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Vertical);

		internal static LinearItemsLayout CreateHorizontalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

		internal static LinearItemsLayout CreateCarouselVerticalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

		internal static LinearItemsLayout CreateCarouselHorizontalDefault()
			=> new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
	}
}