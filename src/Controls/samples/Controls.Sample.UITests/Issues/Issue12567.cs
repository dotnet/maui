using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 12567, "Carousel View does not behave properly in Windows", PlatformAffected.UWP)]
	public class Issue12567 : TestContentPage
	{
		protected override void Init()
		{
			var carouselItemsCount = 10;
			var viewModel = new CarouselViewModel();

			viewModel.GenerateItems(carouselItemsCount);

			BindingContext = viewModel;

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};
			var itemTemplate = GetCarouselItemTemplate();
			var carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				Margin = new Thickness(0, 10, 0, 10),
				BackgroundColor = Colors.Grey,
				AutomationId = "TestCarouselView",
				PeekAreaInsets = new Thickness(30, 0, 30, 0)
			};
			var button = new Button
			{
				Text = "Get Last Item",
				AutomationId = "GetLast",
				Margin = new Thickness(0, 10, 0, 10)
			};
			var carouselContent = new Grid();

			carouselContent.Children.Add(carouselView);

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.Fill,
			};

			button.Clicked += (sender, args) =>
			{
				carouselView.ScrollTo(carouselItemsCount - 1, position: ScrollToPosition.End, animate: true);
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");
			layout.Children.Add(button);
			layout.Children.Add(carouselContent);

			Content = layout;
		}

		DataTemplate GetCarouselItemTemplate()
		{
			return new DataTemplate(() =>
			{
				var layout = new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					VerticalOptions = LayoutOptions.Fill
				};
				var nameLabel = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10),
					AutomationId = "NameLabel"
				};
				var numberLabel = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10),
					AutomationId = "NumberLabel"
				};

				nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
				numberLabel.SetBinding(Label.TextProperty, new Binding("Number"));

				layout.Children.Add(nameLabel);
				layout.Children.Add(numberLabel);

				return new Frame
				{
					Content = layout,
					HasShadow = false
				};
			});
		}
	}

	class CarouselData
	{
		public string Name { get; set; }

		public int Number { get; set; }
	}

	class CarouselViewModel : BindableObject
	{
		ObservableCollection<CarouselData> items;

		public ObservableCollection<CarouselData> Items
		{
			get => items;
			set
			{
				items = value;
				OnPropertyChanged();
			}
		}

		public void GenerateItems(int itemsCount)
		{
			Items = new ObservableCollection<CarouselData>();

			for (var i = 1; i <= itemsCount; i++)
			{
				Items.Add(new CarouselData
				{
					Name = $"Test Item {i}",
					Number = i
				});
			}
		}
	}
}
