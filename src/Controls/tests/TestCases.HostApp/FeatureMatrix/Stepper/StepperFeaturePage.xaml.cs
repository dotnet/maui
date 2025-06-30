using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class StepperFeaturePage : ContentPage
{
	private StepperViewModel _viewModel;

	public StepperFeaturePage(StepperViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnMinimumChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(MinimumEntry.Text, out double min))
		{
			_viewModel.Minimum = min;
		}
	}

	private void OnMaximumChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(MaximumEntry.Text, out double max))
		{
			_viewModel.Maximum = max;
		}
	}

	private void OnIncrementChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(IncrementEntry.Text, out double increment))
		{
			_viewModel.Increment = increment;
		}
	}

	private void OnValueChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(ValueEntry.Text, out double value))
		{
			_viewModel.Value = value;
		}
	}

	private void OnIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.IsEnabled = IsEnabledTrueRadio.IsChecked;
	}

	private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.IsVisible = IsVisibleTrueRadio.IsChecked;
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.FlowDirection = FlowDirectionLTRRadio.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
	}
}