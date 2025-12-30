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

		// Subscribe to TwoPaneView mode changes to update the CurrentModeText
		MyTwoPaneView.ModeChanged += OnTwoPaneViewModeChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// Initialize the IsWideMode property with the current TwoPaneView mode
		_viewModel.IsWideMode = MyTwoPaneView.Mode == Microsoft.Maui.Controls.Foldable.TwoPaneViewMode.Wide;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		// Unsubscribe from the event to prevent memory leaks
		MyTwoPaneView.ModeChanged -= OnTwoPaneViewModeChanged;
	}

	private void OnTwoPaneViewModeChanged(object sender, EventArgs e)
	{
		// Update the ViewModel's IsWideMode property based on the TwoPaneView's current mode
		if (sender is Microsoft.Maui.Controls.Foldable.TwoPaneView twoPaneView)
		{
			_viewModel.IsWideMode = twoPaneView.Mode == Microsoft.Maui.Controls.Foldable.TwoPaneViewMode.Wide;
		}
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new TwoPaneViewViewModel();
		await Navigation.PushAsync(new TwoPaneViewOptionsPage(_viewModel));
	}
}
