namespace Maui.Controls.Sample;

public partial class ButtonOptionsPage : ContentPage
{
	private ButtonViewModal _viewModel;
	public ButtonOptionsPage(ButtonViewModal viewModel)
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

	private void OnBorderColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.BorderColor = radioButton.Content.ToString() == "Red" ? Colors.Red : Colors.Green;
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

	private void OnFontAttributesRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontAttributes = radioButton.Content.ToString() == "Italic" ? FontAttributes.Italic : radioButton.Content.ToString() == "Bold" ? FontAttributes.Bold : FontAttributes.None;
		}
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

	private void OnLineBreakModeRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "CharacterWrap":
					_viewModel.LineBreakMode = LineBreakMode.CharacterWrap;
					break;
				case "HeadTruncation":
					_viewModel.LineBreakMode = LineBreakMode.HeadTruncation;
					break;
				case "MiddleTruncation":
					_viewModel.LineBreakMode = LineBreakMode.MiddleTruncation;
					break;
				case "TailTruncation":
					_viewModel.LineBreakMode = LineBreakMode.TailTruncation;
					break;
				case "WordWrap":
					_viewModel.LineBreakMode = LineBreakMode.WordWrap;
					break;
				default:
					_viewModel.LineBreakMode = LineBreakMode.NoWrap;
					break;
			}
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

	private void OnTextColorRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.TextColor = radioButton.Content.ToString() == "Red" ? Colors.Red : Colors.Green;
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
}