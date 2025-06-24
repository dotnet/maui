using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CheckBoxControlPage : ContentPage
{
	private CheckBoxFeatureMatrixViewModel _viewModel;

	public CheckBoxControlPage()
	{
		InitializeComponent();
		_viewModel = new CheckBoxFeatureMatrixViewModel();
		_viewModel.SetColorCommand = new Command<string>(OnSetColor);
		BindingContext = _viewModel;
	}

	private void OnSetColor(string colorName)
	{
		switch (colorName)
		{
			case "Blue":
				_viewModel.Color = Colors.Blue;
				break;
			case "Green":
				_viewModel.Color = Colors.Green;
				break;
			case "Default":
			default:
				_viewModel.Color = null;
				break;
		}
	}

	private void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel = new CheckBoxFeatureMatrixViewModel();
		_viewModel.SetColorCommand = new Command<string>(OnSetColor);
		BindingContext = _viewModel;
	}

	private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.CheckedChangedCommand.Execute(null);
	}

}


// Extension of the CheckBoxFeatureMatrixViewModel to add commands for the options page
public partial class CheckBoxFeatureMatrixViewModel
{
	public ICommand SetColorCommand { get; set; }
}

