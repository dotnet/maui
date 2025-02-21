using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSModalPagePresentationStyle : ContentPage
	{
		public iOSModalPagePresentationStyle()
		{
			InitializeComponent();
		}

		async void OnReturnButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}
