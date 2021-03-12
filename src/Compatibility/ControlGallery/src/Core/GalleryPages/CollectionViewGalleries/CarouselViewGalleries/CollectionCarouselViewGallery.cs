using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CollectionCarouselViewGallery : ContentPage
	{
		public CollectionCarouselViewGallery()
		{
			Title = "Working with ObservableCollections and CarouselView";

			BindingContext = new CollectionCarouselViewGalleryViewModel();

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			var itemsLayout =
				new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				};

			var itemTemplate = GetCarouselTemplate();

			var carouselView = new CarouselView
			{
				HeightRequest = 300,
				BackgroundColor = Color.Pink,
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				IsScrollAnimated = true,
				IsBounceEnabled = true,
				AutomationId = "TheCarouselView",
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var scroll = new ScrollView();
			var stack = new StackLayout();
			scroll.Content = stack;

			var lblPosition = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				BindingContext = carouselView,
				AutomationId = "lblPosition"
			};
			var lblCenterIndex = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				BindingContext = carouselView,
				AutomationId = "lblCenterIndex"
			};

			lblPosition.SetBinding(Label.TextProperty, nameof(CarouselView.Position));

			carouselView.Scrolled += (s, e) =>
			{
				lblCenterIndex.Text = $"First {e.FirstVisibleItemIndex} CenterIndex: {e.CenterItemIndex} Last: {e.LastVisibleItemIndex}";
			};

			var clearButton = new Button
			{
				Text = "Clear"
			};

			clearButton.SetBinding(Button.CommandProperty, "ClearCommand");

			var newObservableButton = new Button
			{
				Text = "Set new ObservableCollection",
				AutomationId = "btnNewObservable"
			};

			newObservableButton.SetBinding(Button.CommandProperty, "NewObservableCommand");

			var addObservableButton = new Button
			{
				Text = "Add new Items to ObservableCollection",
				AutomationId = "btnAddObservable"
			};

			addObservableButton.SetBinding(Button.CommandProperty, "NewItemsObservableCommand");

			var threadObservableButton = new Button
			{
				Text = "Use ObservableCollection (Threads)"
			};

			threadObservableButton.SetBinding(Button.CommandProperty, "TheadCommand");

			stack.Children.Add(lblPosition);
			stack.Children.Add(lblCenterIndex);
			stack.Children.Add(clearButton);
			stack.Children.Add(newObservableButton);
			stack.Children.Add(addObservableButton);
			stack.Children.Add(threadObservableButton);

			layout.Children.Add(carouselView, 0, 0);
			layout.Children.Add(scroll, 0, 1);

			Content = layout;
		}

		internal DataTemplate GetCarouselTemplate()
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid();

				var info = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(6)
				};

				info.SetBinding(Label.TextProperty, new Binding("Name"));

				grid.Children.Add(info);

				var frame = new Frame
				{
					Content = grid,
					HasShadow = false
				};

				frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

				return frame;
			});
		}
	}

	[Preserve(AllMembers = true)]
	public class CollectionCarouselViewGalleryViewModel : BindableObject
	{
		readonly Random _random;
		ObservableCollection<CarouselData> _items;

		public CollectionCarouselViewGalleryViewModel()
		{
			_random = new Random();
		}

		public ObservableCollection<CarouselData> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand ClearCommand => new Command(Clear);
		public ICommand NewObservableCommand => new Command(NewObservable);
		public ICommand NewItemsObservableCommand => new Command(NewItemsObservable);
		public ICommand TheadCommand => new Command(async () => await NewItemsThread());

		void Clear()
		{
			Items?.Clear();
		}

		void NewObservable()
		{
			Clear();

			var items = new List<CarouselData>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new CarouselData
				{
					Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					Name = $"NewObservable {n + 1}"
				});
			}

			Items = new ObservableCollection<CarouselData>(items);
		}

		void NewItemsObservable()
		{
			Clear();

			Items = new ObservableCollection<CarouselData>();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new CarouselData
				{
					Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					Name = $"NewItemsObservable {n + 1}"
				});
			}
		}

		async Task NewItemsThread()
		{
			Clear();

			await Task.Delay(500);

			Device.BeginInvokeOnMainThread(() =>
		 {
			 Items = new ObservableCollection<CarouselData>();

			 for (int n = 0; n < 5; n++)
			 {
				 Items.Add(new CarouselData
				 {
					 Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					 Name = $"Thead {n + 1}"
				 });
			 }
		 });
		}
	}
}
