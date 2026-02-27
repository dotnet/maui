using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class GraphicsViewControlPage : NavigationPage
{
	private GraphicsViewViewModel _viewModel;

	public GraphicsViewControlPage()
	{
		_viewModel = new GraphicsViewViewModel();
		PushAsync(new GraphicsViewControlMainPage(_viewModel));
	}
}

public partial class GraphicsViewControlMainPage : ContentPage
{
	private GraphicsViewViewModel _viewModel;

	public GraphicsViewControlMainPage(GraphicsViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;

		// Ensure GraphicsView invalidates when drawable changes
		_viewModel.RequestInvalidate = () => graphicsView.Invalidate();

		// Set default drawable
		_viewModel.SelectedDrawable = DrawableType.Square;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new GraphicsViewViewModel(); // Ensure the ViewModel is set for the options page
		await Navigation.PushAsync(new GraphicsViewOptionsPage(_viewModel));
	}

	private void OnStartHoverInteraction(object sender, TouchEventArgs e)
	{
		_viewModel.AddInteractionEvent("StartHoverInteraction");
	}

	private void OnMoveHoverInteraction(object sender, TouchEventArgs e)
	{
		_viewModel.AddInteractionEvent("MoveHoverInteraction");
	}

	private void OnEndHoverInteraction(object sender, EventArgs e)
	{
		_viewModel.AddInteractionEvent("EndHoverInteraction");
	}

	private void OnStartInteraction(object sender, TouchEventArgs e)
	{
		_viewModel.AddInteractionEvent("StartInteraction");
	}

	private void OnDragInteraction(object sender, TouchEventArgs e)
	{
		_viewModel.AddInteractionEvent("DragInteraction");
	}

	private void OnEndInteraction(object sender, EventArgs e)
	{
		_viewModel.AddInteractionEvent("EndInteraction");
	}

	private void OnCancelInteraction(object sender, EventArgs e)
	{
		_viewModel.AddInteractionEvent("CancelInteraction");
	}

	private void OnInvalidateClicked(object sender, EventArgs e)
	{
		// Use a constant color for consistent screenshot testing
		_viewModel.CurrentDrawColor = Colors.Green;

		// Call Invalidate on the GraphicsView to force a redraw
		graphicsView.Invalidate();

		// Add the invalidate event to the history with color information
		_viewModel.AddInteractionEvent($"Invalidate() called - Color changed to {_viewModel.CurrentDrawColor}");
	}

	private void OnClearEventsClicked(object sender, EventArgs e)
	{
		// Clear the interaction event history
		_viewModel.ClearInteractionHistory();
	}

	private void DisplayStoredDimensions()
	{
		var dimensions = _viewModel.GetDrawableDimensions(_viewModel.SelectedDrawable);
		if (dimensions.HasValue)
		{
			Debug.WriteLine($"Stored Dimensions for {_viewModel.SelectedDrawable}: Width = {dimensions.Value.Width}, Height = {dimensions.Value.Height}");
		}
		else
		{
			Debug.WriteLine($"No stored dimensions found for {_viewModel.SelectedDrawable}.");
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		DisplayStoredDimensions();
	}
}
