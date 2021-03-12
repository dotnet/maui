using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.IndicatorView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8693, "[Bug] IndicatorView template resets when data updates", PlatformAffected.All)]
	public class Issue8693 : TestContentPage
	{
		public Issue8693()
		{
			Title = "Issue 8693";
			BindingContext = new Issue8693ViewModel();
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Margin = new Thickness(6),
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Press the Button to update the ItemsSource of the CarouselView below. After updating, verify that the IndicatorView is still visible. If it is visible, the test has passed."
			};

			var updateButton = new Button
			{
				Text = "Update ItemsSource"
			};

			var itemsLayout =
				new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				};

			var carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = GetCarouselTemplate()
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

			var indicatorView = new IndicatorView
			{
				IndicatorColor = Color.Red,
				SelectedIndicatorColor = Color.Green,
				IndicatorTemplate = GetIndicatorTemplate(),
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 0, 0, 24)
			};

			carouselView.IndicatorView = indicatorView;

			layout.Children.Add(instructions);
			layout.Children.Add(updateButton);
			layout.Children.Add(carouselView);
			layout.Children.Add(indicatorView);

			Content = layout;

			updateButton.Clicked += (sender, args) =>
			{
				var vm = (Issue8693ViewModel)BindingContext;
				vm.LoadItems();
			};
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

		internal DataTemplate GetIndicatorTemplate()
		{
			return new DataTemplate(() =>
			{
				var grid = new Grid
				{
					HeightRequest = 6,
					WidthRequest = 50
				};

				return grid;
			});
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8693Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8693ViewModel : BindableObject
	{
		ObservableCollection<Issue8693Model> _items;

		public Issue8693ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue8693Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public void LoadItems()
		{
			Items = new ObservableCollection<Issue8693Model>();

			var random = new Random();
			var items = new List<Issue8693Model>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new Issue8693Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}

			_items = new ObservableCollection<Issue8693Model>(items);
			OnPropertyChanged(nameof(Items));
		}
	}
}