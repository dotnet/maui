using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ImageButtonControlPage : NavigationPage
{
	private ImageButtonViewModel _viewModel;

	public ImageButtonControlPage()
	{
		_viewModel = new ImageButtonViewModel();
		PushAsync(new ImageButtonControlMainPage(_viewModel));
	}
}

public partial class ImageButtonControlMainPage : ContentPage
{
	private ImageButtonViewModel _viewModel;

	public ImageButtonControlMainPage(ImageButtonViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ImageButtonViewModel();
		await Navigation.PushAsync(new ImageButtonOptionsPage(_viewModel));
	}

	private void OnImageButtonClicked(object sender, EventArgs e)
	{
		_viewModel.ClickTotal++;
		_viewModel.IsButtonClicked = true;
	}

	private void OnImageButtonPressed(object sender, EventArgs e)
	{
		_viewModel.PressedTotal++;
		_viewModel.IsButtonClicked = true;
	}

	private void OnImageButtonReleased(object sender, EventArgs e)
	{
		_viewModel.ReleasedTotal++;
		_viewModel.IsButtonClicked = true;
	}
}