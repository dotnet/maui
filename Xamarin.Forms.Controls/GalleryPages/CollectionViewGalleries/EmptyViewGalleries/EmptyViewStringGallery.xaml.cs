using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewStringGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewStringGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.TextChanged += SearchBarOnTextChanged;
		}

		void SearchBarOnTextChanged(object sender, TextChangedEventArgs e)
		{
			_demoFilteredItemSource.FilterItems(e.NewTextValue);
		}
	}
}