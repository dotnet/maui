using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7678, "[iOS] CarouselView binded to a ObservableCollection and add Items later, crash", PlatformAffected.iOS)]
	public class Issue7678Ios : TestContentPage
	{
		public Issue7678Ios()
		{
#if APP
			Title = "Issue 7678";
			BindingContext = new Issue7678IosViewModel();
#endif
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Margin = new Thickness(6),
				Text = "The CarouselView below should render 5 items."
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

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);

			Content = layout;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await ((Issue7678IosViewModel)BindingContext).LoadItemsAsync();
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
	public class Issue7678IosModel
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7678IosViewModel : BindableObject
	{
		ObservableCollection<Issue7678IosModel> _items;

		public ObservableCollection<Issue7678IosModel> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public async Task LoadItemsAsync()
		{
			Items = new ObservableCollection<Issue7678IosModel>();

			await Task.Delay(500);

			var random = new Random();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new Issue7678IosModel
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}
		}
	}


#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 7678, "[Android] CarouselView binded to a new ObservableCollection filled with Items does not render content", PlatformAffected.Android)]
	public class Issue7678Droid : TestContentPage
	{
		public Issue7678Droid()
		{
#if APP
			Title = "Issue 7678";
			BindingContext = new Issue7678DroidViewModel();
#endif
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Margin = new Thickness(6),
				Text = "The CarouselView below must show items. If you do not see any item, this test has failed."
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

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);

			Content = layout;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await ((Issue7678DroidViewModel)BindingContext).LoadItemsAsync();
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
	public class Issue7678DroidModel
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7678DroidViewModel : BindableObject
	{
		ObservableCollection<Issue7678DroidModel> _items;

		public ObservableCollection<Issue7678DroidModel> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public async Task LoadItemsAsync()
		{
			Items = new ObservableCollection<Issue7678DroidModel>();

			var random = new Random();
			var items = new List<Issue7678DroidModel>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new Issue7678DroidModel
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}

			await Task.Delay(500);

			_items = new ObservableCollection<Issue7678DroidModel>(items);
			OnPropertyChanged(nameof(Items));
		}
	}
}