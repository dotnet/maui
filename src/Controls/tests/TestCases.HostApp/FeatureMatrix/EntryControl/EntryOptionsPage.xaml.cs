using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class EntryOptionsPage : ContentPage
{
	private EntryViewModel _viewModel;

	public EntryOptionsPage(EntryViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	private void TextColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.TextColor = button.BackgroundColor;
		}
	}

	private void Entry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is EntryViewModel vm)
		{
			vm.Text = e.NewTextValue;
		}
	}

	private void PlaceholderEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (BindingContext is EntryViewModel vm)
		{
			vm.Placeholder = e.NewTextValue;
		}
	}

	private void PlaceholderColorButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.PlaceholderColor = button.BackgroundColor;
		}
	}

	private void ClearButtonVisibility_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender == ClearButtonWhileEditing)
		{
			_viewModel.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
		}
		else if (sender == ClearButtonNever)
		{
			_viewModel.ClearButtonVisibility = ClearButtonVisibility.Never;
		}
	}

	private void HorizontalAlignmentButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.HorizontalTextAlignment = button.AutomationId switch
			{
				"HStart" => TextAlignment.Start,
				"HCenter" => TextAlignment.Center,
				"HEnd" => TextAlignment.End,
				_ => _viewModel.HorizontalTextAlignment
			};
		}
	}

	private void VerticalAlignmentButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.HeightRequest = 100;
		if (sender is Button button)
		{
			_viewModel.VerticalTextAlignment = button.AutomationId switch
			{
				"VStart" => TextAlignment.Start,
				"VCenter" => TextAlignment.Center,
				"VEnd" => TextAlignment.End,
				_ => _viewModel.VerticalTextAlignment
			};
		}
	}

	private void IsPasswordTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsPasswordTrue.IsChecked)
		{
			_viewModel.IsPassword = true;
		}
		else if (IsPasswordFalse.IsChecked)
		{
			_viewModel.IsPassword = false;
		}
	}

	private void ReturnTypeButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.ReturnType = button.AutomationId switch
			{
				"Done" => ReturnType.Done,
				"Next" => ReturnType.Next,
				"Go" => ReturnType.Go,
				"Search" => ReturnType.Search,
				"Send" => ReturnType.Send,
				"Default" => ReturnType.Default,
				_ => _viewModel.ReturnType
			};
		}
	}

	private void MaxLengthButton_Clicked(object sender, EventArgs e)
	{
		if (int.TryParse(MaxLengthEntry.Text, out int maxLength))
		{
			_viewModel.MaxLength = maxLength;
		}
	}

	private void FontSizeEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(FontSizeEntry.Text, out double fontSize))
		{
			_viewModel.FontSize = fontSize;
		}
	}

	private void CharacterSpacing_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(CharacterSpacingEntry.Text, out double characterSpacing))
		{
			_viewModel.CharacterSpacing = characterSpacing;
		}
	}

	private void IsReadOnlyTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsReadOnlyTrue.IsChecked)
		{
			_viewModel.IsReadOnly = true;
		}
		else if (IsReadOnlyFalse.IsChecked)
		{
			_viewModel.IsReadOnly = false;
		}
	}

	private void IsTextPredictionEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsTextPredictionEnabledTrue.IsChecked)
		{
			_viewModel.IsTextPredictionEnabled = true;
		}
		else if (IsTextPredictionEnabledFalse.IsChecked)
		{
			_viewModel.IsTextPredictionEnabled = false;
		}
	}

	private void IsSpellCheckEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsSpellCheckEnabledTrue.IsChecked)
		{
			_viewModel.IsSpellCheckEnabled = true;
		}
		else if (IsSpellCheckEnabledFalse.IsChecked)
		{
			_viewModel.IsSpellCheckEnabled = false;
		}
	}

	private void KeyboardButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			_viewModel.Keyboard = button.AutomationId switch
			{
				"Default" => Keyboard.Default,
				"Chat" => Keyboard.Chat,
				"Email" => Keyboard.Email,
				"Numeric" => Keyboard.Numeric,
				"Telephone" => Keyboard.Telephone,
				"Text" => Keyboard.Text,
				"Url" => Keyboard.Url,
				_ => _viewModel.Keyboard
			};
		}
	}

	private void FontFamilyEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		_viewModel.FontFamily = FontFamilyEntry.Text;
	}

	private void FlowDirection_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender == FlowDirectionLeftToRight)
		{
			_viewModel.FlowDirection = FlowDirection.LeftToRight;
		}
		else if (sender == FlowDirectionRightToLeft)
		{
			_viewModel.FlowDirection = FlowDirection.RightToLeft;
		}
	}

	private void IsVisibleTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsVisibleTrue.IsChecked)
		{
			_viewModel.IsVisible = true;
		}
		else if (IsVisibleFalse.IsChecked)
		{
			_viewModel.IsVisible = false;
		}
	}

	private void IsEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
	{
		if (IsEnabledTrue.IsChecked)
		{
			_viewModel.IsEnabled = true;
		}
		else if (IsEnabledFalse.IsChecked)
		{
			_viewModel.IsEnabled = false;
		}
	}

	private void TextTransform_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender == TextTransformLowercase)
		{
			_viewModel.TextTransform = TextTransform.Lowercase;
		}
		else if (sender == TextTransformUppercase)
		{
			_viewModel.TextTransform = TextTransform.Uppercase;
		}
		else if (sender == TextTransformDefault)
		{
			_viewModel.TextTransform = TextTransform.Default;
		}
	}

	private void FontAttributes_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender == FontAttributesBold)
		{
			_viewModel.FontAttributes = FontAttributes.Bold;
		}
		else if (sender == FontAttributesNone)
		{
			_viewModel.FontAttributes = FontAttributes.None;
		}
		else if (sender == FontAttributesItalic)
		{
			_viewModel.FontAttributes = FontAttributes.Italic;
		}
	}
}