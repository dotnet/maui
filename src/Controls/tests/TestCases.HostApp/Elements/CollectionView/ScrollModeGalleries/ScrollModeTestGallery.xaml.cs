namespace Maui.Controls.Sample.CollectionViewGalleries.ScrollModeGalleries
{
	public partial class ScrollModeTestGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(20);
		CollectionView _collectionView;

		public ScrollModeTestGallery(IItemsLayout itemsLayout = null, Func<DataTemplate> dataTemplate = null, Func<CollectionView> createCollectionView = null)
		{
			InitializeComponent();

			_collectionView = createCollectionView == null ? new CollectionView() : createCollectionView();
			_collectionView.ItemsLayout = itemsLayout ?? LinearItemsLayout.Vertical;

			var scrollModeSelector = new EnumSelector<ItemsUpdatingScrollMode>(() => _collectionView.ItemsUpdatingScrollMode,
			mode => _collectionView.ItemsUpdatingScrollMode = mode, "SelectScrollMode");

			Grid.Children.Add(scrollModeSelector);


			Grid.Children.Add(_collectionView);
			Grid.SetRow((BindableObject)_collectionView, 5);

			_collectionView.ItemTemplate = dataTemplate == null ? ExampleTemplates.PhotoTemplate() : dataTemplate();
			_collectionView.ItemsSource = _demoFilteredItemSource.Items;
		}

		void ScrollToMiddle_Clicked(object sender, EventArgs e)
		{
			_collectionView.ScrollTo(_demoFilteredItemSource.Items.Count / 2, position: ScrollToPosition.Start, animate: false);
		}

		void AddItemAbove_Clicked(object sender, EventArgs e)
		{
			var index = (_demoFilteredItemSource.Items.Count / 2) - 1;

			_demoFilteredItemSource.Items.Insert(index,
				new CollectionViewGalleryTestItem(DateTime.Now,
				"Inserted item",
				"coffee.png",
				index));
		}

		void AddItemBelow_Clicked(object sender, EventArgs e)
		{
			var index = (_demoFilteredItemSource.Items.Count / 2) + 2;

			_demoFilteredItemSource.Items.Insert(index,
				new CollectionViewGalleryTestItem(DateTime.Now,
				"Inserted item",
				"coffee.png",
				index));
		}

		void AddItemToEnd_Clicked(object sender, EventArgs e)
		{
			_demoFilteredItemSource.Items.Add(
				new CollectionViewGalleryTestItem(DateTime.Now,
				"Added item",
				"coffee.png",
				_demoFilteredItemSource.Items.Count));
		}
	}
}