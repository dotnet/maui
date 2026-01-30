namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33334, "Password obfuscation causes crash on Windows", PlatformAffected.UWP)]
public class Issue33334 : ContentPage
{
	Entry _passwordEntry;

	public Issue33334()
	{
		var label = new Label
		{
			Text = "Tap button - should not crash",
			AutomationId = "StatusLabel"
		};

		_passwordEntry = new Entry
		{
			IsPassword = true,
			AutomationId = "PasswordEntry"
		};

		var button = new Button
		{
			Text = "Reproduce Crash",
			AutomationId = "ReproButton"
		};
		button.Clicked += OnReproduceClicked;

		Content = new VerticalStackLayout
		{
			Children = { label, _passwordEntry, button }
		};
	}

	void OnReproduceClicked(object sender, EventArgs e)
	{
		// Simulate the crash scenario: empty password + set platform text
		if (_passwordEntry.Handler?.PlatformView is { } platformView)
		{
#if WINDOWS
			((Microsoft.UI.Xaml.Controls.TextBox)platformView).Text = "test";
#elif ANDROID
			((Android.Widget.EditText)platformView).Text = "test";
#elif IOS || MACCATALYST
			((UIKit.UITextField)platformView).Text = "test";
#endif
		}
	}
}
