namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSpanPage : ContentPage
	{
		private const int ItemWidth = 120;
		private const int ItemSpacing = 10;
		private const int MaxSpan = 8;
		private const int MinSpan = 2;
		private readonly GridItemsLayout _itemsLayout;

		public VerticalListSpanPage()
		{
			InitializeComponent();

			_collectionView.SizeChanged += OnSizeChanged;
			_itemsLayout = (GridItemsLayout)_collectionView.ItemsLayout;
		}

		private void OnSizeChanged(object sender, EventArgs e)
		{
			var bounds = _collectionView.Bounds;
			var span = (int)(1 + ((bounds.Width - ItemWidth) / (ItemWidth + ItemSpacing)));
			if (span < MinSpan)
				span = MinSpan;
			else if (span > MaxSpan)
				span = MaxSpan;

			if (_itemsLayout.Span != span)
				_itemsLayout.Span = span;
		}
	}
}

