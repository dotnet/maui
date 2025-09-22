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
		public static readonly IItemsLayout Vertical = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='Horizontal']/Docs/*" />
		public static readonly IItemsLayout Horizontal = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

		/// <include file="../../../docs/Microsoft.Maui.Controls/LinearItemsLayout.xml" path="//Member[@MemberName='CarouselVertical']/Docs/*" />
		public static readonly IItemsLayout CarouselVertical = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
		{
			SnapPointsType = SnapPointsType.MandatorySingle,
			SnapPointsAlignment = SnapPointsAlignment.Center
		};

		internal static readonly LinearItemsLayout CarouselDefault = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
		{
			SnapPointsType = SnapPointsType.MandatorySingle,
			SnapPointsAlignment = SnapPointsAlignment.Center
		};

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
	}
}