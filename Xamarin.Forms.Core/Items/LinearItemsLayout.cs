using System;

namespace Xamarin.Forms
{
	public class LinearItemsLayout : ItemsLayout
	{
		public LinearItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		public static readonly IItemsLayout Vertical = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		public static readonly IItemsLayout Horizontal = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

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

		public static readonly BindableProperty ItemSpacingProperty =
			BindableProperty.Create(nameof(ItemSpacing), typeof(double), typeof(LinearItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		public double ItemSpacing
		{
			get => (double)GetValue(ItemSpacingProperty);
			set => SetValue(ItemSpacingProperty, value);
		}
	}
}