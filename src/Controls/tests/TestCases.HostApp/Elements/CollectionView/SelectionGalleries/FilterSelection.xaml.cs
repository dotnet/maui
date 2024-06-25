using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
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