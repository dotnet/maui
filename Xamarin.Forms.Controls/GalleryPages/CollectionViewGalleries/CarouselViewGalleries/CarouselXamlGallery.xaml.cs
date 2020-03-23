using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CarouselXamlGallery : ContentPage
	{
		public CarouselXamlGallery()
		{
			InitializeComponent();
			BindingContext = new CarouselViewModel(CarouselXamlSampleType.Peek);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			//	(BindingContext as CarouselViewModel).Position = 2;
		}
	}

	[Preserve(AllMembers = true)]
	public enum CarouselXamlSampleType
	{
		Normal,
		Peek
	}

	[Preserve(AllMembers = true)]
	internal class CarouselViewModel : ViewModelBase2
	{
		int _count;
		int _position;
		ObservableCollection<CarouselItem> _items;
		CarouselXamlSampleType _type;
		public CarouselViewModel(CarouselXamlSampleType type, int initialItems = 5)
		{
			_type = type;

			var items = new List<CarouselItem>();
			for (int i = 0; i < initialItems; i++)
			{
				switch (_type)
				{
					case CarouselXamlSampleType.Peek:
						items.Add(new CarouselItem(i, "cardBackground"));
						break;
					default:
						items.Add(new CarouselItem(i));
						break;
				}
			}

			MessagingCenter.Subscribe<ExampleTemplateCarousel>(this, "remove", (obj) => Items.Remove(obj.BindingContext as CarouselItem));

			Items = new ObservableCollection<CarouselItem>(items);
			Count = Items.Count - 1;
		}

		public int Count
		{
			get { return _count; }
			set { SetProperty(ref _count, value); }
		}

		public int Position
		{
			get { return _position; }
			set { SetProperty(ref _position, value); }
		}

		public ObservableCollection<CarouselItem> Items
		{
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		CarouselItem _selected;
		public CarouselItem Selected
		{
			get { return _selected; }
			set { SetProperty(ref _selected, value); }
		}

		public ICommand RemoveCommand => new Command(() =>
		{
			Items.Remove(Selected);
			Count = Items.Count - 1;
		});

		public ICommand PreviousCommand => new Command(() =>
		{
			var indexCurrent = Items.IndexOf(Selected);
			if (indexCurrent > 0)
			{
				var newItem = Items[indexCurrent - 1];
				Selected = newItem;
			}
		});

		public ICommand NextCommand => new Command(() =>
		{
			var indexCurrent = Items.IndexOf(Selected);
			if (indexCurrent < Items.Count - 1)
			{
				var newItem = Items[indexCurrent + 1];
				Selected = newItem;
			}
		});
	}

	[Preserve(AllMembers = true)]
	internal class CarouselItem
	{
		public CarouselItem(int index, string image = null)
		{
			if (!string.IsNullOrEmpty(image))
				FeaturedImage = image;
			Index = index;
			Image = "https://placeimg.com/700/300/any";
		}

		public int Index { get; set; }

		public string Image { get; set; }

		public string FeaturedImage { get; set; }
	}
}
