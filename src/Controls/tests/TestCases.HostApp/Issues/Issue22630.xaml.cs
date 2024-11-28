namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 22630, "ListView Scrolled event is not triggered", PlatformAffected.iOS)]
	public partial class Issue22630 : ContentPage
	{
		public Issue22630()
		{
			InitializeComponent();
		}

		void OnListViewScrolled(System.Object sender, ScrolledEventArgs e)
		{
			TestLabel.Text = "Success";
		}
	}
}