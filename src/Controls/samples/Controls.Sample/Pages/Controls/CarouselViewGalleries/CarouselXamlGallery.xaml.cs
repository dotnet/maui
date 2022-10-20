using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CarouselXamlGallery : ContentPage
	{
		public CarouselXamlGallery(bool useLooping, int startCurrentItem = -1)
		{
			InitializeComponent();
			BindingContext = new CarouselViewModel(CarouselXamlSampleType.Peek, useLooping, startCurrentItem: startCurrentItem);
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
		bool _isLoop;
		int _count;
		int _position;
		ObservableCollection<CarouselItem> _items;
		CarouselXamlSampleType _type;
		public CarouselViewModel(CarouselXamlSampleType type, bool loop, int initialItems = 5, int startCurrentItem = -1)
		{
			IsLoop = loop;
			_type = type;

			var items = new List<CarouselItem>();
			for (int i = 0; i < initialItems; i++)
			{
				switch (_type)
				{
					case CarouselXamlSampleType.Peek:
						items.Add(new CarouselItem(i, "card_background.png"));
						break;
					default:
						items.Add(new CarouselItem(i));
						break;
				}
			}

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<ExampleTemplateCarousel>(this, "remove", (obj) => Items.Remove(obj.BindingContext as CarouselItem));
#pragma warning restore CS0618 // Type or member is obsolete

			Items = new ObservableCollection<CarouselItem>(items);
			Count = Items.Count - 1;

			if (startCurrentItem != -1)
				Selected = Items[startCurrentItem];
		}

		public bool IsLoop
		{
			get { return _isLoop; }
			set { SetProperty(ref _isLoop, value); }
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

	[Preserve(AllMembers = true)]
	public class ViewModelBase2 : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual ViewModelBase2 SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			field = value;
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
			return this;
		}
	}
}
