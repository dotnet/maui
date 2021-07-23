using System;
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

		async void OnSampleTapped(object sender, ItemTappedEventArgs e)
		{
			var item = e.Item as SampleItem;
			if (item == null)
				return;

			await Navigation.PushAsync((Page)Activator.CreateInstance(item.PageType));

			// deselect Item
			((ListView)sender).SelectedItem = null;
		}
	}
}
