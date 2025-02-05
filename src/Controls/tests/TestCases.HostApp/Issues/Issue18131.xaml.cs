namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18131, "Color changes are not reflected in the Rectangle shapes", PlatformAffected.All)]
	public partial class Issue18131 : ContentPage
	{
		public Issue18131()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			RectangleTest.BackgroundColor = EllipseTest.BackgroundColor = Colors.Green;
		}
	}
}