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
	private bool _recreating;

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

	async void MainLabel_Tapped(object sender, TappedEventArgs e)
	{
		// Guard against re-entrancy: repeated taps before PopAsync completes would otherwise
		// insert multiple pages and pop the wrong one. Recreation runs at most once per instance.
		if (_recreating)
			return;
		_recreating = true;

		// Recreate the page to verify initial mappers
		// Clear BindingContext first so old Label properly detaches the FormattedString
		// (triggers propertyChanging which unsubscribes events and calls RemoveSpans)
		BindingContext = null;
		Navigation.InsertPageBefore(new LabelControlMainPage(_viewModel), this);
		await Navigation.PopAsync(false);
	}
}