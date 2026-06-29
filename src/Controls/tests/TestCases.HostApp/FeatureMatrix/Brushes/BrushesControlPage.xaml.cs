using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class BrushesControlPage : NavigationPage
{
	private BrushesViewModel _viewModel;
	public BrushesControlPage()
	{
		_viewModel = new BrushesViewModel();
		PushAsync(new BrushesControlMainPage(_viewModel));
	}
}

public partial class BrushesControlMainPage : ContentPage
{
	private BrushesViewModel _viewModel;

	public BrushesControlMainPage(BrushesViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	private void OnAddLinearStopClicked(object sender, EventArgs e)
	{
		_viewModel?.AddLinearGradientStopCommand?.Execute(null);
	}

	private void OnRemoveLinearStopClicked(object sender, EventArgs e)
	{
		_viewModel?.RemoveLinearGradientStopCommand?.Execute(null);
	}

	private void OnAddRadialStopClicked(object sender, EventArgs e)
	{
		_viewModel?.AddRadialGradientStopCommand?.Execute(null);
	}

	private void OnRemoveRadialStopClicked(object sender, EventArgs e)
	{
		_viewModel?.RemoveRadialGradientStopCommand?.Execute(null);
	}

	private void OnApplyLinearClicked(object sender, EventArgs e)
	{
		_viewModel?.ApplyLinearGradientCommand?.Execute(null);
	}

	private void OnApplyRadialClicked(object sender, EventArgs e)
	{
		_viewModel?.ApplyRadialGradientCommand?.Execute(null);
	}

	private void OnCompareClicked(object sender, EventArgs e)
	{
		if (BindingContext is BrushesViewModel vm)
		{
			vm.CompareBrushesCommand?.Execute(null);
		}
	}

	private async void OnOptionsClicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BrushesViewModel();
		await Navigation.PushAsync(new BrushesOptionsPage(_viewModel));
	}
}