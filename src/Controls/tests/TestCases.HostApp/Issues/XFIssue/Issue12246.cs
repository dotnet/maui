namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12246, "[Bug] iOS 14 App freezes when password is entered after email", PlatformAffected.iOS)]
public class Issue12246 : TestContentPage
{
	const string Entry = "Entry";
	const string Password = "Password";
	const string Success = "Success";

	protected override void Init()
	{
		var layout = new StackLayout() { Margin = 40, Spacing = 10, VerticalOptions = LayoutOptions.Center };

		var instructions = new Label
		{
			Text = $"Focus the 'Email' Entry. Type in some text. Then focus the 'Password' Entry." +
			$" Type in some text. Now focus the 'Email' Entry again. The '{Success}' Label should appear. " +
			$"If the '{Success}' Label does not appear or the application hangs, this test has failed."
		};

		var result = new Label { Text = Success, IsVisible = false, AutomationId = Success };

		var entry = new Entry()
		{
			Visual = VisualMarker.Material,
			Keyboard = Keyboard.Email,
			Placeholder = "Email",
			TextColor = Colors.Purple,
			AutomationId = Entry
		};

		var password = new Entry
		{
			Visual = VisualMarker.Material,
			IsPassword = true,
			Placeholder = "Password",
			TextColor = Colors.Purple,
			AutomationId = Password
		};

		var passwordConfirmation = new Entry
		{
			Visual = VisualMarker.Material,
			IsPassword = true,
			Placeholder = "Confirm Password",
			TextColor = Colors.Purple
		};

		password.Unfocused += (sender, args) =>
		{
			result.IsVisible = true;
		};

		layout.Children.Add(instructions);
		layout.Children.Add(entry);
		layout.Children.Add(password);
		layout.Children.Add(passwordConfirmation);
		layout.Children.Add(result);

		Content = layout;
	}
}
