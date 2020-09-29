using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewViewGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewViewGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.SearchCommand = new Command(() => _demoFilteredItemSource.FilterItems(SearchBar.Text));
		}
	}
}