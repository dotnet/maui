using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class ScrollViewControlPage : NavigationPage
{
	private ScrollViewViewModel _viewModel;

	public ScrollViewControlPage()
	{
		_viewModel = new ScrollViewViewModel();
		PushAsync(new ScrollViewControlMainPage(_viewModel));
	}
}

public partial class ScrollViewControlMainPage : ContentPage
{
	private ScrollViewViewModel _viewModel;

	public ScrollViewControlMainPage(ScrollViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new ScrollViewViewModel();
		_viewModel.ScrollToRequestedText = "Not Raised";
		_viewModel.ScrollX = 0;
		_viewModel.ScrollY = 0;
		_viewModel.ContentSize = new Size(0, 0);
		Dispatcher.Dispatch(async () =>
		{
			if (_viewModel.Content != null)
			{
				await MyScrollView.ScrollToAsync(_viewModel.Content, ScrollToPosition.MakeVisible, false);
			}
		});
		await Navigation.PushAsync(new ScrollViewOptionsPage(_viewModel));
	}

	private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
	{
		if (BindingContext is ScrollViewViewModel vm)
		{
			vm.ScrollX = e.ScrollX;
			vm.ScrollY = e.ScrollY;

		}
	}

	private void ScrollViewContent_SizeChanged(object sender, EventArgs e)
	{
		if (BindingContext is ScrollViewViewModel viewModel && sender is View content)
			viewModel.ContentSize = new Size(content.Width, content.Height);
	}


	private async void OnScrollToPositionButtonClicked(object sender, EventArgs e)
	{
		if (BindingContext is ScrollViewViewModel vm && sender is Button btn)
		{
			ScrollToPosition position = ScrollToPosition.MakeVisible;
			switch (btn.Text.ToLowerInvariant())
			{
				case "start":
					position = ScrollToPosition.Start;
					break;
				case "center":
					position = ScrollToPosition.Center;
					break;
				case "end":
					position = ScrollToPosition.End;
					break;
				case "visible":
					position = ScrollToPosition.MakeVisible;
					break;
			}
			if (vm.Content != null)
				await MyScrollView.ScrollToAsync(vm.Content, position, true);
		}
	}

	private void OnScrollViewScrollToRequested(object sender, ScrollToRequestedEventArgs e)
	{
		if (BindingContext is ScrollViewViewModel vm)
		{
			vm.ScrollToRequestedText = "Raised";

			vm.RequestedScrollX = e.ScrollX;
			vm.RequestedScrollY = e.ScrollY;
			vm.RequestedPosition = e.Position;
			vm.RequestedAnimate = e.ShouldAnimate;
			vm.Mode = e.Mode;
			vm.RequestedElementTypeName = e.Element?.GetType().Name ?? string.Empty;
		}
	}

	private async void OnScrollToPixelClicked(object sender, EventArgs e)
	{
		double x = 150;
		double y = 300;
		await MyScrollView.ScrollToAsync(x, y, true);
	}
}
