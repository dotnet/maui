using System.Collections.Generic;
using System.Threading;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewLoadSimulateGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewLoadSimulateGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			new Thread(() =>
			{
				Thread.Sleep(1000);
				Device.BeginInvokeOnMainThread(() => CollectionView.ItemsSource = new List<object>());
				Thread.Sleep(1000);
				Device.BeginInvokeOnMainThread(() => CollectionView.ItemsSource = _demoFilteredItemSource.Items);
			}).Start();
		}
	}
}