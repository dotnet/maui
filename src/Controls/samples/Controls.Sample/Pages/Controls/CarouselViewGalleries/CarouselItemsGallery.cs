﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CarouselItemsGallery : ContentPage
	{
		CarouselItemsGalleryViewModel _viewModel;
		bool _setPositionOnAppering;
		public CarouselItemsGallery(bool startEmptyCollection = false, bool setCollectionWithAsync = false,
									bool useNativeIndicators = false, bool setPositionOnConstructor = false,
									bool setPositionOnAppearing = false, bool useScrollAnimated = true)
		{
			_viewModel = new CarouselItemsGalleryViewModel(startEmptyCollection, setCollectionWithAsync);
			_setPositionOnAppering = setPositionOnAppearing;

			if (setPositionOnConstructor)
				_viewModel.CarouselPosition = 3;

			Title = $"CarouselView (Indicators)";

			var grid = new Grid
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
				SnapPointsAlignment = SnapPointsAlignment.Center,
				ItemSpacing = 8
			};

			var itemTemplate = GetCarouselTemplate();

			var carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				IsScrollAnimated = useScrollAnimated,
				IsBounceEnabled = true,
				EmptyView = "This is the empty view",
				PeekAreaInsets = new Thickness(50),
			};

			carouselView.SetBinding(CarouselView.ItemsSourceProperty, nameof(_viewModel.Items));
			carouselView.SetBinding(CarouselView.PositionProperty, nameof(_viewModel.CarouselPosition));

			var absolute = new Microsoft.Maui.Controls.AbsoluteLayout();
			AbsoluteLayout.SetLayoutBounds(carouselView, new Rect(0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags(carouselView, AbsoluteLayoutFlags.All);
			absolute.Add(carouselView);

			var indicators = new IndicatorView
			{
				Margin = new Thickness(15, 20),
				IndicatorColor = Colors.Gray,
				SelectedIndicatorColor = Colors.Black,
				IndicatorsShape = IndicatorShape.Square
			};

			if (!useNativeIndicators)
			{
				indicators.IndicatorTemplate = new DataTemplate(() =>
				{
					return new Image
					{
						Source = new FontImageSource
						{
							FontFamily = DefaultFontFamily(),
							Glyph = "\uf30c",
						},
					};
				});
			}

			carouselView.IndicatorView = indicators;

			AbsoluteLayout.SetLayoutBounds(indicators, new Rect(.5, 1, -1, -1));
			AbsoluteLayout.SetLayoutFlags(indicators, AbsoluteLayoutFlags.PositionProportional);
			absolute.Add(indicators);

			grid.Children.Add(absolute);

			var StackLayoutButtons = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var changePositionButton = new Button
			{
				Text = "Update Position"
			};

			changePositionButton.Clicked += (sender, e) =>
			{
				_viewModel.CarouselPosition = Random.Shared.Next(_viewModel.Items!.Count);
			};

			var addItemButton = new Button
			{
				Text = "Add Item"
			};

			addItemButton.Clicked += (sender, e) =>
			{
				_viewModel.Items!.Add(new CarouselData
				{
					Color = Colors.Red,
					Name = $"{_viewModel.Items.Count + 1}"
				});
				_viewModel.CarouselPosition = _viewModel.Items.Count - 1;
			};

			var removeItemButton = new Button
			{
				Text = "Remove Item"
			};

			removeItemButton.Clicked += (sender, e) =>
			{
				if (_viewModel.Items!.Any())
					_viewModel.Items!.RemoveAt(_viewModel.Items.Count - 1);

				if (_viewModel.Items!.Count > 0)
					_viewModel.CarouselPosition = _viewModel.Items.Count - 1;
			};

			var clearItemsButton = new Button
			{
				Text = "Clear Items"
			};

			clearItemsButton.Clicked += (sender, e) =>
			{
				_viewModel.Items!.Clear();
			};

			var lbl = new Label
			{
				AutomationId = "lblPosition"
			};
			lbl.SetBinding(Label.TextProperty, nameof(CarouselView.Position));
			lbl.BindingContext = carouselView;

			StackLayoutButtons.Children.Add(changePositionButton);
			StackLayoutButtons.Children.Add(addItemButton);
			StackLayoutButtons.Children.Add(removeItemButton);
			StackLayoutButtons.Children.Add(clearItemsButton);
			StackLayoutButtons.Children.Add(lbl);

			Grid.SetRow(StackLayoutButtons, 1);
			grid.Children.Add(StackLayoutButtons);

			Content = grid;
			BindingContext = _viewModel;
		}

		protected override void OnAppearing()
		{
			if (_viewModel.CarouselPosition != 3 && _setPositionOnAppering)
				_viewModel.CarouselPosition = 3;

			base.OnAppearing();
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

				var frame = new Border
				{
					Content = grid
				};

				frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

				return frame;
			});
		}

		static string DefaultFontFamily()
		{
			var fontFamily = "";
			if (DeviceInfo.Platform == DevicePlatform.iOS)
				fontFamily = "Ionicons";
			else if (DeviceInfo.Platform == DevicePlatform.WinUI)
				fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
			else
				fontFamily = "fonts/ionicons.ttf#";
			return fontFamily;
		}
	}

	[Preserve(AllMembers = true)]
	public class CarouselItemsGalleryViewModel : BindableObject
	{
		ObservableCollection<CarouselData>? _items;
		int _carouselPosition;

		public CarouselItemsGalleryViewModel(bool empty, bool async)
		{
			if (async)
			{
				Task.Run(async () =>
				{
					await Task.Delay(400);
					SetSource(empty);
				});
			}
			else
			{
				SetSource(empty);
			}
		}

		readonly Random _random = new Random();

		void SetSource(bool empty)
		{

			var source = new List<CarouselData>();
			if (!empty)
			{
				for (int n = 0; n < 5; n++)
				{
					source.Add(GetItem(n));
				}
			}
			Items = new ObservableCollection<CarouselData>(source);
		}

		public CarouselData GetItem(int currentCount)
		{
			return new CarouselData
			{
				Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
				Name = $"{currentCount + 1}"
			};
		}

		public ObservableCollection<CarouselData>? Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		public int CarouselPosition
		{
			get => _carouselPosition;
			set
			{
				_carouselPosition = value;
				OnPropertyChanged(nameof(CarouselPosition));
			}
		}
	}
}
