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
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
		BindingContext = _viewModel;
	}

	private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(CheckBoxViewModel.Color) && _viewModel.Color == null)
		{
			MyCheckBox.ClearValue(CheckBox.ColorProperty);
		}
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

