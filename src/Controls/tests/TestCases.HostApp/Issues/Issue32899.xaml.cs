namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32899, "Dotnet bot image is not showing up when using iOS 26 and macOS 26.1", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue32899 : Shell
{
	public Issue32899()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Issue32899Subpage), typeof(Issue32899Subpage));
		GoToAsync(nameof(Issue32899Subpage));
	}

	class Issue32899Subpage : ContentPage
	{
		public Issue32899Subpage()
		{
			Title = "Issue 32899 Subpage";
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "The dotnet bot image should be visible in the title bar above.",
						AutomationId = "InstructionLabel"
					}
				}
			};
		}
	}
}


