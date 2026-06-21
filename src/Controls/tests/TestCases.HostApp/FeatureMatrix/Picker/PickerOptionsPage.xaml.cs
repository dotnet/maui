namespace Maui.Controls.Sample;

public partial class PickerOptionsPage : ContentPage
{
	private PickerViewModel _viewModel;

	public PickerOptionsPage(PickerViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void OnTitleChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.NewTextValue))
		{
			_viewModel.Title = e.NewTextValue;
		}
	}

	private void OnSelectedIndexChanged(object sender, TextChangedEventArgs e)
	{
		if (int.TryParse(e.NewTextValue, out int selectedIndex))
		{
			_viewModel.SelectedIndex = selectedIndex;
		}
	}

	private void OnSelectedItemChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.NewTextValue))
		{
			var matchingItem = _viewModel.ItemsSource.FirstOrDefault(item =>
				item.ToString().Equals(e.NewTextValue, StringComparison.OrdinalIgnoreCase));

			if (matchingItem != null)
			{
				_viewModel.SelectedItem = matchingItem;
			}
			else
			{
				_viewModel.SelectedItem = null;
			}
		}
	}

	private void OnCharacterSpacingChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(e.NewTextValue, out double characterSpacing))
		{
			_viewModel.CharacterSpacing = characterSpacing;
		}
	}

	private void OnFontSizeChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(e.NewTextValue, out double fontSize))
		{
			_viewModel.FontSize = fontSize;
		}
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() switch
			{
				"LTR" => FlowDirection.LeftToRight,
				"RTL" => FlowDirection.RightToLeft,
				_ => FlowDirection.LeftToRight
			};
		}
	}

	private void OnFontAttributesChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.FontAttributes = radioButton.Content.ToString() switch
			{
				"Bold" => FontAttributes.Bold,
				"Italic" => FontAttributes.Italic,
				_ => FontAttributes.None
			};
		}
	}

	private void OnFontAutoScalingChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.FontAutoScalingEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.IsEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.IsVisible = radioButton.Content.ToString() == "True";
		}
	}

	private void OnHorizontalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.HorizontalTextAlignment = radioButton.Content.ToString() switch
			{
				"Center" => TextAlignment.Center,
				"End" => TextAlignment.End,
				_ => TextAlignment.Start
			};
		}
	}

	private void OnVerticalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.VerticalTextAlignment = radioButton.Content.ToString() switch
			{
				"Start" => TextAlignment.Start,
				"End" => TextAlignment.End,
				_ => TextAlignment.Center
			};
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

	private void TextColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.TextColor = button.Text switch
			{
				"Red" => Colors.Red,
				"Blue" => Colors.Blue,
				_ => null
			};
		}
	}

	private void TitleColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.TitleColor = button.Text switch
			{
				"Purple" => Colors.Purple,
				"Orange" => Colors.Orange,
				_ => null
			};
		}
	}

	private void OnFontFamilyRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		var radioButton = sender as RadioButton;
		if (radioButton.IsChecked)
		{
			_viewModel.FontFamily = "Dokdo";
		}
	}

	private void ItemDisplayBindingButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			switch (button.Text)
			{
				case "Name Only":
					_viewModel.ItemDisplayBinding = new Binding("Name");
					break;
				case "Description Only":
					_viewModel.ItemDisplayBinding = new Binding("Description");
					break;
				case "Default (ToString)":
					_viewModel.ItemDisplayBinding = null;
					break;
			}
		}
	}
}