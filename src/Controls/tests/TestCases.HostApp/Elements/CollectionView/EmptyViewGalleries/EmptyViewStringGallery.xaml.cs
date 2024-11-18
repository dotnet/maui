namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	public partial class EmptyViewStringGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewStringGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(SearchBar.Text))
				{
					if (string.IsNullOrEmpty(SearchBar.Text))
					{
						_demoFilteredItemSource.FilterItems(SearchBar.Text);
					}
				}
			};

			SearchBar.SearchCommand = new Command(() => _demoFilteredItemSource.FilterItems(SearchBar.Text));
		}
	}
}