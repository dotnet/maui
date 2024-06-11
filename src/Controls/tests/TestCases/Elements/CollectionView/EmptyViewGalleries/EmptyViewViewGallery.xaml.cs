using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewViewGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public EmptyViewViewGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.PropertyChanged += (s, e) =>
			 {
				 if (e.PropertyName == nameof(SearchBar.Text))
				 {
					 if (string.IsNullOrEmpty(SearchBar.Text))
					 {
						 _demoFilteredItemSource.FilterItems(SearchBar.Text);
					 }
				 }
			 };
			SearchBar.SearchCommand = new Command(() => _demoFilteredItemSource.FilterItems(SearchBar.Text));
		}
	}
}