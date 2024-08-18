namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 24284, "FlyoutHeaderAdaptsToMinimumHeight", PlatformAffected.All)]
public partial class Issue24284 : Shell
{
	public Issue24284()
	{
		InitializeComponent();
		var layout = new VerticalStackLayout()
		{
			AutomationId = "FlyoutHeader",
			BackgroundColor = Colors.Red,
			Children = {
				new Label() { Text = "Flyout Header" }
			},
			MinimumHeightRequest = 30
		};
		FlyoutHeader = layout;
		FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;
		FlyoutIsPresented = true;
	}
}