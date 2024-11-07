namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25679, "When using TabbedPage TabBar is not visible", PlatformAffected.macOS)]
	public partial class Issue25679 : TestTabbedPage
	{
		public Issue25679()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}
	}

	public class Issue25679ContentPage1 : ContentPage
	{
		public Issue25679ContentPage1()
		{
			Content = new Label
			{
				AutomationId = "Issue25679Label",
				Text = "Issue 25679"
			};
		}
	}

	public class Issue25679ContentPage2 : ContentPage
	{

	}

	public class Issue25679ContentPage3 : ContentPage
	{

	}
}