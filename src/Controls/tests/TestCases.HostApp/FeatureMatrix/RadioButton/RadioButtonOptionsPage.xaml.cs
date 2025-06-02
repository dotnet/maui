using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class RadioButtonOptionsPage : ContentPage
{
	private RadioButtonViewModel _viewModel;

	public RadioButtonOptionsPage(RadioButtonViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnContentChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(ContentEntry.Text))
		{
			_viewModel.Content = ContentEntry.Text;
		}
	}

	private void ViewContentButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.Content = new Label
		{
			Text = "App Theme Selection",
			FontSize = 14,
			TextColor = Colors.Black,
			BackgroundColor = Colors.SkyBlue
		};
	}

	private void OnIsVisibleChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsVisible = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIsEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "Left to Right" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}



	private void OnIsCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsChecked = radioButton.Content.ToString() == "True";
		}
	}

	private void OnGroupNameChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.GroupName = radioButton.Content.ToString();
		}
	}

	private void OnValueChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(ValueEntry.Text))
		{
			_viewModel.Value = ValueEntry.Text;
		}
	}

	private void BorderColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "Green":
				_viewModel.BorderColor = Colors.Green;
				break;
			case "Purple":
				_viewModel.BorderColor = Colors.Purple;
				break;
			default:
				_viewModel.BorderColor = Color.FromRgba(1, 122, 255, 255);
				break;
		}
	}

	private void OnBorderWidthChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(BorderWidthEntry.Text, out double width))
		{
			_viewModel.BorderWidth = width;
		}
	}

	private void OnCharacterSpacingChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(CharacterSpacingEntry.Text, out double spacing))
		{
			_viewModel.CharacterSpacing = spacing;
		}
	}

	private void OnCornerRadiusChanged(object sender, TextChangedEventArgs e)
	{
		if (int.TryParse(CornerRadiusEntry.Text, out int radius))
		{
			_viewModel.CornerRadius = radius;
		}
	}

	private void OnFontAttributesChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Bold":
					_viewModel.FontAttributes = FontAttributes.Bold;
					break;
				case "Italic":
					_viewModel.FontAttributes = FontAttributes.Italic;
					break;
				default:
					_viewModel.FontAttributes = FontAttributes.None;
					break;
			}
		}
	}

	private void OnFontAutoScalingChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.FontAutoScalingEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnFontFamilyChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Dokdo":
					_viewModel.FontFamily = "Dokdo";
					break;
				case "MontserratBold":
					_viewModel.FontFamily = "MontserratBold";
					break;
			}
		}
	}

	private void OnFontSizeChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(FontSizeEntry.Text, out double size))
		{
			_viewModel.FontSize = size;
		}
	}

	private void TextColorButton_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		switch (button.Text)
		{
			case "Red":
				_viewModel.TextColor = Colors.Red;
				break;
			case "Blue":
				_viewModel.TextColor = Colors.Blue;
				break;
			default:
				_viewModel.TextColor = Colors.Black;
				break;
		}
	}

	private void OnTextTransformChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Upper":
					_viewModel.TextTransform = TextTransform.Uppercase;
					break;
				case "Lower":
					_viewModel.TextTransform = TextTransform.Lowercase;
					break;
				default:
					_viewModel.TextTransform = TextTransform.None;
					break;
			}
		}
	}

	private void OnSelectedRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.SelectedValue = radioButton.Content.ToString();
		}
	}
}
