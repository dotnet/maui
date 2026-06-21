namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 28130, "[Windows] Shell Flyout Menu Icon disappears from Window Title Bar after Navigation", PlatformAffected.UWP)]
	public class Issue28130_Shell : TestShell
	{

		protected override void Init()
		{
			Routing.RegisterRoute(nameof(Issue28130_Page1), typeof(Issue28130_Page1));
			FlyoutBehavior = FlyoutBehavior.Flyout;
			FlyoutWidth = 300;
			ItemTemplate = new DataTemplate(() => CreateShellItemTemplate());
			ShellItem shellItem = new ShellItem() { Title = "Issue28130" };
			shellItem.Items.Add(new ShellContent() { Content = new Issue28130_DetailPage("Issue28130") });
			Items.Add(shellItem);
		}

		Button CreateShellItemTemplate()
		{
			var navigateButton = new Button() { Text = "Navigate to Page 2", AutomationId = "NavigateButton" };
			navigateButton.Clicked += NavigateButton_Clicked;
			return navigateButton;
		}

		private void NavigateButton_Clicked(object sender, EventArgs e)
		{
			GoToAsync(nameof(Issue28130_Page1));
		}
	}
}