#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WSetter = Microsoft.UI.Xaml.Setter;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.Controls.Platform
{
	public static class CollectionViewExtensions
	{
		public static WStyle? GetItemContainerStyle(this LinearItemsLayout? layout)
		{
			if (layout is null)
				return null;

			var h = layout?.ItemSpacing ?? 0;
			var v = layout?.ItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var style = new WStyle(typeof(GridViewItem));

			if (layout?.Orientation == ItemsLayoutOrientation.Vertical)
			{
				style.Setters.Add(new WSetter(FrameworkElement.MinHeightProperty, 0));
				style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			}
			else if (layout?.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				style.Setters.Add(new WSetter(FrameworkElement.MinWidthProperty, 0));
				style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
			}

			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));

			return style;
		}

		public static WStyle? GetItemContainerStyle(this GridItemsLayout? layout)
		{
			if (layout is null)
				return null;

			var h = layout?.HorizontalItemSpacing ?? 0;
			var v = layout?.VerticalItemSpacing ?? 0;
			var margin = WinUIHelpers.CreateThickness(h, v, h, v);

			var style = new WStyle(typeof(GridViewItem));

			if (layout?.Orientation == ItemsLayoutOrientation.Vertical)
			{
				style.Setters.Add(new WSetter(FrameworkElement.MinHeightProperty, 0));
				style.Setters.Add(new WSetter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
			}
			else if (layout?.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				style.Setters.Add(new WSetter(FrameworkElement.MinWidthProperty, 0));
				style.Setters.Add(new WSetter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
			}

			style.Setters.Add(new WSetter(FrameworkElement.MarginProperty, margin));
			style.Setters.Add(new WSetter(Control.PaddingProperty, WinUIHelpers.CreateThickness(0)));

			return style;
		}

		public static Orientation ToPlatform(this ItemsLayout layout)
		{
			return layout.Orientation == ItemsLayoutOrientation.Horizontal
				? Orientation.Horizontal
				: Orientation.Vertical;
		}
	}
}
