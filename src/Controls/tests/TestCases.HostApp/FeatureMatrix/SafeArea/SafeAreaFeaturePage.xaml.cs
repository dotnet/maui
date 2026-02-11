using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class SafeAreaFeaturePage : NavigationPage
{
	public SafeAreaFeaturePage()
	{
		PushAsync(new SafeAreaFeatureMainPage());
	}
}

public partial class SafeAreaFeatureMainPage : ContentPage
{
	private SafeAreaViewModel _viewModel;

	public SafeAreaFeatureMainPage()
	{
		InitializeComponent();
		_viewModel = new SafeAreaViewModel();
	}

	private async void OnContentPageSafeAreaButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new SafeAreaContentPage(_viewModel));
	}
}
