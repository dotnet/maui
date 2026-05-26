namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "29492", "CharacterSpacing should be applied", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue29492 : TestShell
{
	protected override void Init()
	{
		var shellContent = new ShellContent
		{
			Title = "Home",
			Content = new Issue29492ContentPage() { Title = "Home" }
		};

		Items.Add(shellContent);
	}

	class Issue29492ContentPage : ContentPage
	{
		public Issue29492ContentPage()
		{
			var searchHandler = new SearchHandler
			{
				CharacterSpacing = 10
			};

			var button = new Button
			{
				Text = "Enter Text",
				AutomationId = "Entertext",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			button.Clicked += (s, e) =>
			{
				searchHandler.Query = "Hello World";
			};

			Shell.SetSearchHandler(this, searchHandler);

			Content = button;
		}
	}
}