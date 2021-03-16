using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class EmptyCarouselGallery : ContentPage
	{
		public EmptyCarouselGallery()
		{
			InitializeComponent();
			BindingContext = new EmptyCarouselGalleryViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class EmptyCarouselGalleryViewModel : BindableObject
	{
		ObservableCollection<CarouselData> _items;

		public EmptyCarouselGalleryViewModel()
		{
			Items = new ObservableCollection<CarouselData>();
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

		public ICommand AddCommand => new Command(Add);

		public ICommand ClearCommand => new Command(Clear);

		void LoadItems()
		{
			var random = new Random();

			if (Device.RuntimePlatform == Device.iOS)
			{
				var items = new List<CarouselData>();

				for (int n = 0; n < 5; n++)
				{
					items.Add(new CarouselData
					{
						Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
						Name = $"{n + 1}"
					});
				}

				Items = new ObservableCollection<CarouselData>(items);
			}
			else
			{
				for (int n = 0; n < 5; n++)
				{
					Items.Add(new CarouselData
					{
						Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
						Name = $"{n + 1}"
					});
				}
			}
		}

		void Add()
		{
			LoadItems();
		}

		void Clear()
		{
			Items?.Clear();
		}
	}
}