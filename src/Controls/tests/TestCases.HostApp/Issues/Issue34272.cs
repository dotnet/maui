using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34272, "ObjectDisposed Exception Closing an App on Windows with style triggers", PlatformAffected.Windows)]
public class Issue34272 : ContentPage
{
	readonly Button _triggerButton;
	readonly Label _statusLabel;

	public Issue34272()
	{
		// Style with a Trigger on IsEnabled — matches the reproduction scenario from the issue
		var buttonStyle = new Style(typeof(Button));
		buttonStyle.Triggers.Add(new Trigger(typeof(Button))
		{
			Property = VisualElement.IsEnabledProperty,
			Value = false,
			Setters =
			{
				new Setter { Property = VisualElement.OpacityProperty, Value = 0.3 },
				new Setter { Property = Button.TextProperty, Value = "Disabled" }
			}
		});

		_triggerButton = new Button
		{
			AutomationId = "TriggerButton",
			Text = "Enabled",
			Style = buttonStyle
		};

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Trigger: Not Fired"
		};

		var toggleButton = new Button
		{
			AutomationId = "ToggleButton",
			Text = "Toggle IsEnabled"
		};
		toggleButton.Clicked += OnToggleClicked;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(16),
			Spacing = 12,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				_triggerButton,
				toggleButton,
				_statusLabel
			}
		};
	}

	void OnToggleClicked(object sender, EventArgs e)
	{
		_triggerButton.IsEnabled = !_triggerButton.IsEnabled;
		_statusLabel.Text = _triggerButton.IsEnabled ? "Trigger: Not Fired" : "Trigger: Fired";
	}
}
