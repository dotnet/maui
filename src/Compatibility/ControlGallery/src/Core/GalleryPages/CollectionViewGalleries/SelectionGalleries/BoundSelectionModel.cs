using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[Preserve(AllMembers = true)]
	internal class BoundSelectionModel : INotifyPropertyChanged
	{
		private CollectionViewGalleryTestItem _selectedItem;
		private ObservableCollection<CollectionViewGalleryTestItem> _items;
		private ObservableCollection<object> _selectedItems;

		public event PropertyChangedEventHandler PropertyChanged;

		public BoundSelectionModel()
		{
			Items = new ObservableCollection<CollectionViewGalleryTestItem>();

			for (int n = 0; n < 4; n++)
			{
				Items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n), $"Item {n}", "coffee.png", n));
			}

			SelectedItem = Items[2];

			SelectedItems = new ObservableCollection<object>()
			{
				Items[1], Items[2]
			};
		}

		private void SelectedItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(SelectedItemsText));
		}

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public CollectionViewGalleryTestItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<object> SelectedItems
		{
			get => _selectedItems;
			set
			{
				if (_selectedItems != null)
				{
					_selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;
				}

				_selectedItems = value;

				_selectedItems.CollectionChanged += SelectedItemsCollectionChanged;

				OnPropertyChanged();
				OnPropertyChanged(nameof(SelectedItemsText));
			}
		}

		public ObservableCollection<CollectionViewGalleryTestItem> Items
		{
			get => _items;
			set { _items = value; OnPropertyChanged(); }
		}

		public string SelectedItemsText => SelectedItems.ToCommaSeparatedList();
	}
}