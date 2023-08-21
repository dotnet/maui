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

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.AlternateLayoutGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StaggeredLayout : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public StaggeredLayout()
		{
			InitializeComponent();

			CV.ItemTemplate = ExampleTemplates.RandomSizeTemplate();
			CV.ItemsSource = _demoFilteredItemSource.Items;
		}
	}

	public class StaggeredCollectionView : CollectionView { }

	public class StaggeredGridItemsLayout : GridItemsLayout
	{
		public StaggeredGridItemsLayout([Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
		{
		}

		public StaggeredGridItemsLayout(int span, [Parameter("Orientation")] ItemsLayoutOrientation orientation) : base(span, orientation)
		{
		}
	}
}