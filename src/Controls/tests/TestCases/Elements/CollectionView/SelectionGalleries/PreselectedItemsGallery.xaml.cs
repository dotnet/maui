using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PreselectedItemsGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public PreselectedItemsGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			CollectionView.SelectedItems.Add(_demoFilteredItemSource.Items.Skip(2).First());
			CollectionView.SelectedItems.Add(_demoFilteredItemSource.Items.Skip(4).First());
			CollectionView.SelectedItems.Add(_demoFilteredItemSource.Items.Skip(5).First());

			CollectionView.SelectionMode = SelectionMode.Multiple;
		}
	}
}