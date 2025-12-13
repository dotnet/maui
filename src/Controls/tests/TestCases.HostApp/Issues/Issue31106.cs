using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 31106, "[MacCatalyst] Picker dialog closes automatically with VoiceOver/Keyboard", PlatformAffected.macOS)]
    public class Issue31106 : ContentPage
    {
        readonly Picker _testPicker;
        readonly Label _statusLabel;
        readonly Label _pickerStateLabel;

        public Issue31106()
        {
            Title = "Issue 31106";

            _testPicker = new Picker
            {
                Title = "Select an item",
                AutomationId = "TestPicker"
            };

            _statusLabel = new Label
            {
                Text = "Tap the picker to open it",
                AutomationId = "StatusLabel",
                Margin = new Thickness(0, 20, 0, 0)
            };

            _pickerStateLabel = new Label
            {
                Text = "Picker State: Closed",
                AutomationId = "PickerStateLabel",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Blue
            };

            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20),
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Test: Enable VoiceOver or use Tab key to navigate to the picker",
                        AutomationId = "InstructionsLabel"
                    },
                    _testPicker,
                    _pickerStateLabel,
                    _statusLabel
                }
            };

            var items = new List<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };

            _testPicker.ItemsSource = items;
            _testPicker.SelectedIndexChanged += OnPickerSelectedIndexChanged;
            _testPicker.Focused += OnPickerFocused;
            _testPicker.Unfocused += OnPickerUnfocused;
        }

        void OnPickerFocused(object sender, FocusEventArgs e)
        {
            _pickerStateLabel.Text = "Picker State: Open";
            _pickerStateLabel.TextColor = Colors.Green;
        }

        void OnPickerUnfocused(object sender, FocusEventArgs e)
        {
            _pickerStateLabel.Text = "Picker State: Closed";
            _pickerStateLabel.TextColor = Colors.Red;
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_testPicker.SelectedIndex >= 0)
            {
                _statusLabel.Text = $"Selected: {_testPicker.ItemsSource[_testPicker.SelectedIndex]}";
            }
        }
    }
}
