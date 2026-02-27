using System.Collections.ObjectModel;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalListAddItemsPage : ContentPage
	{
		private ObservableCollection<string> items = new();
		public HorizontalListAddItemsPage()
		{
			InitializeComponent();

			this.items.Add("item: " + this.items.Count);
			this.cv1.ItemsSource = this.items;
		}

		private void ButtonAdd_Clicked(object sender, System.EventArgs e)
		{
			double collectionViewHeight = this.cv1.DesiredSize.Height;
			this.items.Add("item: " + this.items.Count);
		}
	}
}

