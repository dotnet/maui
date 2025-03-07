using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class EmptyViewMainPage : NavigationPage
	{
		public EmptyViewMainPage()
		{
			PushAsync(new EmptyViewContentPage());
		}
	}

	public partial class EmptyViewContentPage : ContentPage
	{
		public EmptyViewContentPage()
		{
			InitializeComponent();
		}

		private async void OnEmptyViewButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new CollectionViewControlPage());
		}
	}
}