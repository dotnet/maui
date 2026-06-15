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

	private void OnContentPageSafeAreaButtonClicked(object sender, EventArgs e)
	{
		Application.Current.Windows[0].Page = new SafeAreaContentPage(_viewModel);
	}

	private void OnBorderSafeAreaButtonClicked(object sender, EventArgs e)
	{
		Application.Current.Windows[0].Page = new SafeAreaBorderPage(_viewModel);
	}
}
