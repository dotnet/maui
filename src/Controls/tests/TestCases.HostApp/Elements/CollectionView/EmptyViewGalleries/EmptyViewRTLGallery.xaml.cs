namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	public partial class EmptyViewRTLGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewRTLGallery()
		{
			InitializeComponent();

			Picker.SelectedIndex = 0;

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.SearchCommand = new Command(() => _demoFilteredItemSource.FilterItems(SearchBar.Text));
		}

		void OnPickerSelectedIndexChanged(object sender, EventArgs e)
		{
			switch (Picker.SelectedIndex)
			{
				default:
				case 0:
					EmptyViewRTLPage.FlowDirection = Microsoft.Maui.FlowDirection.LeftToRight;
					break;
				case 1:
					EmptyViewRTLPage.FlowDirection = Microsoft.Maui.FlowDirection.RightToLeft;
					break;
			}
		}
	}
}