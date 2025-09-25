using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample;

public partial class AbsoluteLayoutOptionsPage : ContentPage
{
	private AbsoluteLayoutViewModel _viewModel;
	public AbsoluteLayoutOptionsPage(AbsoluteLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnLayoutFlagCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None;

		if (chkX.IsChecked)
			flags |= AbsoluteLayoutFlags.XProportional;
		if (chkY.IsChecked)
			flags |= AbsoluteLayoutFlags.YProportional;
		if (chkWidth.IsChecked)
			flags |= AbsoluteLayoutFlags.WidthProportional;
		if (chkHeight.IsChecked)
			flags |= AbsoluteLayoutFlags.HeightProportional;
		if (chkPosition.IsChecked)
			flags |= AbsoluteLayoutFlags.PositionProportional;
		if (chkSize.IsChecked)
			flags |= AbsoluteLayoutFlags.SizeProportional;
		if (chkAll.IsChecked)
			flags |= AbsoluteLayoutFlags.All;

		if (chkNone.IsChecked)
			flags = AbsoluteLayoutFlags.None;

		_viewModel.LayoutFlags = flags;
	}

	private void IsVisibleRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		_viewModel.IsVisible = rb.Content?.ToString() == "True";
	}

	private void OnFlowDirectionChanged(object sender, EventArgs e)
	{
		_viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
	}

	private void OnBackgroundColorChanged(object sender, EventArgs e)
	{
		if (BindingContext is AbsoluteLayoutViewModel vm && sender is Button button && button.Text is string color)
		{
			switch (color)
			{
				case "Red":
					vm.BackgroundColor = Colors.Red;
					break;
				case "Gray":
					vm.BackgroundColor = Colors.Gray;
					break;
				case "LightYellow":
					vm.BackgroundColor = Colors.LightYellow;
					break;
				default:
					vm.BackgroundColor = Colors.White;
					break;
			}
		}
	}
}