namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17390, "Shell where the bottom padding is not calculated properly when navigating between the tabs.",
		PlatformAffected.iOS)]
	public partial class Issue17390 : Shell
	{
		public Issue17390()
		{
			InitializeComponent();
			Routing.RegisterRoute("nontabbedpage", typeof(NonTabbedPage));
			Routing.RegisterRoute("innertabbedpage", typeof(InnerTabbedPage));
		}
	}
}