namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32050, "IconOverride in Shell.BackButtonBehavior does not work.", PlatformAffected.iOS)]
public partial class Issue32050 : Shell
{
	public Issue32050()
	{
		InitializeComponent();
		Routing.RegisterRoute("Issue32050SubPage", typeof(Issue32050SubPage));
		GoToAsync("Issue32050SubPage");
	}

	class Issue32050SubPage : ContentPage
	{
		public Issue32050SubPage()
		{
			Content = new Label { Text = "Label", AutomationId = "Label" };
			SetBackButtonBehavior(this, new BackButtonBehavior { IconOverride = "coffee.png" });
		}
	}
}