using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HeaderFooterAddClearPage : ContentPage
	{
		private ObservableCollection<string> items = new ObservableCollection<string>();
		int itemNumber = 1;

		public HeaderFooterAddClearPage()
		{
			InitializeComponent();
			CollectionView.ItemsSource = items;
		}

		private void Add_click(object sender, EventArgs e)
		{
			for (int i = 0; i < 2; i++)
			{
				items.Add($"Item {itemNumber++}");
			}
		}

		private void Clear_click(object sender, EventArgs e)
		{
			items.Clear();
			itemNumber = 1;
		}
	}
}