﻿namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
{
	public partial class HeaderFooterGrid : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(6); // Reduced the items count to ensure that the footer is displayed on all platforms.

		object header;
		object footer;

		public HeaderFooterGrid()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
		}

		void AddContentClicked(object sender, System.EventArgs e)
		{
			if (sender is VisualElement ve && ve.Parent is StackLayout sl)
				sl.Children.Add(new Label() { Text = "Grow" });
		}

		void ToggleHeader(object sender, System.EventArgs e)
		{
			header = CollectionView.Header ?? header;

			if (CollectionView.Header == null)
				CollectionView.Header = header;
			else
				CollectionView.Header = null;
		}

		void ToggleFooter(object sender, System.EventArgs e)
		{
			footer = CollectionView.Footer ?? footer;

			if (CollectionView.Footer == null)
				CollectionView.Footer = footer;
			else
				CollectionView.Footer = null;
		}
	}
}