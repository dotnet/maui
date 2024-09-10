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

	public class Issue22452Content : ContentPage
	{
		public Issue22452Content()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						AutomationId="TabContent",
						Text = "TabContent"
					}
				},

				IgnoreSafeArea = false
			};
		}
	}
}