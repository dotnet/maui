using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Controls.Sample.UITests.Elements
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewTemplateGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();
		readonly EmptyViewGalleryFilterInfo _emptyViewGalleryFilterInfo = new EmptyViewGalleryFilterInfo();

		public EmptyViewTemplateGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
			CollectionView.EmptyView = _emptyViewGalleryFilterInfo;

			SearchBar.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(SearchBar.Text))
				{
					_demoFilteredItemSource.FilterItems(SearchBar.Text);
				}
			};

			SearchBar.SearchCommand = new Command(() =>
			{
				_demoFilteredItemSource.FilterItems(SearchBar.Text);
				_emptyViewGalleryFilterInfo.Filter = SearchBar.Text;
			});
		}
	}
}