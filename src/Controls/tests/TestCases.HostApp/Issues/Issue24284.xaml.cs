namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 24284, "FlyoutHeaderAdaptsToMinimumHeight", PlatformAffected.All)]
public partial class Issue24284 : Shell
{
	public Issue24284()
	{
		InitializeComponent();

		var label = new Label() 
		{ 
			AutomationId = "HeaderLabel",
			Text = "Flyout Header",
			MinimumHeightRequest = 30,
		};

		FlyoutHeader = label;
		FlyoutHeaderBehavior = FlyoutHeaderBehavior.CollapseOnScroll;
		FlyoutIsPresented = true;
	}
}