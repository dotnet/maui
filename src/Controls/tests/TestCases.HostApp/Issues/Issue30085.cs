using System;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30085, "Entry with IsPassword toggling loses previously entered text on iOS when IsPassword is re-enabled", PlatformAffected.iOS)]
	public class Issue30085 : TestContentPage
	{
		public Issue30085()
		{
		}

		protected override void Init()
		{
			var entry = new Entry
			{
				IsPassword = true,
				Placeholder = "Password",
				Background = Brush.Gray,
				ReturnType = ReturnType.Done,
				AutomationId = "passwordEntry"
			};

			var toggleButton = new Button
			{
				Text = "Toggle Password Visibility",
				AutomationId = "toggleButton"
			};

			var resultLabel = new Label
			{
				Text = "Enter text, toggle twice, then type more text. Original text should be preserved.",
				AutomationId = "resultLabel"
			};

			var statusLabel = new Label
			{
				Text = "Status: Ready for testing",
				AutomationId = "statusLabel"
			};

			toggleButton.Clicked += (sender, e) =>
			{
				entry.IsPassword = !entry.IsPassword;
				toggleButton.Text = entry.IsPassword ? "Show Password" : "Hide Password";
				statusLabel.Text = $"Status: Password visibility is {(entry.IsPassword ? "hidden" : "visible")}";
			};

			// Add a text changed handler to show current text for testing
			entry.TextChanged += (sender, e) =>
			{
				resultLabel.Text = $"Current text: '{e.NewTextValue}' (Length: {e.NewTextValue?.Length ?? 0})";
			};

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					resultLabel,
					entry,
					toggleButton,
					statusLabel,
					new Label
					{
						Text = "Test Steps:\n" +
						       "1. Type 'password123' in the entry field\n" +
						       "2. Tap 'Toggle Password Visibility' to show text\n" +
						       "3. Tap 'Toggle Password Visibility' again to hide text\n" +
						       "4. Type '456' at the end\n" +
						       "5. The text should be 'password123456'",
						FontSize = 12,
						TextColor = Colors.Gray
					}
				}
			};
		}
	}
}