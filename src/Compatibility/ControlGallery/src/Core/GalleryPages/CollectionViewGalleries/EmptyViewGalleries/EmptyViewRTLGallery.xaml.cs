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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
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
					EmptyViewRTLPage.FlowDirection = FlowDirection.LeftToRight;
					break;
				case 1:
					EmptyViewRTLPage.FlowDirection = FlowDirection.RightToLeft;
					break;
			}
		}
	}
}