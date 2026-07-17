namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35513, "Button TextColor does not restore to platform default when reset to null after dynamic update", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35513 : ContentPage
{
	Button _sampleButton;
	Label _sampleLabel;
	Entry _sampleEntry;
	Editor _sampleEditor;
	RadioButton _sampleRadioButton;
	Picker _samplePicker;

	public Issue35513()
	{
		_sampleButton = new Button
		{
			Text = "Button Control"
		};

		_sampleLabel = new Label
		{
			Text = "Label Control"
		};

		_sampleEntry = new Entry
		{
			Text = "Entry Control"
		};

		_sampleEditor = new Editor
		{
			Text = "Editor Control",
			AutoSize = EditorAutoSizeOption.TextChanges
		};

		_sampleRadioButton = new RadioButton
		{
			Content = "RadioButton Control"
		};

		_samplePicker = new Picker
		{
			Title = "Picker Control"
		};
		_samplePicker.Items.Add("First Option");
		_samplePicker.Items.Add("Second Option");
		_samplePicker.SelectedIndex = 0;


		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(24),
				Spacing = 14,
				Children =
				{
					_sampleButton,
					_sampleLabel,
					_sampleEntry,
					_sampleEditor,
					_sampleRadioButton,
					_samplePicker,
					new Button
					{
						Text = "Set Text Color",
						AutomationId = "SetTextColorButton",
						Command = new Command(() =>
						{
							_sampleButton.TextColor = Colors.Orange;
							_sampleLabel.TextColor = Colors.Orange;
							_sampleEntry.TextColor = Colors.Orange;
							_sampleEditor.TextColor = Colors.Orange;
							_sampleRadioButton.TextColor = Colors.Orange;
							_samplePicker.TextColor = Colors.Orange;
						})
					},
					new Button
					{
						Text = "Reset Text Color",
						AutomationId = "ResetTextColorButton",
						Command = new Command(() =>
						{
							_sampleButton.TextColor = null;
							_sampleLabel.TextColor = null;
							_sampleEntry.TextColor = null;
							_sampleEditor.TextColor = null;
							_sampleRadioButton.TextColor = null;
							_samplePicker.TextColor = null;
						})
					}
				}
			}
		};
	}
}
