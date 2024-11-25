using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	public partial class SelectionSynchronization : ContentPage
	{
		public SelectionSynchronization()
		{
			InitializeComponent();
			BindingContext = new SelectionSyncModel();
		}

		void SwitchSourceClicked(object sender, EventArgs e)
		{
			var newSource = new List<string> { "Item -1", "Item 0", "Item 1", "Item 3", "Item 4", "Item 5" };
			CVSwitchSource.ItemsSource = newSource;
		}
	}

	public class SelectionSyncModel
	{
		public SelectionSyncModel()
		{
			Items = new List<string>() {
				"Item 1", "Item 2", "Item 3", "Item 4"
			};

			SelectedItem = "Item 2";
			SelectedItems = new ObservableCollection<object> { "Item 3", "Item 2" };

			SelectedItemNotInSource = "Foo";
			SelectedItemsNotInSource = new ObservableCollection<object> { "Foo", "Bar", "Baz" };
		}

		public List<string> Items { get; set; }

		public string SelectedItem { get; set; }
		public ObservableCollection<object> SelectedItems { get; set; }

		public string SelectedItemNotInSource { get; set; }
		public ObservableCollection<object> SelectedItemsNotInSource { get; set; }
	}
}