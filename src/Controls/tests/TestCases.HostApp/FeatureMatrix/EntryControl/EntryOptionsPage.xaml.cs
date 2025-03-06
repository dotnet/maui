using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample
{
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
            _viewModel.IsPassword = IsPasswordTrue.IsChecked;
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

        private void SelectionLength_Clicked(object sender, EventArgs e)
        {
            if (int.TryParse(SelectionLengthEntry.Text, out int selectionLength))
            {
                _viewModel.SelectionLength = selectionLength;
            }
        }
        private void CursorPositionButton_Clicked(object sender, EventArgs e)
        {
            if (int.TryParse(CursorPositionEntry.Text, out int cursorPosition))
            {
                _viewModel.CursorPosition = cursorPosition;
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
            _viewModel.IsReadOnly = IsReadOnlyTrue.IsChecked;
        }
        private void IsTextPredictionEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
        {
            _viewModel.IsTextPredictionEnabled = IsTextPredictionEnabledTrue.IsChecked;
        }

        private void IsSpellCheckEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
        {
            _viewModel.IsSpellCheckEnabled = IsSpellCheckEnabledTrue.IsChecked;
        }

        private void FontAutoScalingEnabledTrueOrFalse_Clicked(object sender, EventArgs e)
        {
            _viewModel.FontAutoScalingEnabled = FontAutoScalingEnabledTrue.IsChecked;
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
    }
}