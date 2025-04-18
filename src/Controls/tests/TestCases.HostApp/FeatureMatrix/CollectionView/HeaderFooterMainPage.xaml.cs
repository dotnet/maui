using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class HeaderFooterMainPage : NavigationPage
	{
		public HeaderFooterMainPage()
		{
			PushAsync(new HeaderFooterContentPage());
		}
	}

	public partial class HeaderFooterContentPage : ContentPage
	{
		public HeaderFooterContentPage()
		{
			InitializeComponent();
		}

		private async void OnHeaderFooterViewButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new CollectionViewControlPage());
		}
	}
}