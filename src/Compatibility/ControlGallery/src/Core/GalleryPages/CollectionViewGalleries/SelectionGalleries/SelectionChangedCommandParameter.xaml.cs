using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectionChangedCommandParameter : ContentPage
	{
		public SelectionChangedCommandParameter()
		{
			InitializeComponent();
			BindingContext = new ItemsViewModel(Result);
		}
	}

	[Preserve(AllMembers = true)]
	class Item
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	[Preserve(AllMembers = true)]
	class ItemsViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<Item> Items { get; set; }
		public Command LoadItemsCommand { get; set; }

		Item _selectedItem;
		readonly Label _result;

		public event PropertyChangedEventHandler PropertyChanged;

		public Item SelectedItem
		{
			get => _selectedItem;
			set { _selectedItem = value; OnPropertyChanged(); }
		}

		public Command<Item> SelectionChangedCommand { get; }

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ItemsViewModel(Label result)
		{
			Items = new ObservableCollection<Item>();

			for (int n = 0; n < 10; n++)
			{
				Items.Add(new Item { Id = n.ToString(), Text = $"Item {n}", Description = $"This is item {n}" });
			}

			SelectionChangedCommand = new Command<Item>(item =>
			{
				var fromParameter = item;
				var fromSelectedItem = SelectedItem;

				if (fromParameter != fromSelectedItem)
				{
					_result.Text = "Fail";
				}
				else
				{
					_result.Text = "Success";
				}
			});
			_result = result;
		}
	}
}