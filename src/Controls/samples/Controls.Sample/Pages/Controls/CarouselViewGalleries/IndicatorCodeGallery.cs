using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Picker = Microsoft.Maui.Controls.Picker;
using Slider = Microsoft.Maui.Controls.Slider;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public class IndicatorCodeGallery : ContentPage
	{
		CarouselView _carouselView;
		Button _btnPrev;
		Button _btnNext;
		public IndicatorCodeGallery()
		{
			Title = "IndicatorView Gallery";

			//On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);

			var nItems = 10;

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				}
			};

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

			var itemTemplate = ExampleTemplates.CarouselTemplate();

			_carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				BackgroundColor = Colors.LightGray,
				AutomationId = "TheCarouselView"
			};

			layout.Children.Add(_carouselView);
			var generator = new ItemsSourceGenerator(_carouselView, nItems, ItemsSourceType.ObservableCollection);
			layout.Children.Add(generator);

			generator.GenerateItems();

			_carouselView.PropertyChanged += CarouselViewPropertyChanged;
			((ObservableCollection<CollectionViewGalleryTestItem>)_carouselView.ItemsSource).CollectionChanged += IndicatorCodeGalleryCollectionChanged;

			var indicatorView = new IndicatorView
			{
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(12, 6, 12, 12),
				IndicatorColor = Colors.Gray,
				SelectedIndicatorColor = Colors.Black,
				IndicatorsShape = IndicatorShape.Square,
				AutomationId = "TheIndicatorView",
				Count = 5,
			};

			_carouselView.IndicatorView = indicatorView;

			layout.Children.Add(indicatorView);

			var stckColors = new StackLayout { Orientation = StackOrientation.Horizontal };
			stckColors.Children.Add(new Label { VerticalOptions = LayoutOptions.Center, Text = "IndicatorColor" });

			var colors = new List<string>
   			{
				"Black",
				"Blue",
				"Red"
			};

			var colorsPicker = new Picker
			{
				ItemsSource = colors,
				WidthRequest = 150
			};
			colorsPicker.SelectedIndex = 0;

			colorsPicker.SelectedIndexChanged += (s, e) =>
			{
				var selectedIndex = colorsPicker.SelectedIndex;

				switch (selectedIndex)
				{
					case 0:
						indicatorView.IndicatorColor = Colors.Black;
						break;
					case 1:
						indicatorView.IndicatorColor = Colors.Blue;
						break;
					case 2:
						indicatorView.IndicatorColor = Colors.Red;
						break;
				}
			};

			stckColors.Children.Add(colorsPicker);

			layout.Children.Add(stckColors);

			var stckTemplate = new StackLayout { Orientation = StackOrientation.Horizontal };
			stckTemplate.Children.Add(new Label { VerticalOptions = LayoutOptions.Center, Text = "IndicatorTemplate" });

			var templates = new List<string>
			{
				"Circle",
				"Square",
				"Template"
			};

			var templatePicker = new Picker
			{
				ItemsSource = templates,
				WidthRequest = 150,
				TextColor = Colors.Black
			};

			templatePicker.SelectedIndexChanged += (s, e) =>
			{
				var selectedIndex = templatePicker.SelectedIndex;

				switch (selectedIndex)
				{
					case 0:
						indicatorView.IndicatorTemplate = null;
						indicatorView.IndicatorsShape = IndicatorShape.Circle;
						break;
					case 1:
						indicatorView.IndicatorTemplate = null;
						indicatorView.IndicatorsShape = IndicatorShape.Square;
						break;
					case 2:
						indicatorView.IndicatorTemplate = ExampleTemplates.IndicatorTemplate();
						break;
				}
			};

			templatePicker.SelectedIndex = 0;

			stckTemplate.Children.Add(templatePicker);

			layout.Children.Add(stckTemplate);

			var stckSize = new StackLayout { Orientation = StackOrientation.Horizontal };
			stckSize.Children.Add(new Label { VerticalOptions = LayoutOptions.Center, Text = "Indicator Size" });

			//indicatorView.IndicatorSize = 25;

			var sizeSlider = new Slider
			{
				WidthRequest = 150,
				Value = indicatorView.IndicatorSize,
				Maximum = 50,
			};

			sizeSlider.ValueChanged += (s, e) =>
			{
				var indicatorSize = sizeSlider.Value;
				indicatorView.IndicatorSize = indicatorSize;
			};

			stckSize.Children.Add(sizeSlider);

			layout.Children.Add(stckSize);

			Grid.SetRow(generator, 0);
			Grid.SetRow(stckColors, 1);
			Grid.SetRow(stckTemplate, 2);
			Grid.SetRow(stckSize, 3);
			Grid.SetRow(_carouselView, 4);
			Grid.SetRow(indicatorView, 6);

			var layoutBtn = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
			};

			var btnRemove = new Button
			{
				Text = "DEL First",
				FontSize = 8,
				AutomationId = "btnRemoveFirst",
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(5),
				Command = new Command(() =>
				{
					var items = (ObservableCollection<CollectionViewGalleryTestItem>)_carouselView.ItemsSource;
					items.Remove(items[0]);
				})
			};

			_btnPrev = new Button
			{
				Text = "Prev",
				FontSize = 8,
				AutomationId = "btnPrev",
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(5),
				Command = new Command(() =>
				{
					_carouselView.Position--;
				}, () =>
				{
					return _carouselView.Position > 0;
				})
			};

			_btnNext = new Button
			{
				Text = "Next",
				FontSize = 8,
				AutomationId = "btnNext",
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(5),
				Command = new Command(() =>
				{
					_carouselView.Position++;
				}, () =>
				{
					var items = (ObservableCollection<CollectionViewGalleryTestItem>)_carouselView.ItemsSource;
					return _carouselView.Position < items.Count - 1;
				})
			};

			var btnRemoveLast = new Button
			{
				Text = "DEL Last",
				FontSize = 8,
				AutomationId = "btnRemoveLast",
				BackgroundColor = Colors.LightGray,
				Padding = new Thickness(5),
				Command = new Command(() =>
				{
					var items = (ObservableCollection<CollectionViewGalleryTestItem>)_carouselView.ItemsSource;
					var indexToRemove = items.Count - 1;
					items.Remove(items[indexToRemove]);
				})
			};

			layoutBtn.Children.Add(btnRemove);
			layoutBtn.Children.Add(_btnPrev);
			layoutBtn.Children.Add(_btnNext);
			layoutBtn.Children.Add(btnRemoveLast);

			layout.Children.Add(layoutBtn);
			Grid.SetRow(layoutBtn, 5);
			Content = layout;
		}

		void IndicatorCodeGalleryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateButtons();
		}

		void CarouselViewPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateButtons();
		}

		void UpdateButtons()
		{
			(_btnPrev?.Command as Command)?.ChangeCanExecute();
			(_btnNext?.Command as Command)?.ChangeCanExecute();
		}
	}
}