using System;

namespace Xamarin.Forms
{
	public class ListItemsLayout : ItemsLayout
	{
		public ListItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		public static readonly IItemsLayout Vertical = new ListItemsLayout(ItemsLayoutOrientation.Vertical); 
		public static readonly IItemsLayout Horizontal = new ListItemsLayout(ItemsLayoutOrientation.Horizontal);

		// TODO hartez 2018/08/29 20:31:54 Need something like these previous two, but as a carousel default	

		public static readonly BindableProperty ItemSpacingProperty =
			BindableProperty.Create(nameof(ItemSpacing), typeof(double), typeof(ListItemsLayout), default(double),
				validateValue: (bindable, value) => (double)value >= 0);

		public double ItemSpacing
		{
			get => (double)GetValue(ItemSpacingProperty);
			set => SetValue(ItemSpacingProperty, value);
		}
	}
}