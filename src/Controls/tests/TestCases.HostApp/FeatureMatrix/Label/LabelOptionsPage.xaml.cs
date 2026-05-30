namespace Maui.Controls.Sample;

public partial class LabelOptionsPage : ContentPage
{
	private readonly LabelViewModel _viewModel;

	public LabelOptionsPage(LabelViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		if (Navigation.NavigationStack.Count > 1)
		{
			await Navigation.PopAsync();
		}
	}

	private void OnFontSizeChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(FontSizeEntry?.Text) &&
			double.TryParse(FontSizeEntry.Text, out double size))
		{
			_viewModel.FontSize = size;
		}
	}

	private void OnCharacterSpacingChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(CharacterSpacingEntry?.Text) &&
			double.TryParse(CharacterSpacingEntry.Text, out double spacing))
		{
			_viewModel.CharacterSpacing = spacing;
		}
	}

	private void OnLineHeightChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(LineHeightEntry?.Text) &&
			double.TryParse(LineHeightEntry.Text, out double height))
		{
			_viewModel.LineHeight = height;
		}
	}

	private void OnMaxLinesChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrWhiteSpace(MaxLinesEntry?.Text) &&
			int.TryParse(MaxLinesEntry.Text, out int lines))
		{
			_viewModel.MaxLines = lines;
		}
	}

	private void OnPaddingChanged(object sender, TextChangedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(PaddingEntry?.Text))
			return;

		try
		{
			string[] parts = PaddingEntry.Text.Split(',');
			if (parts.Length == 4 &&
				double.TryParse(parts[0], out double left) &&
				double.TryParse(parts[1], out double top) &&
				double.TryParse(parts[2], out double right) &&
				double.TryParse(parts[3], out double bottom))
			{
				_viewModel.Padding = new Thickness(left, top, right, bottom);
			}
		}
		catch { }

	}

	private void OnFontAttributesChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.FontAttributes = rb.Value.ToString() switch
			{
				"Bold" => FontAttributes.Bold,
				"Italic" => FontAttributes.Italic,
				_ => FontAttributes.None
			};
		}
	}

	private void OnFontFamilyChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.FontFamily = rb.Value.ToString() switch
			{
				"Dokdo" => "Dokdo",
				"Mont" => "MontserratBold",
				_ => null
			};
		}
	}
	private void OnFontAutoScalingChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && e.Value)
		{
			string valueStr = rb.Value?.ToString();
			if (bool.TryParse(valueStr, out bool result))
			{
				_viewModel.FontAutoScalingEnabled = result;
			}
		}
	}



	private void OnTextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is LabelViewModel viewModel)
		{
			viewModel.Text = e.NewTextValue;
		}
	}

	private void OnTextColorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.TextColor = rb.Value.ToString() switch
			{
				"Red" => Colors.Red,
				"Green" => Colors.Green,
				_ => Colors.Black
			};
		}
	}

	private void OnTextDecorationsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.TextDecorations = rb.Value.ToString() switch
			{
				"Underline" => TextDecorations.Underline,
				"Strikethrough" => TextDecorations.Strikethrough,
				"Nil" => TextDecorations.None,
				_ => TextDecorations.None
			};
		}
	}

	private void OnTextTransformChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.TextTransform = rb.Value.ToString() switch
			{
				"Uppercase" => TextTransform.Uppercase,
				"Lowercase" => TextTransform.Lowercase,
				"Nil" => TextTransform.None,
				_ => TextTransform.None
			};
		}
	}

	private void OnTextTypeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.TextType = rb.Value.ToString() switch
			{
				"Html" => TextType.Html,
				"Plain" => TextType.Text,
				_ => TextType.Text
			};
		}
	}

	private void OnHorizontalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.HorizontalTextAlignment = rb.Value.ToString() switch
			{
				"St" => TextAlignment.Start,
				"Cen" => TextAlignment.Center,
				"End" => TextAlignment.End,
				_ => TextAlignment.Start
			};
		}
	}

	private void OnVerticalTextAlignmentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.VerticalTextAlignment = rb.Value.ToString() switch
			{
				"St" => TextAlignment.Start,
				"Cen" => TextAlignment.Center,
				"End" => TextAlignment.End,
				_ => TextAlignment.Start
			};
		}
	}

	private void OnLineBreakModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton rb && rb.IsChecked && rb.Value != null)
		{
			_viewModel.LineBreakMode = rb.Value.ToString() switch
			{
				"Head" => LineBreakMode.HeadTruncation,
				"Mid" => LineBreakMode.MiddleTruncation,
				"Ta" => LineBreakMode.TailTruncation,
				"No" => LineBreakMode.NoWrap,
				"Char" => LineBreakMode.CharacterWrap,
				"Word" => LineBreakMode.WordWrap,
				_ => LineBreakMode.WordWrap
			};
		}
	}

	private void OnFormattedTextChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value)
		{
			_viewModel.FormattedText = new FormattedString
			{
				Spans =
			{
				new Span { Text = "Lorem ipsum dolor sit amet consectetur adipis elit vivamus lacinia felis eu sagittis congue nibh urna malesuada orci at fringilla quam turpis eget nunc", FontAttributes = FontAttributes.Bold },
				new Span { Text = "consectetur adipiscing elit", FontAttributes = FontAttributes.Italic },
				new Span { Text = "Sed do eiusmod tempor.", FontAttributes = FontAttributes.Bold }
			}
			};
			_viewModel.Text = null;
		}
	}

}