using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
	public partial class CarouselViewCoreGalleryPage : ContentPage
	{
		public CarouselViewCoreGalleryPage()
		{
			InitializeComponent();

			bool useLooping = true;
			int startCurrentItem = 3;

			BindingContext = new CarouselViewModel(useLooping, startCurrentItem: startCurrentItem);
		}
	}

	internal class CarouselViewModel : BindableObject
	{
		bool _isLoop;
		int _count;
		int _position;
		ObservableCollection<CarouselItem> _items;

		public CarouselViewModel(bool loop, int initialItems = 5, int startCurrentItem = -1)
		{
			IsLoop = loop;

			var items = new List<CarouselItem>();
			for (int i = 0; i < initialItems; i++)
			{
				items.Add(new CarouselItem(i));
			}

			Items = new ObservableCollection<CarouselItem>(items);
			Count = Items.Count - 1;

			if (startCurrentItem != -1)
				Selected = Items[startCurrentItem];
		}

		public bool IsLoop
		{
			get { return _isLoop; }
			set
			{
				_isLoop = value;
				OnPropertyChanged();
			}
		}

		public int Count
		{
			get { return _count; }
			set
			{
				_count = value;
				OnPropertyChanged();
			}
		}

		public int Position
		{
			get { return _position; }
			set
			{
				_position = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<CarouselItem> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		CarouselItem _selected;
		public CarouselItem Selected
		{
			get { return _selected; }
			set
			{
				_selected = value;
				OnPropertyChanged();
			}
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
			if (indexCurrent == 0)
			{
				var newItem = Items[Items.Count - 1];
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
			if (indexCurrent == Items.Count - 1)
			{
				var newItem = Items[0];
				Selected = newItem;
			}
		});
	}

	internal class CarouselItem
	{
		public CarouselItem(int index, string image = null)
		{
			Index = index;

			if (string.IsNullOrEmpty(image))
				Image = "oasis.jpg";
			else
				Image = image;
		}

		public int Index { get; set; }

		public string Title => $"CarouselItem{Index}";

		public string Image { get; set; }
	}
}