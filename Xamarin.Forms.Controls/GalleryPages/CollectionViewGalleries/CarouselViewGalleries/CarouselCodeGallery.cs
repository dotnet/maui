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
				Position = 2,
				//NumberOfSideItems = 1,
				Margin = new Thickness(0,10,0,40),
				BackgroundColor = Color.LightGray,
				AutomationId = "TheCarouselView"
			};

			if (orientation == ItemsLayoutOrientation.Horizontal)
				carouselView.PeekAreaInsets = new Thickness(30, 0, 30, 0);
			else
				carouselView.PeekAreaInsets = new Thickness(0, 30, 0, 30);

			carouselView.Scrolled += CarouselView_Scrolled;

			layout.Children.Add(carouselView);

			StackLayout stacklayoutInfo = GetReadOnlyInfo(carouselView);

			var generator = new ItemsSourceGenerator(carouselView, initialItems: nItems, itemsSourceType: ItemsSourceType.ObservableCollection);

			layout.Children.Add(generator);

			var positionControl = new PositionControl(carouselView, nItems);
			layout.Children.Add(positionControl);

			var spacingModifier = new SpacingModifier(carouselView.ItemsLayout, "Update Spacing");

			layout.Children.Add(spacingModifier);

			layout.Children.Add(stacklayoutInfo);

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

			padi.ValueChanged += (s, e) => {
				var peek = padi.Value;

				if (orientation == ItemsLayoutOrientation.Horizontal)
					carouselView.PeekAreaInsets = new Thickness(peek, 0, peek, 0);
				else
					carouselView.PeekAreaInsets = new Thickness(0, peek, 0, peek);
			};

			stckPeek.Children.Add(padi);
			stacklayoutInfo.Children.Add(stckPeek);
			stacklayoutInfo.Children.Add(_scrollInfoLabel);

			Grid.SetRow(positionControl, 1);
			Grid.SetRow(stacklayoutInfo, 2);
			Grid.SetRow(spacingModifier, 3);
			Grid.SetRow(carouselView, 4);

			Content = layout;
			generator.CollectionChanged += (sender, e) => {
				positionControl.UpdatePositionCount(generator.Count);
			};

			generator.GenerateItems();
		}

		private void CarouselView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
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
	}
}