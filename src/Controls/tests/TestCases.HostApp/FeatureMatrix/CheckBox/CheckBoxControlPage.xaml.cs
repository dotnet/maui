using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CheckBoxControlPage : ContentPage
{
	private CheckBoxViewModel _viewModel;

	public CheckBoxControlPage()
	{
		InitializeComponent();
		_viewModel = new CheckBoxViewModel();
		BindingContext = _viewModel;
	}

	private void ResetButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.Reset();
	}

	private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.CheckedChangedCommand.Execute(null);
	}
}
