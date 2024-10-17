namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
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
				Dispatcher.Dispatch(() => CollectionView.ItemsSource = new List<object>());
				await Task.Delay(1000);
				Dispatcher.Dispatch(() => CollectionView.ItemsSource = _demoFilteredItemSource.Items);
			});
		}
	}
}