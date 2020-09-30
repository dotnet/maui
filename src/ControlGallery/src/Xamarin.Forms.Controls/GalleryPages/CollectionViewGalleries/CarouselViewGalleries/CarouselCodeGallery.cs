using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SpacingGalleries;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	internal class CarouselCodeGallery : ContentPage
	{
		readonly Label _scrollInfoLabel = new Label();
		readonly ItemsLayoutOrientation _orientation;

		public CarouselCodeGallery(ItemsLayoutOrientation orientation)
		{
			On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);

			_scrollInfoLabel.MaxLines = 1;
			_scrollInfoLabel.LineBreakMode = LineBreakMode.TailTruncation;
			_orientation = orientation;

			Title = $"CarouselView (Code, {orientation})";

			var nItems = 5;
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};
			var itemsLayout =
			new LinearItemsLayout(orientation)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

			var itemTemplate = ExampleTemplates.CarouselTemplate();

			var carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				Margin = new Thickness(0, 10, 0, 10),
				BackgroundColor = Color.Red,
				AutomationId = "TheCarouselView",
				//Loop = false
			};

			if (orientation == ItemsLayoutOrientation.Horizontal)
				carouselView.PeekAreaInsets = new Thickness(30, 0, 30, 0);
			else
				carouselView.PeekAreaInsets = new Thickness(0, 30, 0, 30);

			carouselView.Scrolled += CarouselViewScrolled;

			StackLayout stacklayoutInfo = GetReadOnlyInfo(carouselView);

			var generator = new ItemsSourceGenerator(carouselView, initialItems: nItems, itemsSourceType: ItemsSourceType.ObservableCollection);

			var positionControl = new PositionControl(carouselView, nItems);

			var spacingModifier = new SpacingModifier(carouselView.ItemsLayout, "Update Spacing");

			var stckPeek = new StackLayout { Orientation = StackOrientation.Horizontal };
			stckPeek.Children.Add(new Label { Text = "Peek" });
			var padi = new Slider
			{
				Maximum = 100,
				Minimum = 0,
				Value = 30,
				WidthRequest = 100,
				BackgroundColor = Color.Pink
			};

			padi.ValueChanged += (s, e) =>
			{
				var peek = padi.Value;

				if (orientation == ItemsLayoutOrientation.Horizontal)
					carouselView.PeekAreaInsets = new Thickness(peek, 0, peek, 0);
				else
					carouselView.PeekAreaInsets = new Thickness(0, peek, 0, peek);
			};

			stckPeek.Children.Add(padi);

			var content = new Grid();
			content.Children.Add(carouselView);

#if DEBUG
			// Uncomment this line to add a helper to visualize the center of each element.
			//content.Children.Add(CreateDebuggerLines());
#endif
			layout.Children.Add(generator);
			layout.Children.Add(positionControl);
			layout.Children.Add(stacklayoutInfo);
			layout.Children.Add(stckPeek);
			layout.Children.Add(spacingModifier);
			layout.Children.Add(_scrollInfoLabel);
			layout.Children.Add(content);

			Grid.SetRow(positionControl, 1);
			Grid.SetRow(stacklayoutInfo, 2);
			Grid.SetRow(stckPeek, 3);
			Grid.SetRow(spacingModifier, 4);
			Grid.SetRow(_scrollInfoLabel, 5);
			Grid.SetRow(content, 6);

			Content = layout;
			generator.CollectionChanged += (sender, e) =>
			{
				positionControl.UpdatePositionCount(generator.Count);
			};

			generator.GenerateItems();
			positionControl.UpdatePosition(1);
		}

		void CarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			_scrollInfoLabel.Text = $"First item: {e.FirstVisibleItemIndex}, Last item: {e.LastVisibleItemIndex}";

			double delta;
			double offset;

			if (_orientation == ItemsLayoutOrientation.Horizontal)
			{
				delta = e.HorizontalDelta;
				offset = e.HorizontalOffset;
			}
			else
			{
				delta = e.VerticalDelta;
				offset = e.VerticalOffset;
			}

			_scrollInfoLabel.Text += $", Delta: {delta}, Offset: {offset}";
		}

		static StackLayout GetReadOnlyInfo(CarouselView carouselView)
		{
			var stacklayoutInfo = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				BindingContext = carouselView
			};
			var labelDragging = new Label { Text = nameof(carouselView.IsDragging) };
			var switchDragging = new Switch();

			switchDragging.SetBinding(Switch.IsToggledProperty, nameof(carouselView.IsDragging), BindingMode.OneWay);
			stacklayoutInfo.Children.Add(labelDragging);
			stacklayoutInfo.Children.Add(switchDragging);

			return new StackLayout { Children = { stacklayoutInfo } };
		}
#if DEBUG
		Grid CreateDebuggerLines()
		{
			var grid = new Grid
			{
				InputTransparent = true,
				Margin = new Thickness(0, 10, 0, 10)
			};

			var horizontalLine = new Grid
			{
				HeightRequest = 1,
				BackgroundColor = Color.Red,
				VerticalOptions = LayoutOptions.Center
			};

			grid.Children.Add(horizontalLine);

			var verticalLine = new Grid
			{
				WidthRequest = 1,
				BackgroundColor = Color.Red,
				HorizontalOptions = LayoutOptions.Center
			};

			grid.Children.Add(verticalLine);

			return grid;
		}
#endif
	}
}