//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FilterSelection : ContentPage
	{
		DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public FilterSelection()
		{
			InitializeComponent();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.SearchCommand = new Command(() =>
			{
				_demoFilteredItemSource.FilterItems(SearchBar.Text);
			});

			ResetButton.Clicked += ResetButtonClicked;
		}

		void ResetButtonClicked(object sender, EventArgs e)
		{
			_demoFilteredItemSource = new DemoFilteredItemSource(new Random().Next(3, 50));
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
		}
	}
}