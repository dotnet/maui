using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class CollectionViewFeaturePage : NavigationPage
	{
		public CollectionViewFeaturePage()
		{
			PushAsync(new CollectionViewFeatureMainPage());
		}
	}

	public partial class CollectionViewFeatureMainPage : ContentPage
	{
		public CollectionViewFeatureMainPage()
		{
			InitializeComponent();
		}

		private async void OnEmptyViewButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new CollectionViewEmptyViewPage());
		}

		private async void OnHeaderFooterViewButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new CollectionViewHeaderPage());
		}
	}
}