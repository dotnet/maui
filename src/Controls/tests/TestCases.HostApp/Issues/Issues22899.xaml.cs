namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22899, "Title not updated after OnAppearing for TabbedPage in NavigationPage", PlatformAffected.All)]
	public class Issue22899NavPage : NavigationPage
	{
		public Issue22899NavPage() : base(new Issue22899()) { }
	}

	public partial class Issue22899 : TabbedPage
	{
		public Issue22899()
		{
			InitializeComponent();
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Title = "Title has changed";
			label.IsVisible = true;
		}
	}
}