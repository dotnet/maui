using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class StepperControlPage : NavigationPage
{

	public StepperControlPage()
	{
		PushAsync(new StepperControlMainPage());
	}
}
public partial class StepperControlMainPage : ContentPage
{
	private StepperViewModel _viewModel;

	public StepperControlMainPage()
	{
		InitializeComponent();

		if (OperatingSystem.IsIOSVersionAtLeast(26))
		{
			StepperControl.HorizontalOptions = LayoutOptions.Center;
		}

		BindingContext = _viewModel = new StepperViewModel();
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new StepperViewModel();
		await Navigation.PushAsync(new StepperFeaturePage(_viewModel));
	}
}