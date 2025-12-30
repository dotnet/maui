using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class LayoutFeaturePage : NavigationPage
{
	public LayoutFeaturePage()
	{
		PushAsync(new LayoutFeatureMainPage());
	}
}

public partial class LayoutFeatureMainPage : ContentPage
{
	public LayoutFeatureMainPage()
	{
		InitializeComponent();
	}

	private async void OnScrollViewWithLayoutOptionsButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new LayoutControlPage());
	}

	private async void OnDynamicStackLayoutButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new DynamicStackLayoutControlPage());
	}

	private async void OnDynamicGridLayoutButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new DynamicGridLayoutControlPage());
	}

	private async void OnDynamicFlexLayoutButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new DynamicFlexLayoutControlPage());
	}
}