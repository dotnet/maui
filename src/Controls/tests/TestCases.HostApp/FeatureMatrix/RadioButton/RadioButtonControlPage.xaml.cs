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
		BindingContext = _viewModel = new RadioButtonViewModel();
		RadioButtonControlTwo.IsChecked = false;
		RadioButtonControlThree.IsChecked = false;
		RadioButtonControlFour.IsChecked = false;
		RadioButtonControlFive.IsChecked = false;
		SelectedValueLabelOne.Text = string.Empty;
		SelectedValueLabelTwo.Text = string.Empty;
		await Navigation.PushAsync(new RadioButtonOptionsPage(_viewModel));
	}

	private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			var radioButton = sender as RadioButton;
			if (radioButton != null && radioButton.GroupName == "Theme")
			{
				SelectedValueLabelOne.Text = radioButton.Content.ToString();
			}
			else
			{
				SelectedValueLabelTwo.Text = radioButton.Content.ToString();
			}
		}
	}
}
