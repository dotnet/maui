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
		_viewModel.ScrollX = 0;
		_viewModel.ScrollY = 0;
		 ScrollToStartRadio.IsChecked = true;
		_viewModel.ContentSize = new Size(0, 0);
		Dispatcher.Dispatch(async () =>
		{
			if (_viewModel.Content != null)
			{
				await MyScrollView.ScrollToAsync(_viewModel.Content, ScrollToPosition.Start, false);
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


	private async void OnScrollToPositionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		if (BindingContext is ScrollViewViewModel vm)
		{
			ScrollToPosition position = ScrollToPosition.MakeVisible;

			if (sender == ScrollToStartRadio)
				position = ScrollToPosition.Start;
			else if (sender == ScrollToCenterRadio)
				position = ScrollToPosition.Center;
			else if (sender == ScrollToEndRadio)
				position = ScrollToPosition.End;
			else if (sender == ScrollToMakeVisibleRadio)
				position = ScrollToPosition.MakeVisible;

			if (vm.Content != null)
				await MyScrollView.ScrollToAsync(vm.Content, position, true);
		}
	}
}