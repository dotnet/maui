namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19859, "NavigationPage: BarBackgroundColor, BarTextColor and Title not updating", PlatformAffected.Android)]
	public partial class Issue19859 : FlyoutPage
	{
		public Issue19859()
		{
			InitializeComponent();
		}

		private static int _count = 0;

		private void OnClicked(object sender, EventArgs e)
		{
			var oldBarBackgroundColor = NavigationPage.BarBackgroundColor;
			NavigationPage.BarBackgroundColor = oldBarBackgroundColor.Equals(Colors.Yellow)
				? Colors.Red
				: Colors.Yellow;
			var newBarBackgroundColor = NavigationPage.BarBackgroundColor;

			var oldBarTextColor = NavigationPage.BarTextColor;
			NavigationPage.BarTextColor = oldBarTextColor.Equals(Colors.Yellow)
				? Colors.Red
				: Colors.Yellow;
			var newBarTextColor = NavigationPage.BarTextColor;

			var oldTitle = NavigationPage.Title;
			NavigationPage.Title = oldTitle == "Title 1"
				? "Title 2"
				: "Title 1";
			var newTitle = NavigationPage.Title;

			button.Text = $"{_count++}";
		}
	}
}