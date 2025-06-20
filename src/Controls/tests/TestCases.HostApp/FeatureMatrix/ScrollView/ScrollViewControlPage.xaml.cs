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
		await MyScrollView.ScrollToAsync(0, 0, false);
		_viewModel.ScrollX = 0;
		_viewModel.ScrollY = 0;
		_viewModel.ContentSize = new Size(0, 0);
		ScrollToPositionEntry.Text = string.Empty;
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


	private async void OnScrollToTargetClicked(object sender, EventArgs e)
	{
		if (BindingContext is ScrollViewViewModel vm)
		{
			var positionText = ScrollToPositionEntry.Text?.Trim();
			ScrollToPosition position = ScrollToPosition.MakeVisible;

			switch (positionText?.ToLowerInvariant())
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
				case "makevisible":
					position = ScrollToPosition.MakeVisible;
					break;
			}
			if (vm.Content != null)
				await MyScrollView.ScrollToAsync(vm.Content, position, true);
		}
	}
}