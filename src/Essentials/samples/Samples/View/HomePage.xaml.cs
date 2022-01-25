using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Samples.Model;

namespace Samples.View
{
	public partial class HomePage : BasePage
	{
		public HomePage()
		{
			InitializeComponent();
		}

		async void OnSampleTapped(object sender, SelectionChangedEventArgs e)
		{
			var item = e.CurrentSelection?.FirstOrDefault() as SampleItem;
			if (item == null)
				return;

			await Navigation.PushAsync((Page)Activator.CreateInstance(item.PageType));

			// deselect Item
			((CollectionView)sender).SelectedItem = null;
		}
	}
}
