namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22032, "Shell FlyoutItem Tab Selected Icon Color not changing if using Font icons", PlatformAffected.iOS)]
	public partial class Issue22032 : TabbedPage
	{
		public Issue22032()
		{
			InitializeComponent();
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			CurrentPage = tab2;
		}
	}
}