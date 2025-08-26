namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 21034, "Child Span does not inherit TextColor or FontAttributes from parent Label", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue21034 : ContentPage
	{
		public Issue21034()
		{
			InitializeComponent();
		}
	}
}