namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 18937, "[Windows] Setting IsClippedToBound is true inside a Border control will crash on Windows", PlatformAffected.UWP)]
	public partial class Issue18937 : ContentPage
	{
		public Issue18937()
		{
			InitializeComponent();
		}
	}
}