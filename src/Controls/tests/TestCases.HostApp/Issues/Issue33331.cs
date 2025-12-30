using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33331, "[Android] Picker IsOpen not reset when picker is dismissed", PlatformAffected.Android)]
public class Issue33331 : ContentPage
{
    readonly List<string> _fruits = new() { "Apple", "Banana", "Mango", "Orange", "Pineapple" };
    readonly Picker _testPicker;
    readonly Label _isOpenLabel;

    public Issue33331()
    {
        _testPicker = new Picker
        {
            AutomationId = "TestPicker",
            Title = "Pick a fruit"
        };
        _testPicker.ItemsSource = _fruits;

        var openButton = new Button
        {
            AutomationId = "OpenPickerButton",
            Text = "Open Programmatically"
        };
        openButton.Clicked += OnOpenPicker;

        _isOpenLabel = new Label
        {
            AutomationId = "IsOpenLabel",
            Text = "IsOpen: False",
            FontAttributes = FontAttributes.Bold
        };

        _testPicker.PropertyChanged += OnPickerPropertyChanged;

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 12,
            Children =
                {
                    _testPicker,
                    openButton,
                    _isOpenLabel
                }
        };
    }

    void OnPickerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Picker.IsOpen))
        {
            _isOpenLabel.Text = $"IsOpen: {_testPicker.IsOpen}";
        }
    }

    void OnOpenPicker(object sender, EventArgs e)
    {
        _testPicker.IsOpen = true;
    }
}
