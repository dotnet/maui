using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class RadioButtonControlPage : NavigationPage
{
	public RadioButtonControlPage()
	{
		PushAsync(new RadioButtonControlMainPage());
	}
}

public partial class RadioButtonControlMainPage : ContentPage
{
	private RadioButtonViewModel _viewModel;

	public RadioButtonControlMainPage()
	{
		InitializeComponent();
		_viewModel = new RadioButtonViewModel();
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		// reset the the control page values
		BindingContext = _viewModel = new RadioButtonViewModel();
		await Navigation.PushAsync(new RadioButtonOptionsPage(_viewModel));
	}

	private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			var radioButton = sender as RadioButton;
			if (radioButton != null)
			{
				SelectedValueLabel.Text = radioButton.Content.ToString();
			}
		}
	}

	private void RadioButton_CheckedChanged_2(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			var radioButton = sender as RadioButton;
			if (radioButton != null)
			{
				SelectedValueLabel2.Text = radioButton.Content.ToString();
			}
		}

	}
}
