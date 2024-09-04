namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22452, "Fix error when running new template maui app on iOS", PlatformAffected.iOS)]
	public partial class Issue22452 : Shell
	{
		public Issue22452()
		{
			InitializeComponent();
		}
	}

	public class Issue22452Tab1Content : ContentPage
	{
		public Issue22452Tab1Content()
		{
			Content = new Label()
			{
				AutomationId= "TapOnePage",
				Text = "Tap one page",
			};
		}
	}
}