using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Maui.Xaml;

namespace System.Maui.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewLoadSimulateGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewLoadSimulateGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			Task.Run(async () =>
			{
				await Task.Delay(1000);
				Device.BeginInvokeOnMainThread(() => CollectionView.ItemsSource = new List<object>());
				await Task.Delay(1000);
				Device.BeginInvokeOnMainThread(() => CollectionView.ItemsSource = _demoFilteredItemSource.Items);
			});
		}
	}
}