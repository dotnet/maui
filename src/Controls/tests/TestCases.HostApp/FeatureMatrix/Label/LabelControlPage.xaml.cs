using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class LabelControlPage : NavigationPage
{
	private LabelViewModel _viewModel;

	public LabelControlPage()
	{
		_viewModel = new LabelViewModel();
		PushAsync(new LabelControlMainPage(_viewModel));
	}
}

public partial class LabelControlMainPage : ContentPage
{
	private LabelViewModel _viewModel;

	public LabelControlMainPage(LabelViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new LabelViewModel();
		await Navigation.PushAsync(new LabelOptionsPage(_viewModel));
	}
}