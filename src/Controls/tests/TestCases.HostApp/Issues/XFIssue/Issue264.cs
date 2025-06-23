namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 264, "PopModal NRE", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue264 : TestContentPage
{
	protected override void Init()
	{
		var aboutBtn = new Button
		{
			Text = "About"
		};

		aboutBtn.Clicked += (s, e) => Navigation.PushModalAsync(new AboutPage());

		var popButton = new Button
		{
			Text = "Pop me",
			Command = new Command(async () => await Navigation.PopAsync())
		};

		Content = new StackLayout
		{
			Children = {
				new Label {Text = "Home"},
				aboutBtn,
				popButton
			}
		};
	}
}

public class AboutPage : ContentPage
{
	public AboutPage()
	{
		BackgroundColor = Colors.Bisque;
		Content = new Button { Text = "CloseMe", Command = new Command(() => Navigation.PopModalAsync()) };

	}
}
