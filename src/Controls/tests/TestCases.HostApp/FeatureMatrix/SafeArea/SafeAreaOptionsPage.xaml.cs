using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class SafeAreaOptionsPage : ContentPage
{
	private SafeAreaViewModel _viewModel;

	public SafeAreaOptionsPage(SafeAreaViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopModalAsync();
	}

	private void OnUniformChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		SafeAreaRegions region = SafeAreaRegions.None;

		if (UniformAll.IsChecked)
			region = SafeAreaRegions.All;
		else if (UniformContainer.IsChecked)
			region = SafeAreaRegions.Container;
		else if (UniformSoftInput.IsChecked)
			region = SafeAreaRegions.SoftInput;
		else if (UniformDefault.IsChecked)
			region = SafeAreaRegions.Default;

		// Set all four edges to the same value
		_viewModel.LeftEdge = region;
		_viewModel.TopEdge = region;
		_viewModel.RightEdge = region;
		_viewModel.BottomEdge = region;
	}

	private void OnLeftEdgeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		_viewModel.LeftEdge = GetSelectedRegion(LeftNone, LeftContainer, LeftSoftInput, LeftAll, LeftDefault);
	}

	private void OnTopEdgeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		_viewModel.TopEdge = GetSelectedRegion(TopNone, TopContainer, TopSoftInput, TopAll, TopDefault);
	}

	private void OnRightEdgeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		_viewModel.RightEdge = GetSelectedRegion(RightNone, RightContainer, RightSoftInput, RightAll, RightDefault);
	}

	private void OnBottomEdgeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		_viewModel.BottomEdge = GetSelectedRegion(BottomNone, BottomContainer, BottomSoftInput, BottomAll, BottomDefault);
	}

	private SafeAreaRegions GetSelectedRegion(RadioButton none, RadioButton container, RadioButton softInput, RadioButton all, RadioButton defaultBtn)
	{
		if (none.IsChecked) return SafeAreaRegions.None;
		if (container.IsChecked) return SafeAreaRegions.Container;
		if (softInput.IsChecked) return SafeAreaRegions.SoftInput;
		if (all.IsChecked) return SafeAreaRegions.All;
		if (defaultBtn.IsChecked) return SafeAreaRegions.Default;

		return SafeAreaRegions.None;
	}

	private void OnPaddingCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.Padding = e.Value ? new Thickness(20) : new Thickness(0);
	}

	private void OnBackgroundCheckBoxChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.Background = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1),
				GradientStops = new GradientStopCollection
				{
					new GradientStop(Colors.LightBlue, 0.0f),
					new GradientStop(Colors.LightPink, 1.0f)
				}
			};
		}
		else
		{
			_viewModel.Background = null;
		}
	}
}
