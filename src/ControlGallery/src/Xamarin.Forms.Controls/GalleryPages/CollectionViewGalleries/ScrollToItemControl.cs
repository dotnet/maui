using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ScrollToItemControl : ContentView
	{
		readonly Picker _picker;
		ItemsView _itemsView;
		ScrollToPosition _currentScrollToPosition;
		Switch _animateSwitch;

		public ScrollToItemControl(ItemsView itemsView, bool showPositionSelector = true)
		{
			_itemsView = itemsView;

			var layout = new StackLayout { Margin = 3 };

			_itemsView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == ItemsView.ItemsSourceProperty.PropertyName)
				{
					var items = new List<object>();
					foreach (var item in itemsView.ItemsSource)
					{
						items.Add(item);
					}

					_picker.ItemsSource = items;
				}
			};

			var indexLabel = new Label { Text = "Scroll To Item: ", VerticalTextAlignment = TextAlignment.Center };
			_picker = new Picker { WidthRequest = 200 };
			var indexButton = new Button { Text = "Go" };

			indexButton.Clicked += ScrollTo;

			var row1 = new StackLayout { Orientation = StackOrientation.Horizontal };
			row1.Children.Add(indexLabel);
			row1.Children.Add(_picker);
			row1.Children.Add(indexButton);

			layout.Children.Add(row1);

			var animateLabel = new Label { Text = "Animate: ", VerticalTextAlignment = TextAlignment.Center };
			_animateSwitch = new Switch { IsToggled = true };

			var row2 = new StackLayout { Orientation = StackOrientation.Horizontal };
			row2.Children.Add(animateLabel);
			row2.Children.Add(_animateSwitch);

			layout.Children.Add(row2);

			if (showPositionSelector)
			{
				var row4 = new StackLayout { Orientation = StackOrientation.Horizontal };

				var scrollPositionSelector = new EnumSelector<ScrollToPosition>(() => _currentScrollToPosition,
					type => _currentScrollToPosition = type);
				row4.Children.Add(scrollPositionSelector);

				layout.Children.Add(row4);
			}

			Content = layout;
		}

		void ScrollToItem(object item)
		{
			_itemsView.ScrollTo(item, position: _currentScrollToPosition, animate: _animateSwitch.IsToggled);
		}

		void ScrollTo()
		{
			if (_picker.SelectedItem != null)
			{
				ScrollToItem(_picker.SelectedItem);
			}
		}

		void ScrollTo(object sender, EventArgs e)
		{
			ScrollTo();
		}
	}
}