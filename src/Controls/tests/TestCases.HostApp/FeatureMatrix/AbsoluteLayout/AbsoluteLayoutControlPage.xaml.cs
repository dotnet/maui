using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;

public class AbsoluteLayoutControlPage : NavigationPage
{
	private AbsoluteLayoutViewModel _viewModel;
	public AbsoluteLayoutControlPage()
	{
		_viewModel = new AbsoluteLayoutViewModel();
		PushAsync(new AbsoluteLayoutControlMainPage(_viewModel));
	}
}

public partial class AbsoluteLayoutControlMainPage : ContentPage
{
	private AbsoluteLayoutViewModel _viewModel;

	public AbsoluteLayoutControlMainPage(AbsoluteLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new AbsoluteLayoutViewModel();
		await Navigation.PushAsync(new AbsoluteLayoutOptionsPage(_viewModel));
	}
}