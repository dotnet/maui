using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public class RefreshViewGallery : ContentPage
	{
		public RefreshViewGallery()
		{
			Title = "RefreshView Gallery";

			var button = new Button
			{
				Text = "Enable CarouselView",
				AutomationId = "EnableCarouselView"
			};
			button.Clicked += ButtonClicked;

			Content = new StackLayout
			{
				Children =
				{
					button,
					GalleryBuilder.NavButton("Refresh Layout Gallery", () => new RefreshLayoutGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh ScrollView Gallery", () => new RefreshScrollViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh ListView Gallery", () => new RefreshListViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh CollectionView Gallery", () => new RefreshCollectionViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh CarouselView Gallery", () => new RefreshCarouselViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh WebView Gallery", () => new RefreshWebViewGallery(), Navigation),
					GalleryBuilder.NavButton("IsEnabled RefreshView Gallery", () => new IsEnabledRefreshViewGallery(), Navigation)
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "CarouselView Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.CarouselViewExperimental });
		}
	}

	[Preserve(AllMembers = true)]
	public class RefreshItem
	{
		public string Name { get; set; }
		public Color Color { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class RefreshViewModel : BindableObject
	{
		const int RefreshDuration = 2;

		readonly Random _random;
		bool _isRefresing;
		ObservableCollection<RefreshItem> _items;
		
		public RefreshViewModel(bool initialRefresh = false)
		{
			_random = new Random();
			Items = new ObservableCollection<RefreshItem>();

			if (initialRefresh)
				ExecuteRefresh();
			else
				LoadItems();
		}

		public bool IsRefreshing
		{
			get { return _isRefresing; }
			set
			{
				_isRefresing = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<RefreshItem> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand RefreshCommand => new Command(ExecuteRefresh);

		void LoadItems()
		{
			for (int i = 0; i < 50; i++)
			{
				Items.Insert(i, new RefreshItem
				{
					Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					Name = DateTime.Now.AddMinutes(i).ToString("F")
				});
			}
		}

		void ExecuteRefresh()
		{
			IsRefreshing = true;

			Device.StartTimer(TimeSpan.FromSeconds(RefreshDuration), () =>
			{
				LoadItems();

				IsRefreshing = false;

				return false;
			});
		}
	}
}