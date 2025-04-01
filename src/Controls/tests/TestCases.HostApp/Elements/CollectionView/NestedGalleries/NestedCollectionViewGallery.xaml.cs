namespace Maui.Controls.Sample.CollectionViewGalleries.NestedGalleries
{
	public partial class NestedCollectionViewGallery : ContentPage
	{
		public NestedCollectionViewGallery()
		{
			InitializeComponent();
			BindingContext = new NestedCollectionViewModel();
		}
	}


	internal class NestedItemSource
	{
		public List<CollectionViewGalleryTestItem> Items { get; set; }
		public string Title { get; set; }

		public NestedItemSource(string title)
		{
			Items = new List<CollectionViewGalleryTestItem>();

			int count = new Random().Next(6, 15);

			var source = new DemoFilteredItemSource(count);

			for (int n = 0; n < count; n++)
			{
				Items.Add(source.Items[n]);
			}

			Title = title;
		}
	}


	internal class NestedCollectionViewModel
	{
		public List<NestedItemSource> Items { get; set; }

		public NestedCollectionViewModel()
		{
			Items = new List<NestedItemSource>();

			for (int n = 0; n < 20; n++)
			{
				Items.Add(new NestedItemSource($"Source {n}"));
			}
		}
	}
}