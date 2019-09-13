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

		// TODO hartez 2018/08/29 20:31:54 Need something like these previous two, but as a carousel default	

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