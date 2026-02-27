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

	private void OnColorChanged(object sender, TextChangedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(ColorEntry.Text))
		{
			_viewModel.Color = null;
			return;
		}

		if (Color.TryParse(ColorEntry.Text, out Color color))
		{
			_viewModel.Color = color;
		}
	}

	private void ResetButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.Reset();
		ColorEntry.Text = string.Empty;
	}

	private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		_viewModel.CheckedChangedCommand.Execute(null);
	}
}

