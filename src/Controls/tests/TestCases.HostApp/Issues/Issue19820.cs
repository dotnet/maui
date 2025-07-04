namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19820, "Implicit styling is being ignored on the page level", PlatformAffected.All)]
public class Issue19820 : ContentPage
{
	public Issue19820()
	{
		var application = Application.Current;
		application.Resources = new ResourceDictionary
		{
			new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.FontSizeProperty, Value = 30.0 },
					new Setter { Property = Label.TextColorProperty, Value = Colors.Green },
					new Setter { Property = Label.BackgroundColorProperty, Value = Colors.LightYellow },
				}
			}
		};

		Resources = new ResourceDictionary
		{
			new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red },
				}
			}
		};


		Content = new StackLayout
		{
			Padding = new Thickness(20),
			Spacing = 15,
			Children = {
					new Label
					{
						Text = "The test passes when the Label has a font size of 20, a text color of red, and a background color of yellow.",
						AutomationId = "TestLabel",
					},
				},
		};
	}
}