using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class TwoPaneViewControlPage : NavigationPage
{
	private TwoPaneViewViewModel _viewModel;

	public TwoPaneViewControlPage()
	{
		_viewModel = new TwoPaneViewViewModel();
		PushAsync(new TwoPaneViewControlMainPage(_viewModel));
	}
}
public partial class TwoPaneViewControlMainPage : ContentPage
{
	private TwoPaneViewViewModel _viewModel;
	public TwoPaneViewControlMainPage(TwoPaneViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new TwoPaneViewViewModel();
		await Navigation.PushAsync(new TwoPaneViewOptionsPage(_viewModel));
	}
}
