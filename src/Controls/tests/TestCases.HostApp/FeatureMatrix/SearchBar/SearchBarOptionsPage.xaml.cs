namespace Maui.Controls.Sample;

public partial class SearchBarOptionsPage : ContentPage
{
	private SearchBarViewModel _viewModel;
	public SearchBarOptionsPage(SearchBarViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void OnCancelButtonColorClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			_viewModel.CancelButtonColor = button.Text == "Orange" ? Colors.Orange : Colors.Yellow;
		}
	}

	private void OnFlowDirectionRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LTR" ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}
	}

	private void OnFontAttributesRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontAttributes = radioButton.Content.ToString() == "Italic" ? FontAttributes.Italic : FontAttributes.Bold;
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

	private void OnHorizontalTextAlignmentRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.HorizontalTextAlignment = radioButton.Content.ToString() == "Center"
				? TextAlignment.Center
				: TextAlignment.End;
		}
	}

	private void OnIsEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsEnabled = false;
		}
	}

	private void OnIsVisibleRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsVisible = false;
		}
	}

	private void OnShadowRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.Shadow = new Shadow { Brush = Colors.Violet, Radius = 10, Offset = new Point(0, 0), Opacity = 1f };
		}
	}

	private void OnIsReadOnlyRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsReadOnly = radioButton.Content.ToString() == "True" ? true : false;
		}
	}

	private void OnIsSpellCheckEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsSpellCheckEnabled = radioButton.Content.ToString() == "True" ? true : false;
		}
	}

	private void OnIsTextPredictionEnabledRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.IsTextPredictionEnabled = radioButton.Content.ToString() == "True" ? true : false;
		}
	}

	private void OnKeyboardRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Chat":
					_viewModel.Keyboard = Keyboard.Chat;
					break;
				case "Numeric":
					_viewModel.Keyboard = Keyboard.Numeric;
					break;
				case "Text":
					_viewModel.Keyboard = Keyboard.Text;
					break;
				case "URL":
					_viewModel.Keyboard = Keyboard.Url;
					break;
				default:
					_viewModel.Keyboard = Keyboard.Default;
					break;
			}
		}
	}

	private void OnPlaceholderColorButtonClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			_viewModel.PlaceholderColor = button.Text == "Red" ? Colors.Red : Colors.Green;
		}
	}

	private void OnTextColorButtonClicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		if (button != null)
		{
			_viewModel.TextColor = button.Text == "Red" ? Colors.Red : Colors.Green;
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

	private void OnVerticalTextAlignmentRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			switch (radioButton.Content.ToString())
			{
				case "Start":
					_viewModel.VerticalTextAlignment = TextAlignment.Start;
					break;
				case "End":
					_viewModel.VerticalTextAlignment = TextAlignment.End;
					break;
				default:
					_viewModel.VerticalTextAlignment = TextAlignment.Center;
					break;
			}
		}
	}

	private void OnFontAutoScalingRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			var radioButton = sender as RadioButton;
			if (radioButton.Content.ToString() == "False")
			{
				_viewModel.FontAutoScalingEnabled = false;
			}
		}
	}
}