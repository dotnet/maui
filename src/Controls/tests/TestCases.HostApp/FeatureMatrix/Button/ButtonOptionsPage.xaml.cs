namespace Maui.Controls.Sample;

public partial class ButtonOptionsPage : ContentPage
{
	private ButtonViewModel _viewModel;
	public ButtonOptionsPage(ButtonViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void BorderColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.BorderColor = button.Text switch
			{
				"Red" => Colors.Red,
				"Green" => Colors.Green,
				_ => Colors.White,
			};
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

	private void FontAttributesCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var attrs = FontAttributes.None;
		if (FontAttributesBoldCheckBox.IsChecked)
			attrs |= FontAttributes.Bold;
		if (FontAttributesItalicCheckBox.IsChecked)
			attrs |= FontAttributes.Italic;
		_viewModel.FontAttributes = attrs;
	}

	private void OnFontFamilyRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontFamily = radioButton.Content.ToString() == "Dokdo" ? "Dokdo" : "MontserratBold";
		}
	}

	private void OnIsEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIsVisibleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton != null && radioButton.IsChecked)
		{
			_viewModel.IsVisible = radioButton.Content.ToString() == "True";
		}
	}

	private void LineBreakModeButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.LineBreakMode = button.Text switch
			{
				"CharacterWrap" => LineBreakMode.CharacterWrap,
				"HeadTruncation" => LineBreakMode.HeadTruncation,
				"MiddleTruncation" => LineBreakMode.MiddleTruncation,
				"TailTruncation" => LineBreakMode.TailTruncation,
				"WordWrap" => LineBreakMode.WordWrap,
				_ => LineBreakMode.NoWrap,
			};
		}
	}

	private void OnShadowRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.ShadowOpacity = radioButton.Value.ToString() == "1" ? 1f : 0f;
		}
	}

	private void TextColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.TextColor = button.Text switch
			{
				"Red" => Colors.Red,
				"Green" => Colors.Green,
				_ => null,
			};
		}
	}

	private void OnTextTransformRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.TextTransform = radioButton.Content.ToString() == "Lowercase"
				? TextTransform.Lowercase
				: TextTransform.Uppercase;
		}
	}

	private void BackgroundColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.Background = button.AutomationId switch
			{
				"BackgroundColorRed" => new SolidColorBrush(Colors.Red),
				"BackgroundColorGreen" => new SolidColorBrush(Colors.Green),
				_ => null,
			};
		}
	}
}
