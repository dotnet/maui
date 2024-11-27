using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	public partial class SelectionChangedCommandParameter : ContentPage
	{
		public SelectionChangedCommandParameter()
		{
			InitializeComponent();
			BindingContext = new ItemsViewModel(Result);
		}
	}

	class Item
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	class ItemsViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<Item> Items { get; set; }
		public Command LoadItemsCommand { get; set; }

		Item _selectedItem;
		readonly Label _result = default!;

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
			_result = result ?? throw new ArgumentNullException(nameof(result));
		}
	}
}