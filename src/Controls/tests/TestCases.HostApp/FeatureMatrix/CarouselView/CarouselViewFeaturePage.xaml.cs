using System;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public class CarouselViewFeaturePage : NavigationPage
{
	public CarouselViewFeaturePage()
	{
		PushAsync(new CarouselViewFeatureMainPage());
	}
}

public partial class CarouselViewFeatureMainPage : ContentPage
{
	public CarouselViewFeatureMainPage()
	{
		InitializeComponent();
	}


	private async void OnCarouselViewClicked(object sender, System.EventArgs e)
	{
		await Navigation.PushAsync(new CarouselViewControlPage());
	}

}